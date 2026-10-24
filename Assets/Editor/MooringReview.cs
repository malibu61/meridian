using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    /// <summary>Opt-in isolated validation. Never writes career completion to the player's storage.</summary>
    public static class MooringReview
    {
        private const string Folder="Logs/MooringReview";private static int checks;
        private static void Check(bool value,string label){checks++;if(!value)throw new InvalidOperationException("Mooring review: "+label);}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        [MenuItem("Meridian/Mooring - Check Rules")]
        public static void CheckRules()
        {
            checks=0;Directory.CreateDirectory(Folder);
            foreach(var id in VesselCatalog.All)foreach(var berth in new[]{HarborBerth.East,HarborBerth.West})
            {
                var v=VesselCatalog.Get(id);var target=MooringRules.Destination(v,berth);int side=berth==HarborBerth.East?1:-1;
                Check(side>0?target.x+v.BeamMetres/8<32.5f:target.x-v.BeamMetres/8>-30.5f,"hull clear of pier cap "+id+berth);
                for(int i=0;i<2;i++)
                {
                    Vector3 p=target+MooringRules.Fairlead(v,i,side),a=MooringRules.ShoreAnchor(v,berth,i);
                    Check(MooringRules.CastEnvelope(p,a,0,0,0),"cast at target "+id+berth+i);
                    Check(!MooringRules.CastEnvelope(p,a,20,0,0)&&!MooringRules.CastEnvelope(p,a,0,1,0),"heading and speed limits "+id+i);
                    Vector3 force=MooringRules.PullAcceleration(p-Vector3.right*side*2,a,Vector3.zero,.5f);
                    Check(Vector3.Dot(force,Vector3.right*side)>0&&force.magnitude<=.0221f&&force.y==0,"tension towards shore, bounded, planar "+id+i);
                }
            }
            Check(MooringRules.PullAcceleration(Vector3.zero,Vector3.right,Vector3.left,2)==Vector3.zero,"slack line never pushes");
            Check(MooringRules.PullAcceleration(Vector3.zero,Vector3.right,Vector3.right*5,.5f)==Vector3.zero,"closing vessel not pulled harder by damping");
            var run=new DockingMissionRun(DockingMissionCatalog.Get(0));run.Begin();for(int i=0;i<3;i++)run.Tick(.1f,DockingMissionCatalog.Gate(i)+Vector3.forward,0,1,0,false,0);
            for(int i=0;i<80;i++)run.Tick(.1f,run.Target,0,0,0,false,0);Check(run.Phase==DockingMissionPhase.Running&&run.HoldSeconds==0,"parking without ropes no longer passes");
            run.SetMooringComplete(true);for(int i=0;i<55;i++)run.Tick(.1f,run.Target,0,0,0,false,0);Check(run.Phase==DockingMissionPhase.Passed,"secured ropes then five seconds passes");
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(2622,1206),new Vector2Int(1024,768)})foreach(bool four in new[]{false,true})
            {
                var l=new HelmLayout(size.x,size.y,new Rect(0,0,size.x,size.y),four);var panel=MooringController.PanelRect(l);
                Check(!panel.Overlaps(l.Rudder)&&!panel.Overlaps(l.Port)&&!panel.Overlaps(l.Starboard)&&!panel.Overlaps(l.Master)&&!panel.Overlaps(l.PortInner)&&!panel.Overlaps(l.StarboardInner)&&!panel.Overlaps(l.Signals),"mooring panel clear of helm "+size+four);
            }
            File.WriteAllText(Folder+"/rules.txt",$"MOORING_RULES_PASS checks={checks}\n");Debug.Log($"MOORING_RULES_PASS checks={checks}");
        }
        [MenuItem("Meridian/Mooring - Run Integration")]
        public static void RunInteractive()=>Run(false);
        public static void RunBatch()=>Run(true);
        private static void Run(bool batch)
        {CheckRules();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.MooringReview",true);SessionState.SetBool("Meridian.MooringBatch",batch);EditorApplication.isPlaying=true;}
        [MenuItem("Meridian/Mooring - Open Visual Demo")]
        public static void OpenDemo()
        {EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.MooringDemo",true);EditorApplication.isPlaying=true;}
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.MooringDemo",false))
            {
                SessionState.SetBool("Meridian.MooringDemo",false);session.StartMode(SimulationMode.Harbor,VesselId.Kumkale,new SeaStateSettings(1,315));
                var target=MooringRules.Destination(session.PlayerShip.Definition,HarborBerth.East);session.PlayerShip.ResetSimulation(target-Vector3.right*1.7f,Quaternion.identity);session.Mooring.RefreshBerth();
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();return;
            }
            if(!SessionState.GetBool("Meridian.MooringReview",false))return;SessionState.SetBool("Meridian.MooringReview",false);
            try
            {
                checks=0;
                var original=session.PlayerShip.gameObject.scene;var isolated=SceneManager.CreateScene("Mooring isolated physics",new CreateSceneParameters(LocalPhysicsMode.Physics3D));var physics=isolated.GetPhysicsScene();
                var report=new System.Text.StringBuilder();
                try
                {
                    foreach(var id in VesselCatalog.All)foreach(var berth in new[]{HarborBerth.East,HarborBerth.West})
                    {
                        SceneManager.MoveGameObjectToScene(session.PlayerShip.gameObject,original);session.StartMode(SimulationMode.Harbor,id,new SeaStateSettings(1,315));
                        var ship=session.PlayerShip;var body=ship.GetComponent<Rigidbody>();var mooring=session.Mooring;
                        ship.enabled=false;ship.GetComponent<ShipBuoyancy>().enabled=false;mooring.enabled=false;
                        if(session.Tugs.Available)session.Tugs.SelectBerth(berth);
                        SceneManager.MoveGameObjectToScene(ship.gameObject,isolated);body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
                        Vector3 target=MooringRules.Destination(ship.Definition,berth);int side=berth==HarborBerth.East?1:-1;
                        ship.ResetSimulation(target-Vector3.right*side*1.7f,Quaternion.identity);mooring.RefreshBerth();
                        Check(mooring.Berth==berth&&mooring.Available,"automatic or tug berth "+id+berth);
                        ship.SetCombinedThrottle(.1f);Check(!mooring.Cast(0),"active engine command refuses cast "+id);ship.EmergencyStop();
                        Check(mooring.Cast(0),"cast bow "+id+berth);Check(!mooring.Cast(1),"sequential crew handling "+id);
                        session.OpenMenu();mooring.SimulateStep(1);Check(mooring.Phase(0)==MooringLinePhase.Throwing&&mooring.Delivered(0)==0,"menu suspends line phase and force "+id);session.Resume();
                        float maxForce=0;bool second=false;float elapsed=0;Vector3 start=body.position;
                        while(elapsed<300&&!mooring.Complete)
                        {
                            const float dt=.05f;ship.SimulateForces(dt);mooring.SimulateStep(dt);physics.Simulate(dt);elapsed+=dt;
                            maxForce=Mathf.Max(maxForce,mooring.Delivered(0),mooring.Delivered(1));
                            if(!second&&mooring.Phase(0)==MooringLinePhase.Secured)second=mooring.Cast(1);
                        }
                        report.AppendLine($"{id} {berth}: second={second} complete={mooring.Complete} seconds={elapsed:0.0} x={body.position.x:0.000} z={body.position.z:0.000} yaw={ship.Heading:0.00} knots={ship.SpeedKnots:0.000} displacement={Vector3.Distance(start,body.position)*4:0.00}m peak_accel={maxForce:0.00000}");
                        File.WriteAllText(Folder+"/physics-detail.txt",report.ToString());
                        Check(second,"second line can be cast after first "+id+berth);Check(maxForce>.0001f&&Vector3.Dot(body.position-start,Vector3.right*side)>.4f,"real shoreward translation from forces "+id+berth);
                        Check(mooring.Complete&&mooring.BothSecured,"two-line physical settle "+id+berth);
                        mooring.Release();Check(!mooring.Complete&&!mooring.Engaged,"release clears hold and ropes "+id);
                        mooring.enabled=true;ship.enabled=true;ship.GetComponent<ShipBuoyancy>().enabled=true;body.constraints=RigidbodyConstraints.None;
                    }
                }
                finally{SceneManager.MoveGameObjectToScene(session.PlayerShip.gameObject,original);SceneManager.UnloadSceneAsync(isolated);}
                session.StartMode(SimulationMode.Harbor,VesselId.Kumkale);var life=Object.FindAnyObjectByType<HarborShoreLife>();Check(life!=null&&life.PatrolCount==16,"sixteen shore patrols");
                var walkers=life.GetComponentsInChildren<HarborCrewMember>();life.Sample(0);Vector3 first=walkers[0].transform.position;life.Sample(10);Check(Vector3.Distance(first,walkers[0].transform.position)>1,"patrol actually walks");
                foreach(var walker in walkers){var mf=walker.GetComponent<MeshFilter>();Check(mf.sharedMesh.vertexCount==576&&walker.PoseFrames>0,"animated bounded low-poly person");Check(walker.transform.position.y>=1.97f,"feet on pier cap");}
                session.StartMode(SimulationMode.StraitPassage,VesselId.Kumkale);Check(!session.Mooring.Available&&!session.Mooring.Engaged,"strait clears mooring");
                session.StartMode(SimulationMode.Harbor,VesselId.Kumkale);Check(session.GetComponents<MooringController>().Length==1,"one service after all vessel switches");
                session.OpenMenu();File.WriteAllText(Folder+"/runtime.txt",$"MOORING_RUNTIME_PASS checks={checks} ships=4 berths=2 physical_cycles=8 shore_patrols=16\n");Debug.Log($"MOORING_RUNTIME_PASS checks={checks}");
                if(SessionState.GetBool("Meridian.MooringBatch",false))EditorApplication.Exit(0);
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);if(SessionState.GetBool("Meridian.MooringBatch",false))EditorApplication.Exit(1);}
        }
    }
}
