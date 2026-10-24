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
    /// <summary>In-memory careers, production force paths, separate physics scenes; no user save writes.</summary>
    public static class MotionRescueReview
    {
        private const string Folder="Logs/MotionRescueReview";
        private static int checks,stage=-1;private static double deadline;
        private sealed class Memory:IMissionProgressStorage
        {private readonly Dictionary<string,string> data=new();public string Read(string key)=>data.TryGetValue(key,out var value)?value:"";public void Write(string key,string value)=>data[key]=value;}
        private static void Check(bool ok,string name){checks++;if(!ok)throw new InvalidOperationException("Motion/rescue: "+name);}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        public static void RunBatch()
        {
            Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/cases.txt","");
            try
            {
                CheckRules();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
                SessionState.SetBool("Meridian.MotionRescueReview",true);EditorApplication.isPlaying=true;
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED rules\n"+ex);Debug.LogException(ex);EditorApplication.Exit(1);}
        }
        public static void CheckRules()
        {
            checks=0;
            foreach(float value in new[]{-30f,-17f,-.05f})Check(PrototypeHud.TravelDirection(value)==-1,"astern direction "+value);
            foreach(float value in new[]{30f,17f,.05f})Check(PrototypeHud.TravelDirection(value)==1,"ahead direction "+value);
            foreach(float value in new[]{-.049f,0,.049f,float.NaN,float.PositiveInfinity})Check(PrototypeHud.TravelDirection(value)==0,"direction deadband "+value);
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(2622,1206),new Vector2Int(1024,768),new Vector2Int(852,393)})
            {
                var l=new HelmLayout(size.x,size.y,new Rect(0,0,size.x,size.y));Rect p=MooringController.PanelRect(l,true);
                Check(!p.Overlaps(l.Signals)&&!p.Overlaps(l.Port)&&!p.Overlaps(l.Master)&&!p.Overlaps(l.Rudder),"combined rope/tug panel clear "+size);
                Check(l.HeaderMetric(0).width>=140,"speed arrow and value fit "+size);
            }
            foreach(var id in VesselCatalog.All)
            {
                var d=DockingMissionCatalog.Get(id,5);var r=new ManOverboardRun(d);
                Check(d.InitialKnots==(id==VesselId.Ruzgar?30:17),"requested MOB speed "+id);
                Check(d.InitialKnots<VesselCatalog.Get(id).MaximumAheadKnots,"below vessel top speed "+id);
                Check(!r.InWater&&!r.Active&&r.FallSeconds>.5f&&r.FallSeconds<3,"starts on deck "+id);
                Check(Vector3.Distance(r.FallPosition(WaterWaves.SeaLevel),r.FallStart)<.001f,"continuous deck start "+id);
                r.Begin();float previous=r.FallStart.y;
                while(!r.InWater)
                {
                    r.Tick(.02f,d.Spawn,0,d.InitialKnots,false,0,0);
                    float y=r.FallPosition(WaterWaves.SeaLevel).y;
                    Check(y<=previous+.0001f&&y>=WaterWaves.SeaLevel-.001f,"continuous descent "+id);previous=y;
                }
                Check(r.Failure==null&&Mathf.Abs(previous-WaterWaves.SeaLevel)<.001f,"water entry without artificial collision "+id);
                Check(Mathf.Abs(r.Casualty.x-d.Spawn.x)>r.Vessel.BeamMetres/8+1.25f,"outside stern propeller exclusion "+id);
            }
            ManOverboardReview.CheckRules();CompactHudReview.CheckRules();
            File.WriteAllText(Folder+"/rules.txt",$"MOTION_RESCUE_RULES_PASS checks={checks} + existing_MOB_fleet_career_HUD_mooring_tug_regressions\n");
        }
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.MotionRescueReview",false))
            {SessionState.SetBool("Meridian.MotionRescueReview",false);session.Missions.SetStorageForReview(new Memory());stage=0;deadline=EditorApplication.timeSinceStartup+180;}
            if(stage<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Stage "+stage);
                if(stage<4){TugCase(session,stage<2?VesselId.Anadolu:VesselId.Barbaros,stage%2==0?HarborBerth.East:HarborBerth.West);stage++;return;}
                if(stage<8){RescueCase(session,VesselCatalog.All[stage-4]);stage++;return;}
                File.WriteAllText(Folder+"/runtime.txt",$"MOTION_RESCUE_RUNTIME_PASS checks={checks} rope_tug_cases=4 moving_fall_cases=4 direction_cases=4 memory_saves_only\n");
                Debug.Log("MOTION_RESCUE_RUNTIME_PASS checks="+checks);stage=-1;EditorApplication.Exit(0);
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED stage="+stage+"\n"+ex);Debug.LogException(ex);stage=-1;EditorApplication.Exit(1);}
        }
        private static void TugCase(GameSessionController session,VesselId id,HarborBerth berth)
        {
            session.StartMode(SimulationMode.Harbor,id,new SeaStateSettings(1,315));
            var ship=session.PlayerShip;var body=ship.GetComponent<Rigidbody>();var mooring=session.Mooring;var tugs=session.Tugs;
            var original=ship.gameObject.scene;var scene=SceneManager.CreateScene("Rope plus tug force "+id+berth,new CreateSceneParameters(LocalPhysicsMode.Physics3D));var physics=scene.GetPhysicsScene();
            ship.enabled=false;ship.GetComponent<ShipBuoyancy>().enabled=false;mooring.enabled=tugs.enabled=false;
            try
            {
                SceneManager.MoveGameObjectToScene(ship.gameObject,scene);body.interpolation=RigidbodyInterpolation.None;
                body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
                int sign=berth==HarborBerth.East?1:-1;Vector3 origin=MooringRules.Destination(ship.Definition,berth)-Vector3.right*(sign*1.7f);
                ship.ResetSimulation(origin,Quaternion.identity);tugs.SelectBerth(berth);mooring.RefreshBerth();
                Check(mooring.Cast(0)&&tugs.Toggle(0)&&tugs.Toggle(1),"cast and independent tug commands "+id+berth);
                bool hauling=false,secured=false;float peak=0;
                for(int n=0;n<1800;n++)
                {
                    ship.SimulateForces(.02f);mooring.SimulateStep(.02f);tugs.SimulateStep(.02f,n*.02f);physics.Simulate(.02f);
                    float delivery=tugs.Delivered(0)+tugs.Delivered(1);peak=Mathf.Max(peak,delivery);
                    if(delivery>.02f&&mooring.Phase(0)==MooringLinePhase.Hauling)hauling=true;
                    if(delivery>.02f&&mooring.Phase(0)==MooringLinePhase.Secured)secured=true;
                }
                Check(hauling&&secured&&peak>.1f,"actual tug force with hauling AND secured first line "+id+berth);
                Check(Vector3.Dot(body.position-origin,Vector3.right*sign)>.4f,"combined forces move ship shoreward "+id+berth);
                tugs.StopAll();Check(tugs.Toggle(0)&&tugs.Toggle(0),"toggle remains usable after first line secured "+id+berth);
                session.OpenMenu();tugs.SimulateStep(.02f,40);Check(tugs.Delivered(0)==0&&tugs.Delivered(1)==0,"menu still blocks forces "+id+berth);session.Resume();
                bool second=false;
                for(int n=0;n<12000&&!mooring.Complete;n++)
                {
                    if(!second)second=mooring.Cast(1);
                    ship.SimulateForces(.02f);mooring.SimulateStep(.02f);tugs.SimulateStep(.02f,40+n*.02f);physics.Simulate(.02f);
                }
                Check(second&&mooring.Complete&&mooring.BothSecured,"two-line settling still completes "+id+berth);
                Check(!tugs.Commanded(0)&&!tugs.Commanded(1)&&!tugs.Toggle(0),"completed mooring stops tugs "+id+berth);
                File.AppendAllText(Folder+"/cases.txt",$"{id} {berth}: force_during_hauling={hauling} force_after_first_secured={secured} peak_delivery={peak:0.000} final_complete={mooring.Complete}\n");
            }
            finally
            {SceneManager.MoveGameObjectToScene(ship.gameObject,original);SceneManager.UnloadSceneAsync(scene);ship.enabled=true;ship.GetComponent<ShipBuoyancy>().enabled=true;mooring.enabled=tugs.enabled=true;body.constraints=RigidbodyConstraints.None;}
        }
        private static void RescueCase(GameSessionController session,VesselId id)
        {
            Check(session.Missions.StartMission(id,5),"real rescue start "+id);
            var ship=session.PlayerShip;var body=ship.GetComponent<Rigidbody>();var controller=session.Missions;var r=controller.Run.Rescue;
            var visual=Object.FindAnyObjectByType<ManOverboardVisual>();
            Check(Mathf.Abs(ship.SignedSpeedKnots-controller.Run.Definition.InitialKnots)<.001f,"actual Rigidbody initial speed "+id);
            Check(ship.GetEngineThrottle(0)>0&&ship.GetEngineSignedLoad(0)>0,"underway engines already loaded "+id);
            Check(!r.InWater&&Vector3.Distance(visual.PersonPosition,r.FallStart)<.001f&&visual.SplashCount==0,"visible person begins on deck "+id);
            Check(controller.BeginMission(),"begin moving fall "+id);controller.enabled=false;ship.enabled=false;ship.GetComponent<ShipBuoyancy>().enabled=false;
            var original=ship.gameObject.scene;var scene=SceneManager.CreateScene("Moving MOB fall "+id,new CreateSceneParameters(LocalPhysicsMode.Physics3D));var physics=scene.GetPhysicsScene();
            try
            {
                SceneManager.MoveGameObjectToScene(ship.gameObject,scene);body.interpolation=RigidbodyInterpolation.None;
                body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
                Vector3 start=body.position,personStart=visual.PersonPosition;bool middle=false;float paused=r.Elapsed;
                session.OpenMenu();visual.SampleVisuals();Check(r.Elapsed==paused&&Vector3.Distance(visual.PersonPosition,personStart)<.001f,"menu pauses fall "+id);session.Resume();
                for(int n=0;n<Mathf.CeilToInt((r.FallSeconds+1)/.02f);n++)
                {
                    ship.SimulateForces(.02f);physics.Simulate(.02f);
                    controller.Run.TickRescue(.02f,body.position,ship.Heading,ship.SpeedKnots,false,0,0);visual.SampleVisuals();
                    if(!middle&&r.FallProgress>=.5f&&!r.InWater)
                    {middle=true;Check(visual.PersonPosition.y<personStart.y&&visual.PersonPosition.z>personStart.z,"tumble descends and retains surge "+id);Capture(ship,visual.PersonPosition,id+"-fall");}
                }
                Check(middle&&r.InWater&&r.Failure==null&&visual.SplashCount==1,"one splash and safe water entry "+id);
                Check(body.position.z>start.z+1,"ship continues underway through incident "+id);
                Check(Mathf.Abs(visual.PersonPosition.y-WaterWaves.SurfaceY(r.Casualty,Time.time)-.025f)<.001f,"casualty floats at rendered water "+id);
                Capture(ship,visual.PersonPosition,id+"-water");
                ship.SetCombinedThrottle(-1);Check(PrototypeHud.TravelDirection(ship.SignedSpeedKnots)==1,"astern propulsion does not fake backwards arrow "+id);
                body.linearVelocity=-ship.ForwardOnWater*.5f;Check(PrototypeHud.TravelDirection(ship.SignedSpeedKnots)==-1,"real astern arrow "+id);
                body.linearVelocity=ship.RightOnWater*.5f;Check(PrototypeHud.TravelDirection(ship.SignedSpeedKnots)==0,"pure sideways drift not mislabeled ahead "+id);
                File.AppendAllText(Folder+"/cases.txt",$"{id}: initial={controller.Run.Definition.InitialKnots}kn fall_seconds={r.FallSeconds:0.00} splash={visual.SplashCount} continued_metres={(body.position.z-start.z)*4:0.00} failure={r.Failure??"none"}\n");
            }
            finally
            {SceneManager.MoveGameObjectToScene(ship.gameObject,original);SceneManager.UnloadSceneAsync(scene);controller.enabled=ship.enabled=true;ship.GetComponent<ShipBuoyancy>().enabled=true;body.constraints=RigidbodyConstraints.None;}
        }
        private static void Capture(ShipController ship,Vector3 person,string name)
        {
            var go=new GameObject("Temporary MOB inspection camera");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.nearClipPlane=.02f;camera.farClipPlane=250;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.18f,.24f);
            Vector3 focus=Vector3.Lerp(person,ship.transform.position+Vector3.up*.3f,.22f);
            int side=person.x>=ship.transform.position.x?1:-1;
            camera.transform.position=focus+new Vector3(side*(ship.Definition.BeamMetres/8+4),4,-5);camera.transform.LookAt(focus);
            var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Folder+"/"+name+".png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(go);}
        }
    }
}
