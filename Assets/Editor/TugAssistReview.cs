using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    /// <summary>Opt-in checks; never starts or changes the user's running editor on domain reload.</summary>
    public static class TugAssistReview
    {
        private const string Folder="Logs/TugAssistReview";
        private static int checks,stage=-1;
        private static bool batch;
        private static double deadline;
        private static void Check(bool ok,string label){checks++;if(!ok)throw new InvalidOperationException("Tug review: "+label);}
        [InitializeOnLoadMethod] private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        [MenuItem("Meridian/Tugs - Check Rules and Hull")]
        public static void CheckSources()
        {
            checks=0;Directory.CreateDirectory(Folder);
            foreach(var vessel in VesselCatalog.All)foreach(var mode in new[]{SimulationMode.Harbor,SimulationMode.StraitPassage,SimulationMode.BuoyMooring})
                Check(TugAssistRules.Available(mode,vessel)==((vessel==VesselId.Anadolu||vessel==VesselId.Barbaros)&&mode==SimulationMode.Harbor),"large-vessel harbour support "+vessel+"/"+mode);
            Check(!TugAssistRules.Valid(HarborBerth.None)&&!TugAssistRules.Valid((HarborBerth)99),"reject unset/invalid berths");
            Check(TugAssistRules.PushSign(Vector3.right,HarborBerth.East)==1&&TugAssistRules.PushSign(Vector3.right,HarborBerth.West)==-1,"opposite side push");
            Check(TugAssistRules.PushSign(Vector3.left,HarborBerth.East)==-1,"ship-relative side");
            Check(TugAssistRules.LocalStation(0,1).x<0&&TugAssistRules.LocalStation(1,-1).x>0,"tugs positioned opposite push");
            Check(TugAssistRules.SpeedFactor(0)==1&&TugAssistRules.SpeedFactor(5)==0&&TugAssistRules.SpeedFactor(8)==0,"speed cutoff");
            Check(TugAssistRules.InArea(new Vector3(0,0,-52),HarborBerth.East)&&TugAssistRules.InArea(new Vector3(0,0,18),HarborBerth.East)&&!TugAssistRules.InArea(new Vector3(0,0,-100),HarborBerth.East),"assistance during approach, including user's 210m-away case");
            Check(!TugAssistRules.Aligned(90)&&TugAssistRules.Aligned(355),"parallel heading gate");
            Check(TugAssistRules.ForceMagnitude(200000,1,0,-1)==0,"never push beyond berth target");
            Check(TugAssistRules.AtBerth(TugAssistRules.Destination(HarborBerth.East),0,0,HarborBerth.East)&&!TugAssistRules.AtBerth(TugAssistRules.Destination(HarborBerth.East),90,0,HarborBerth.East),"berthing requires heading");
            Mesh mesh=HarborTugModel.CreateHull();
            try
            {
                Check(Mathf.Abs(mesh.bounds.size.z-7.5f)<.001f&&Mathf.Abs(mesh.bounds.size.x-2.45f)<.001f,"30 by 9.8 metre game model");
                var vertices=mesh.vertices;var triangles=mesh.triangles;var edges=new Dictionary<string,(int count,int direction)>();double volume=0;
                string Key(Vector3 p)=>$"{Mathf.RoundToInt(p.x*100000)},{Mathf.RoundToInt(p.y*100000)},{Mathf.RoundToInt(p.z*100000)}";
                void Edge(Vector3 a,Vector3 b){string x=Key(a),y=Key(b);int sign=string.CompareOrdinal(x,y)<0?1:-1;string key=sign>0?x+"/"+y:y+"/"+x;edges.TryGetValue(key,out var e);edges[key]=(e.count+1,e.direction+sign);}
                for(int i=0;i<triangles.Length;i+=3)
                {
                    Vector3 a=vertices[triangles[i]],b=vertices[triangles[i+1]],c=vertices[triangles[i+2]];
                    Check(Vector3.Cross(b-a,c-a).sqrMagnitude>1e-12,"nondegenerate hull triangle");volume+=Vector3.Dot(a,Vector3.Cross(b,c))/6;Edge(a,b);Edge(b,c);Edge(c,a);
                }
                foreach(var e in edges.Values)Check(e.count==2&&e.direction==0,"watertight consistently outward hull");
                Check(volume>0,"positive hull volume");
            }
            finally{Object.DestroyImmediate(mesh);}
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2622,1206),new Vector2Int(1024,768)})
            {
                var layout=new HelmLayout(size.x,size.y,new Rect(0,0,size.x,size.y));Rect panel=TugAssistController.PanelRect(layout);
                Check(panel.width>=620&&panel.yMax<layout.Port.y&&panel.yMax<layout.Master.y&&!panel.Overlaps(layout.RollInstrument),"safe-area landscape controls "+size);
            }
            File.WriteAllText(Folder+"/rules.txt",$"TUG_RULES_HULL_PASS checks={checks}\n");Debug.Log($"TUG_RULES_HULL_PASS checks={checks}");
        }
        [MenuItem("Meridian/Tugs - Run Integration Review")]
        public static void RunInteractive(){Run(false);}
        public static void RunBatch(){Run(true);}
        private static void Run(bool quit)
        {
            batch=quit;CheckSources();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            // SessionState survives the transition into Play mode if domain reload is enabled.
            SessionState.SetBool("Meridian.TugReview.Start",true);SessionState.SetBool("Meridian.TugReview.Batch",quit);
            EditorApplication.isPlaying=true;
        }
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            if(SessionState.GetBool("Meridian.TugReview.Start",false))
            {SessionState.SetBool("Meridian.TugReview.Start",false);batch=SessionState.GetBool("Meridian.TugReview.Batch",false);stage=0;checks=0;deadline=EditorApplication.timeSinceStartup+120;Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/physics.txt","");}
            if(stage<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Tug integration stage "+stage);
                var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
                if(stage==0)
                {
                    Check(session.StartMode(SimulationMode.Harbor,VesselId.Anadolu,new SeaStateSettings(1,0)),"start Anadolu harbour");
                    Check(session.Tugs.Available&&session.Tugs.IntentOpen&&!session.CanControl&&session.Tugs.Count==0,"intent before control or tug creation");
                    Check(!session.Tugs.SelectBerth((HarborBerth)99)&&session.Tugs.IntentOpen,"invalid berth keeps modal");
                    Check(session.Tugs.SelectBerth(HarborBerth.East)&&session.CanControl&&session.Tugs.Count==2,"choose east starts fleet");
                    Check(session.Tugs.Toggle(0)&&session.Tugs.Toggle(1),"independent commands");
                    session.OpenMenu();Check(!session.Tugs.Toggle(0)&&!session.CanControl,"menu blocks commands");session.Resume();
                    session.Tugs.RequestIntent();Check(!session.CanControl&&!session.Tugs.Commanded(0)&&!session.Tugs.Commanded(1),"replanning releases tugs");
                    session.Tugs.SelectBerth(HarborBerth.West);Check(session.Tugs.PushSign==-1&&session.Tugs.Count==2,"west replans same two tugs");
                    stage++;return;
                }
                if(stage==1)
                {
                    Check(session.Tugs.GetComponentsInChildren<MeshRenderer>().Length<25,"bounded active tug renderers");
                    CaptureModel(session.Tugs.transform.Find("Anadolu harbour tug assistance/Bow harbour tug"));
                    foreach(var id in VesselCatalog.All)foreach(var mode in new[]{SimulationMode.Harbor,SimulationMode.StraitPassage})
                    {Check(session.StartMode(mode,id),"switch "+id+"/"+mode);bool eligible=TugAssistRules.Available(mode,id);Check(session.Tugs.Available==eligible&&session.Tugs.Count==0&&session.Tugs.IntentOpen==eligible,"no leaked fleet "+id+"/"+mode);}
                    stage++;return;
                }
                if(stage>=2&&stage<=7)
                {int test=stage-2;PhysicsCase(test);stage++;return;}
                if(stage==8)
                {
                    session.StartMode(SimulationMode.Harbor,VesselId.Anadolu,new SeaStateSettings(1,0));
                    File.WriteAllText(Folder+"/runtime.txt",$"TUG_INTEGRATION_PASS checks={checks} physics_cases=6\n");Debug.Log($"TUG_INTEGRATION_PASS checks={checks}");
                    stage=-1;
                    if(batch)EditorApplication.Exit(0);else {var game=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));game.Focus();}
                }
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);stage=-1;if(batch)EditorApplication.Exit(1);}
        }
        private static void PhysicsCase(int test)
        {
            Scene scene=SceneManager.CreateScene("Isolated tug force case "+test,new CreateSceneParameters(LocalPhysicsMode.Physics3D));PhysicsScene physics=scene.GetPhysicsScene();
            var vessel=new GameObject("Tug test ship");SceneManager.MoveGameObjectToScene(vessel,scene);var box=vessel.AddComponent<BoxCollider>();var d=VesselCatalog.Get(VesselId.Anadolu);box.size=d.ColliderSize;box.center=d.ColliderCenter;
            var ship=vessel.AddComponent<ShipController>();ship.ConfigureVessel(d);ship.enabled=false;var rb=ship.GetComponent<Rigidbody>();rb.interpolation=RigidbodyInterpolation.None;rb.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
            HarborBerth berth=test==1?HarborBerth.West:HarborBerth.East;ship.ResetSimulation(new Vector3(test==1?-18:20,.1f,test==4?18:test==5?-52:TugAssistRules.Destination(berth).z),Quaternion.identity);
            if(test>=4)ship.SetCombinedThrottle(.1f);
            var host=new GameObject("Tug test session");SceneManager.MoveGameObjectToScene(host,scene);var session=host.AddComponent<GameSessionController>();session.enabled=false;
            typeof(GameSessionController).GetProperty("HasStarted").SetValue(session,true);
            var assist=host.AddComponent<TugAssistController>();assist.enabled=false;assist.Initialize(session,ship,null);assist.BeginMode(SimulationMode.Harbor,VesselId.Anadolu);assist.SelectBerth(berth);
            try
            {
                assist.Toggle(0);if(test<2||test>=4)assist.Toggle(1);if(test==3){assist.Toggle(0);assist.Toggle(1);}
                Vector3 origin=rb.position;WaterWaves.Reset(new SeaStateSettings(1,0),0);
                for(int i=0;i<1500;i++)
                {
                    ship.SimulateForces(.02f);assist.SimulateStep(.02f,i*.02f);physics.Simulate(.02f);
                    if(i==49)Check(Mathf.Abs(rb.position.x-origin.x)<.001f&&assist.Pressure(0)==0&&assist.Pressure(1)==0,"no force before contact case "+test);
                }
                float dx=rb.position.x-origin.x,yaw=Mathf.DeltaAngle(0,ship.Heading);
                if(test==0)Check(dx>.4f&&Mathf.Abs(yaw)<1,"two port tugs translate starboard without spin");
                if(test==1)Check(dx<-.4f&&Mathf.Abs(yaw)<1,"two starboard tugs translate port without spin");
                if(test==2)Check(dx>.1f&&yaw>1,"bow-only real yaw moment");
                if(test==3)Check(dx>.1f&&yaw<-1,"stern-only opposite yaw moment");
                if(test>=4)Check(dx>.4f&&rb.position.z-origin.z>.25f&&assist.Delivered(0)>.1f&&assist.Delivered(1)>.1f,"real push while progressing, far from final berth case "+test);
                assist.StopAll();Check(!assist.Commanded(0)&&!assist.Commanded(1),"stop all clears command case "+test);
                ship.ResetSimulation(new Vector3(20,.1f,70),Quaternion.identity);rb.linearVelocity=Vector3.forward*(6/(4*1.9438445f));assist.Toggle(0);assist.Toggle(1);
                for(int i=0;i<50;i++){assist.SimulateStep(.02f,i*.02f);physics.Simulate(.02f);}
                Check(Mathf.Abs(rb.position.x-20)<.001f,"no force above five knots case "+test);
                ship.ResetSimulation(new Vector3(20,.1f,70),Quaternion.identity);typeof(GameSessionController).GetProperty("MenuOpen").SetValue(session,true);
                for(int i=0;i<50;i++){assist.SimulateStep(.02f,i*.02f);physics.Simulate(.02f);}
                Check(Mathf.Abs(rb.position.x-20)<.001f&&assist.Delivered(0)==0&&assist.Delivered(1)==0,"no delivered force with menu open case "+test);
                File.AppendAllText(Folder+"/physics.txt",$"case={test} dx_metres={dx*4:F3} yaw_deg={yaw:F3}\n");
            }
            finally{SceneManager.UnloadSceneAsync(scene);}
        }
        private static void CaptureModel(Transform tug)
        {
            Check(tug!=null,"model available for review");
            for(int i=0;i<3;i++)
            {
                var go=new GameObject("Temporary tug render camera");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.11f,.16f,.21f);camera.nearClipPlane=.05f;camera.farClipPlane=60;
                Vector3 focus=tug.position+Vector3.up*1.1f;camera.transform.position=focus+tug.rotation*(Quaternion.Euler(i==2?65:14,i==0?80:225,0)*Vector3.back*12);camera.transform.LookAt(focus);
                var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;
                try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Folder+$"/tug-{i}.png",image.EncodeToPNG());}
                finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(go);}
            }
        }
    }
}
