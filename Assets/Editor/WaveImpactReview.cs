using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class WaveImpactReview
    {
        private const string Folder="Temp/WaveImpactReview";
        private static int stage=-1,checks,samples;
        private static double deadline;
        private static Vector3 origin,pausedPosition;
        private static Quaternion pausedRotation;
        private static float initialHeading,maxYaw,maxDrift,maxForce,maxTorque,withoutYaw,withoutDrift;
        private static int capturedBursts;
        private static double captureAt;
        [InitializeOnLoadMethod] private static void Register(){EditorApplication.update-=Update;EditorApplication.update+=Update;}
        private static void Check(bool ok,string label){checks++;if(!ok){stage=-1;throw new InvalidOperationException("Wave impact review: "+label);}}
        [MenuItem("Meridian/Waves - Run Impact Review")]
        public static void Start()
        {
            checks=0;Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/result.txt","");
            SeaStateSettings saved=WaterWaves.State;
            try
            {
                for(int level=1;level<=5;level++)for(int direction=0;direction<8;direction++)
                {
                    WaterWaves.Reset(new SeaStateSettings(level,direction*45),0);
                    for(int i=0;i<12;i++)
                    {
                        Vector3 p=new(i*1.7f,0,i*.9f);float t=i*.31f,eps=.002f;
                        float derivative=(WaterWaves.Height(p.x,p.z,t+eps)-WaterWaves.Height(p.x,p.z,t-eps))/(2*eps);
                        Check(Mathf.Abs(derivative-WaterWaves.OrbitalVelocity(p,t).y)<.002f,"orbital vertical derivative");
                        float dx=(WaterWaves.Height(p.x+eps,p.z,t)-WaterWaves.Height(p.x-eps,p.z,t))/(2*eps);
                        float dz=(WaterWaves.Height(p.x,p.z+eps,t)-WaterWaves.Height(p.x,p.z-eps,t))/(2*eps);
                        Check(Vector3.Dot(new Vector3(-dx,1,-dz).normalized,WaterWaves.Normal(p.x,p.z,t))>.99999f,"geometry normal derivative");
                    }
                }
                Check(HullWaveImpacts.Strength(0,0,1)==0,"no spray without impact");
                Check(HullWaveImpacts.Strength(1,1,0)==0,"no spray on non-facing side");
                Check(HullWaveImpacts.Strength(.8f,.8f,1)>HullWaveImpacts.Strength(.2f,.2f,1),"stronger impact stronger spray");
            }
            finally{WaterWaves.Reset(saved,Time.time);}
            File.WriteAllText(Folder+"/logic-result.txt",$"WAVE_IMPACT_LOGIC_PASS checks={checks}\n");
            Debug.Log($"WAVE_IMPACT_LOGIC_PASS checks={checks}");
            SessionState.SetBool("Meridian.ImpactReview",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;
        }
        private static void Begin(GameSessionController session)
        {
            Check(session.StartMode(stage==3?SimulationMode.StraitPassage:SimulationMode.Harbor,stage==3?VesselId.Anadolu:VesselId.Barbaros,new SeaStateSettings(stage==0?1:5,45)),"launch "+stage);
            var ship=session.PlayerShip;if(stage==3)ship.ResetSimulation(StraitRoute.Point(0),Quaternion.identity);
            ship.GetComponent<ShipBuoyancy>().HorizontalForcesEnabled=stage!=1;
            origin=ship.transform.position;initialHeading=ship.Heading;maxYaw=maxDrift=maxForce=maxTorque=0;samples=0;capturedBursts=0;
            captureAt=0;deadline=EditorApplication.timeSinceStartup+13;
        }
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.ImpactReview",false))
            {
                SessionState.SetBool("Meridian.ImpactReview",false);stage=0;checks=0;
                Check(session.GetComponent<ModeSelectionMenu>().StableSelectionLayout,"selected/unselected layout parity");
                Check(RenderSettings.skybox.GetFloat("_SunDisk")==0,"no procedural sun disc");
                Check(!ShaderUtil.ShaderHasError(Resources.Load<Shader>("MaritimeOcean")),"ocean shader compiles");
                TestFarWaterHole();Begin(session);return;
            }
            if(stage<0)return;
            var ship=session.PlayerShip;var impacts=ship.GetComponent<HullWaveImpacts>();var body=ship.GetComponent<ShipBuoyancy>();
            if(stage==4)
            {
                if(EditorApplication.timeSinceStartup<deadline)return;
                Check(Vector3.Distance(pausedPosition,ship.transform.position)<.001f&&Quaternion.Angle(pausedRotation,ship.transform.rotation)<.001f,"pause freezes motion");
                stage=-1;File.AppendAllText(Folder+"/result.txt",$"WAVE_IMPACT_PASS checks={checks}\n");Debug.Log($"WAVE_IMPACT_PASS checks={checks}");
                session.GetComponent<ModeSelectionMenu>().SelectSea(session.ActiveSea);session.GetComponent<ModeSelectionMenu>().ShowSeaOptions();return;
            }
            samples++;maxYaw=Mathf.Max(maxYaw,Mathf.Abs(Mathf.DeltaAngle(initialHeading,ship.Heading)));
            maxDrift=Mathf.Max(maxDrift,Vector3.ProjectOnPlane(ship.transform.position-origin,Vector3.up).magnitude);
            maxForce=Mathf.Max(maxForce,body.LastWaveForce.magnitude);maxTorque=Mathf.Max(maxTorque,Mathf.Abs(body.LastWaveTorque.y));
            if(stage>=2&&captureAt==0&&impacts.TotalBursts>capturedBursts+3){captureAt=EditorApplication.timeSinceStartup+.22;capturedBursts=impacts.TotalBursts;}
            if(captureAt>0&&EditorApplication.timeSinceStartup>=captureAt){Capture(session,$"stage{stage}-spray.png",270);captureAt=0;}
            if(EditorApplication.timeSinceStartup<deadline)return;
            Check(samples>30&&float.IsFinite(maxDrift)&&maxDrift<30&&Mathf.Abs(body.RollDegrees)<24,"stable body "+stage);
            Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedShipEmitters,"emitter budget "+stage);
            if(stage>=2){Check(impacts.TotalBursts>0,"real hull impacts "+stage);Check(maxForce>0&&maxTorque>0,"horizontal force and yaw torque "+stage);}
            var ocean=Object.FindAnyObjectByType<OceanSurface>();float minimum=100,maximum=-100;
            foreach(Vector3 vertex in ocean.GetComponent<MeshFilter>().sharedMesh.vertices){minimum=Mathf.Min(minimum,vertex.y);maximum=Mathf.Max(maximum,vertex.y);}
            File.AppendAllText(Folder+"/mesh-diagnostics.txt",$"stage={stage} vertices={ocean.VertexCount} range={maximum-minimum:F4}\n");
            Check(ocean.VertexCount<10000&&maximum-minimum>(stage==0?.08f:1f),"actual wave geometry "+stage+" range="+(maximum-minimum));
            int sprayBudget=0,sprayLive=0;foreach(var ps in ship.GetComponentsInChildren<ParticleSystem>())if(ps.name.Contains("hull spray")){sprayBudget+=ps.main.maxParticles;sprayLive+=ps.particleCount;}
            Check(sprayBudget==HullWaveImpacts.SprayBudget,"bounded spray "+stage);
            File.AppendAllText(Folder+"/result.txt",$"stage={stage} vessel={session.ActiveVessel} level={session.ActiveSea.Level} horizontal={body.HorizontalForcesEnabled} yaw={maxYaw:F4} drift={maxDrift:F4} force={maxForce:F4} yawTorque={maxTorque:F4} bursts={impacts.TotalBursts} port={impacts.PortBursts} starboard={impacts.StarboardBursts} meshRange={maximum-minimum:F3} liveSpray={sprayLive} globalSea={Shader.GetGlobalVector("_MeridianSea")} globalAmplitude={Shader.GetGlobalFloat("_MeridianAmplitude")}\n");
            if(stage==1){withoutYaw=maxYaw;withoutDrift=maxDrift;}
            if(stage==2)
            {
                Check(maxYaw>withoutYaw+.02f&&maxDrift>withoutDrift+.02f,"Barbaros actual heading/track response");
                Capture(session,"barbaros-bow.png",15);Capture(session,"barbaros-quarter.png",135);Capture(session,"barbaros-port.png",270);
            }
            if(stage==3)
            {
                var wake=ship.GetComponent<WakeTrail>();Material foam=null;
                foreach(var ps in ship.GetComponentsInChildren<ParticleSystem>())if(ps.name=="Port propeller wake")foam=ps.GetComponent<ParticleSystemRenderer>().sharedMaterial;
                wake.Configure(ship,foam);wake.Configure(ship,foam);
                Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedShipEmitters&&ship.GetComponents<HullWaveImpacts>().Length==1,"idempotent configure");
                ship.ResetSimulation(ship.transform.position,ship.transform.rotation);
                foreach(var ps in ship.GetComponentsInChildren<ParticleSystem>())Check(ps.particleCount==0,"mode reset clears particles");
                session.OpenMenu();pausedPosition=ship.transform.position;pausedRotation=ship.transform.rotation;stage=4;deadline=EditorApplication.timeSinceStartup+.6;return;
            }
            stage++;Begin(session);
        }
        private static void TestFarWaterHole()
        {
            GameObject plane=GameObject.CreatePrimitive(PrimitiveType.Plane),go=new("Temporary far-water clipping test");plane.layer=30;plane.transform.localScale=Vector3.one;
            Material mat=new(Resources.Load<Shader>("MaritimeOcean"));mat.SetFloat("_FarWater",1);plane.GetComponent<Renderer>().sharedMaterial=mat;
            Camera camera=go.AddComponent<Camera>();camera.enabled=false;camera.transform.SetPositionAndRotation(new Vector3(0,10,0),Quaternion.Euler(90,0,0));camera.orthographic=true;camera.orthographicSize=5;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.magenta;
            Vector4 patch=Shader.GetGlobalVector("_MeridianPatch");RenderTexture previous=RenderTexture.active;RenderTexture rt=new(128,128,24);Texture2D texture=new(128,128,TextureFormat.RGB24,false);
            try
            {
                Shader.SetGlobalVector("_MeridianPatch",new Vector4(0,0,2,2));camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,128,128),0,0);texture.Apply();
                Color center=texture.GetPixel(64,64),outer=texture.GetPixel(12,12);
                Check(center.r>.9f&&center.b>.9f&&center.g<.1f,"far plane does not fill near-wave troughs");
                Check(outer.g>.05f&&outer.r<.8f,"far plane remains visible outside patch");
            }
            finally{Shader.SetGlobalVector("_MeridianPatch",patch);camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(go);Object.DestroyImmediate(plane);Object.DestroyImmediate(mat);}
        }
        private static void Capture(GameSessionController session,string name,float azimuth)
        {
            ShipController ship=session.PlayerShip;GameObject go=new("Temporary wave impact review");Camera camera=go.AddComponent<Camera>();camera.CopyFrom(Object.FindAnyObjectByType<FollowCamera>().GetComponent<Camera>());camera.enabled=false;
            Vector3 focus=ship.transform.position+Vector3.up*ship.Definition.CameraFocusHeight;
            camera.transform.position=focus+Quaternion.Euler(0,ship.Heading,0)*(Quaternion.Euler(13,azimuth,0)*Vector3.back*(38*ship.Definition.CameraScale));camera.transform.LookAt(focus);
            RenderTexture previous=RenderTexture.active;RenderTexture rt=new(1280,720,24);Texture2D image=new(1280,720,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Folder+"/"+name,image.EncodeToPNG());
                string report="";foreach(var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(renderer.sharedMaterial!=null&&renderer.sharedMaterial.shader.name=="Meridian/Layered Ocean")report+=$"{renderer.name} far={renderer.sharedMaterial.GetFloat("_FarWater")} position={renderer.transform.position} visible={renderer.isVisible}\n";
                File.WriteAllText(Folder+"/render-diagnostics.txt",report);
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(go);}
        }
    }
}
