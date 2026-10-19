using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Meridian.Editor
{
    public static class EngineExhaustReview
    {
        private static double nextCheck;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.delayCall+=RunChecks;
            EditorApplication.update-=RunRequested;EditorApplication.update+=RunRequested;
        }

        [MenuItem("Meridian/Engine Exhaust - Check Response")]
        public static void RunChecks()
        {
            int checks=0;
            void Check(bool pass,string name){checks++;if(!pass)throw new InvalidOperationException("Exhaust check failed: "+name);}
            EngineExhaustResponse calm=new(),hard=new(),other=new();
            for(int i=0;i<100;i++)calm.Step((i+1)/100f,(i+1)/100f,.02f);
            Check(calm.Surge<.001f,"gentle throttle does not trigger acceleration smoke");
            Check(calm.EmissionRate>.8f,"load increases steady exhaust");
            hard.Step(1f,0f,.02f);
            float peak=hard.EmissionRate;
            Check(hard.Surge>.9f,"abrupt full throttle pulse");
            Check(hard.EmissionRate>calm.EmissionRate*3f,"pulse is much denser");
            Check(hard.Darkness>calm.Darkness+.4f,"pulse is darker");
            Check(hard.UpwardSpeed>calm.UpwardSpeed,"pulse exits faster");
            Check(other.Surge==0f,"independent engines");
            float surge=hard.Surge;hard.Step(1f,.1f,.02f);Check(hard.Surge<surge,"holding throttle does not retrigger");
            for(int i=0;i<250;i++)hard.Step(1f,1f,.02f);
            Check(hard.Surge<.001f && hard.EmissionRate<peak*.4f,"pulse settles at full power");
            hard.Step(0f,1f,.02f);Check(hard.Surge==0f,"reducing throttle is not a load surge");
            for(int i=0;i<250;i++)hard.Step(0f,0f,.02f);
            Check(hard.EmissionRate<.81f,"idle is faint");
            hard.Step(-1f,0f,.02f);Check(hard.Surge>.9f,"astern load also emits");
            hard.Reset(1f,1f);hard.Step(-1f,1f,.02f);Check(hard.Surge>.5f,"hard reversal pulse");
            hard.Reset(.8f,.8f);hard.Step(.8f,.8f,.02f);Check(hard.Surge==0f,"initial binding is not a throttle jump");
            hard.Reset();hard.Step(1f,1f,0f);Check(hard.Surge==0f,"paused frame");
            hard.Step(float.NaN,1f,.02f);hard.Step(1f,float.PositiveInfinity,.02f);Check(hard.EmissionRate<1f,"invalid input ignored");
            hard.Step(100f,100f,.02f);Check(hard.EmissionRate<81f && hard.Opacity<=.5451f,"input and particle budget bounded");
            Debug.Log($"ENGINE_EXHAUST_CHECK_PASS checks={checks}");
        }

        private static void RunRequested()
        {
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup<nextCheck)return;
            nextCheck=EditorApplication.timeSinceStartup+.5;
            const string request="Temp/EngineExhaustReview/request.txt";
            if(!File.Exists(request))return;
            string token=File.ReadAllText(request).Trim();
            if(token.Length==0 || SessionState.GetString("Meridian.ExhaustReview.Token","")==token)return;
            EngineExhaustSmoke live=UnityEngine.Object.FindAnyObjectByType<EngineExhaustSmoke>();
            if(live==null || !live.IsBoundToShip)return;
            SessionState.SetString("Meridian.ExhaustReview.Token",token);
            ShipController controller=live.GetComponentInParent<ShipController>();
            Debug.Log($"EXHAUST_LIVE_BINDING_PASS {token} portCommand={controller.PortThrottle:F2} starboardCommand={controller.StarboardThrottle:F2} portRate={live.Port.EmissionRate:F2} starboardRate={live.Starboard.EmissionRate:F2} portSurge={live.Port.Surge:F2} starboardSurge={live.Starboard.Surge:F2} particles={live.ParticleCount}");
            if(token.StartsWith("live-",StringComparison.Ordinal))CaptureLive(live,token);else CaptureStudy();
        }

        private static void CaptureLive(EngineExhaustSmoke smoke,string token)
        {
            Transform ship=smoke.GetComponentInParent<ShipController>().transform;
            GameObject go=new("Temporary live exhaust review camera");Camera camera=go.AddComponent<Camera>();camera.enabled=false;
            camera.fieldOfView=40f;camera.nearClipPlane=.1f;camera.farClipPlane=150f;camera.clearFlags=CameraClearFlags.Skybox;
            Vector3 focus=ship.TransformPoint(new Vector3(0f,5f,-1f));
            camera.transform.position=focus+ship.rotation*new Vector3(-10f,2f,-7f);camera.transform.LookAt(focus);
            RenderTexture rt=new(1280,720,24),previous=RenderTexture.active;
            Texture2D texture=new(1280,720,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
                File.WriteAllBytes(Path.GetFullPath("Temp/EngineExhaustReview/"+token.Replace("/","").Replace("\\","")+".png"),texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [MenuItem("Meridian/Engine Exhaust - Capture Study")]
        public static void CaptureStudy()
        {
            if(!EditorApplication.isPlaying){Debug.LogWarning("Enter Play mode for the exhaust study.");return;}
            string folder=Path.GetFullPath("Temp/EngineExhaustReview");Directory.CreateDirectory(folder);
            GameObject fixture=new("Temporary exhaust review fixture");fixture.transform.position=new Vector3(70f,0f,-70f);
            ShipController controller=fixture.AddComponent<ShipController>();controller.enabled=false;
            fixture.GetComponent<Rigidbody>().isKinematic=true;
            Transform visuals=new GameObject("Visuals").transform;visuals.SetParent(fixture.transform,false);
            GameObject cameraObject=new("Exhaust review camera");Camera camera=cameraObject.AddComponent<Camera>();camera.enabled=false;
            camera.fieldOfView=40f;camera.nearClipPlane=.1f;camera.farClipPlane=150f;camera.clearFlags=CameraClearFlags.Skybox;
            RenderTexture rt=new(1280,720,24);RenderTexture previous=RenderTexture.active;
            Texture2D contact=new(2560,360,TextureFormat.RGB24,false);
            try
            {
                new BarbarosModelBuilder(visuals).Build();
                EngineExhaustSmoke smoke=fixture.GetComponentInChildren<EngineExhaustSmoke>();
                if(smoke==null)throw new InvalidOperationException("Exhaust component missing from generated ship.");
                camera.targetTexture=rt;
                Vector3 focus=fixture.transform.position+new Vector3(0f,5.0f,-1.0f);
                camera.transform.position=focus+new Vector3(-10f,2f,-7f);camera.transform.LookAt(focus);
                string[] names={"01-idle","02-cruise","03-sudden-full","04-settled-full"};
                for(int view=0;view<4;view++)
                {
                    float initial=view==1?.5f:0f;smoke.ResetStudy(initial,initial,initial,initial);
                    for(int step=0;step<80;step++)smoke.StepStudy(initial,initial,initial,initial,.05f);
                    if(view>=2)
                    {
                        float actual=0f;int steps=view==2?14:160;
                        for(int step=0;step<steps;step++){actual=Mathf.MoveTowards(actual,1f,.45f*.05f);smoke.StepStudy(1f,actual,1f,actual,.05f);}
                    }
                    camera.Render();RenderTexture.active=rt;
                    if(view>0 && smoke.ParticleCount<20)throw new InvalidOperationException("Exhaust particles did not remain active in "+names[view]);
                    Texture2D capture=new(1280,720,TextureFormat.RGB24,false);capture.ReadPixels(new Rect(0,0,1280,720),0,0);capture.Apply();
                    File.WriteAllBytes(Path.Combine(folder,names[view]+".png"),capture.EncodeToPNG());
                    Color[] thumbnail=new Color[640*360];for(int y=0;y<360;y++)for(int x=0;x<640;x++)thumbnail[y*640+x]=capture.GetPixel(x*2,y*2);
                    contact.SetPixels(view*640,0,640,360,thumbnail);UnityEngine.Object.DestroyImmediate(capture);
                    Debug.Log($"EXHAUST_STUDY {names[view]} rate={smoke.Port.EmissionRate:F2} surge={smoke.Port.Surge:F3} particles={smoke.ParticleCount}");
                }
                contact.Apply();File.WriteAllBytes(Path.Combine(folder,"comparison.png"),contact.EncodeToPNG());
                Debug.Log("ENGINE_EXHAUST_VISUAL_PASS "+folder);
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=previous;rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(contact);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(fixture);
            }
        }
    }
}
