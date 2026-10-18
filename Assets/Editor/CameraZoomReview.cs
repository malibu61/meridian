using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Meridian.Editor
{
    public static class CameraZoomReview
    {
        private static double nextCheck;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.delayCall += RunChecks;
            EditorApplication.update -= SnapshotRequested;
            EditorApplication.update += SnapshotRequested;
        }

        [MenuItem("Meridian/Camera Zoom - Check Input")]
        public static void RunChecks()
        {
            int checks=0;
            void Check(bool condition,string name){checks++;if(!condition)throw new InvalidOperationException("Zoom check failed: "+name);}
            bool Near(float a,float b)=>Mathf.Abs(a-b)<.001f;
            OrbitZoomInput z=new();
            z.ApplyWheel(1f);Check(z.Distance<34f,"wheel zoom in");
            z.ApplyWheel(-1f);Check(Near(z.Distance,34f),"wheel zoom out / inverse");
            z.ApplyWheel(1000f);Check(Near(z.Distance,6f),"near clamp");
            z.ApplyWheel(-1000f);Check(Near(z.Distance,60f),"far clamp");
            z.ApplyWheel(float.NaN);z.ApplyWheel(float.PositiveInfinity);Check(Near(z.Distance,60f),"invalid scroll ignored");
            z.Reset();Check(Near(z.Distance,34f),"reset");
            void Pinch(int a,int b,float gap,bool allowed=true)=>z.UpdatePinch(a,Vector2.zero,b,Vector2.right*gap,allowed);
            Pinch(1,2,100f);Check(Near(z.Distance,34f),"new fingers do not jump");
            Pinch(1,2,200f);Check(Near(z.Distance,17f),"spread to zoom in");
            Pinch(1,2,100f);Check(Near(z.Distance,34f),"pinch to zoom out");
            Pinch(1,2,100f);Check(Near(z.Distance,34f),"stationary fingers");
            z.UpdatePinch(2,Vector2.right*100f,1,Vector2.zero,true);Check(Near(z.Distance,34f),"touch array reordering");
            Pinch(1,3,400f);Check(Near(z.Distance,34f),"replaced finger rebases");
            Pinch(1,3,800f,false);Check(Near(z.Distance,17f),"captured pinch can cross HUD");
            z.EndPinch();Pinch(1,2,100f,false);Pinch(1,2,200f,true);
            Check(Near(z.Distance,17f) && !z.IsPinching,"HUD-start gesture stays blocked");
            z.EndPinch();Pinch(1,2,100f);Pinch(1,2,200f);Check(Near(z.Distance,8.5f),"release starts a fresh pinch");
            z.Reset();Pinch(1,2,0f);Pinch(1,2,200f);Check(Near(z.Distance,34f),"zero gap cannot divide / jump");
            Pinch(1,2,400f);Check(Near(z.Distance,17f),"gesture recovers after zero gap");
            Pinch(1,2,100000f);Check(Near(z.Distance,6f),"pinch near clamp");
            Pinch(1,2,8f);Check(Near(z.Distance,60f),"pinch far clamp");
            z.Reset();Pinch(1,2,100f);Pinch(1,2,125f);float scaled=z.Distance;
            z.Reset();Pinch(1,2,400f);Pinch(1,2,500f);Check(Near(z.Distance,scaled),"resolution-independent ratio");
            z.EndPinch();Check(!z.IsPinching,"cancel gesture");
            Pinch(1,1,200f);Check(!z.IsPinching,"same finger cannot pinch");
            z.UpdatePinch(1,new Vector2(float.NaN,0f),2,Vector2.one,true);Check(!z.IsPinching,"invalid touch ignored");
            Debug.Log($"CAMERA_ZOOM_CHECK_PASS checks={checks} range=6..60");
        }

        private static void SnapshotRequested()
        {
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup<nextCheck)return;
            nextCheck=EditorApplication.timeSinceStartup+.5;
            const string folder="Temp/CameraZoomReview",request=folder+"/request.txt";
            if(!File.Exists(request))return;
            string token=File.ReadAllText(request).Trim();
            if(token.Length==0 || token.IndexOfAny(Path.GetInvalidFileNameChars())>=0 || SessionState.GetString("Meridian.ZoomReview.Token","")==token)return;
            FollowCamera follow=UnityEngine.Object.FindAnyObjectByType<FollowCamera>();
            if(follow==null)return;
            SessionState.SetString("Meridian.ZoomReview.Token",token);
            Camera camera=follow.GetComponent<Camera>();
            RenderTexture previous=RenderTexture.active,previousTarget=camera.targetTexture;
            RenderTexture rt=new(1280,720,24);Texture2D capture=new(1280,720,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                capture.ReadPixels(new Rect(0,0,1280,720),0,0);capture.Apply();
                File.WriteAllBytes(Path.Combine(folder,token+".png"),capture.EncodeToPNG());
                Debug.Log($"CAMERA_ZOOM_STATE {token} requested={follow.RequestedDistance:F3} current={follow.CurrentDistance:F3}");
            }
            finally
            {
                camera.targetTexture=previousTarget;RenderTexture.active=previous;
                UnityEngine.Object.DestroyImmediate(capture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
