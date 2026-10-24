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
    /// <summary>Fleet career uses in-memory saves. Force cases run the real controllers in a separate physics scene.</summary>
    public static class FleetCareerReview
    {
        private const string Folder="Logs/FleetCareerReview";private static int checks;
        private sealed class Memory:IMissionProgressStorage
        {public readonly Dictionary<string,string> Data=new();public string Read(string key)=>Data.TryGetValue(key,out var text)?text:"";public void Write(string key,string text)=>Data[key]=text;}
        private static void Check(bool value,string label){checks++;if(!value)throw new InvalidOperationException("Fleet review: "+label);}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        [MenuItem("Meridian/Fleet - Check Mission Rules")]
        public static void CheckRules()
        {
            checks=0;Directory.CreateDirectory(Folder);var ids=new HashSet<string>();var memory=new Memory();
            Check(DockingMissionCatalog.TotalCount==24,"twenty docking and four rescue missions");
            foreach(var vessel in VesselCatalog.All)
            {
                var progress=new MissionProgressStore(memory,vessel);Check(progress.CompletedCount==0&&progress.IsUnlocked(0)&&!progress.IsUnlocked(1),"independent initial locks "+vessel);
                for(int i=0;i<5;i++)
                {
                    var def=DockingMissionCatalog.Get(vessel,i);Check(def.Vessel==vessel&&ids.Add(def.Id)&&DockingMissionCatalog.IndexOf(vessel,def.Id)==i,"stable unique fleet ID "+def.Id);
                    Check(def.Spawn.z<-100&&def.InitialKnots<=4&&def.HarborSpeedLimit>=5,"exterior profile "+def.Id);
                    Check(progress.Start(i)&&progress.Complete(i,1,100+i),"one-star chain "+def.Id);
                    var reload=new MissionProgressStore(memory,vessel);Check(reload.IsCompleted(i)&&reload.CompletedCount==i+1,"saved ship progression "+def.Id);
                    var run=new DockingMissionRun(def);run.Begin();FeedGates(run);run.SetMooringComplete(true);
                    if(def.RequiresTugs||def.IndependentEngines)
                    {for(int n=0;n<60;n++)run.Tick(.1f,run.Target,0,0,0,false,0);Check(run.Phase==DockingMissionPhase.Running,"cannot skip required work "+def.Id);}
                    Fulfill(run);for(int n=0;n<55;n++)run.Tick(.1f,run.Target,0,0,0,false,0);
                    Check(run.Phase==DockingMissionPhase.Passed,"all requirements then settle "+def.Id);
                }
                Check(new MissionProgressStore(memory,vessel).CompletedCount==5,"five persisted "+vessel);
            }
            Check(memory.Data.Count==4,"four separate save keys");
            var legacy=new Memory();legacy.Data["Meridian.Career.v1.Kumkale"]="{\"version\":1,\"lastMissionId\":\"kumkale.port.v1\",\"entries\":[{\"id\":\"kumkale.starboard.v1\",\"completed\":true,\"stars\":2,\"bestSeconds\":91}]}";
            var old=new MissionProgressStore(legacy);Check(old.CompletedCount==1&&old.ContinueIndex==1&&old.Stars(0)==2&&old.BestSeconds(0)==91,"existing Kumkale save preserved without migration loss");
            Check(new MissionProgressStore(legacy,VesselId.Barbaros).CompletedCount==0,"Kumkale does not unlock frigate");
            foreach(var vessel in new[]{VesselId.Barbaros,VesselId.Anadolu})foreach(var berth in new[]{HarborBerth.East,HarborBerth.West})
            {
                var v=VesselCatalog.Get(vessel);Check(TugAssistRules.Available(SimulationMode.Harbor,vessel)&&!TugAssistRules.Available(SimulationMode.StraitPassage,vessel),"heavy-vessel tug scope "+vessel);
                for(int i=0;i<2;i++){var p=TugAssistRules.LocalStation(i,berth==HarborBerth.East?1:-1,v);Check(Mathf.Abs(p.z)<v.LengthMetres/8&&Mathf.Abs(p.x)>v.BeamMetres/8,"own-hull contact station "+vessel+berth+i);}
            }
            var tugRun=new DockingMissionRun(DockingMissionCatalog.Get(VesselId.Barbaros,2));tugRun.Begin();FeedGates(tugRun);
            for(int n=0;n<40;n++)tugRun.RecordTugDelivery(.1f,0,0,tugRun.Berth);Check(!tugRun.TugWorkReady&&tugRun.SoloTugSeconds==0,"commands/zero delivery do not count");
            for(int n=0;n<40;n++)tugRun.RecordTugDelivery(.1f,1,1,tugRun.Berth);Check(!tugRun.TugWorkReady&&tugRun.SoloTugSeconds==0,"both together cannot substitute solo lesson");
            for(int n=0;n<40;n++)tugRun.RecordTugDelivery(.1f,1,0,HarborBerth.West);Check(tugRun.SoloTugSeconds==0,"wrong-side support does not count");
            Fulfill(tugRun);Check(tugRun.TugWorkReady,"solo then both lesson passes");
            var reassigned=new DockingMissionRun(DockingMissionCatalog.Get(VesselId.Anadolu,4));reassigned.Begin();reassigned.Tick(.1f,DockingMissionCatalog.Gate(0)+Vector3.forward,0,1,0,false,0);Fulfill(reassigned);Check(reassigned.TugWorkReady,"pre-change evidence fixture");
            reassigned.Tick(.1f,DockingMissionCatalog.Gate(1)+Vector3.forward,0,1,0,false,0);Check(reassigned.TargetChanged&&reassigned.Berth==HarborBerth.West&&!reassigned.TugWorkReady,"new berth requires fresh delivered support");
            var independent=new DockingMissionRun(DockingMissionCatalog.Get(VesselId.Ruzgar,1));independent.Begin();FeedGates(independent);
            for(int n=0;n<40;n++)independent.RecordEngineHandling(.1f,.1f,true);Check(independent.IndependentEngineSeconds==0,"synchronized/near-equal loads insufficient");
            for(int n=0;n<40;n++)independent.RecordEngineHandling(.1f,1,false);Check(independent.IndependentEngineSeconds==0,"not counted outside low-speed approach");
            Fulfill(independent);Check(independent.ApproachWorkReady,"independent inner/outer work");
            File.WriteAllText(Folder+"/rules.txt",$"FLEET_RULES_PASS checks={checks} missions={ids.Count} vessel_namespaces=4 legacy_kumkale_preserved\n");Debug.Log($"FLEET_RULES_PASS checks={checks}");
        }
        public static void FeedGates(DockingMissionRun run)
        {for(int i=0;i<3;i++)run.Tick(.1f,DockingMissionCatalog.Gate(i)+Vector3.forward,0,1,0,false,0);}
        public static void Fulfill(DockingMissionRun run)
        {
            if(run.Definition.TugExercise==TugTraining.BowSolo)for(int i=0;i<32;i++)run.RecordTugDelivery(.1f,1,0,run.Berth);
            if(run.Definition.TugExercise==TugTraining.SternSolo)for(int i=0;i<32;i++)run.RecordTugDelivery(.1f,0,1,run.Berth);
            for(int i=0;i<32;i++){run.RecordTugDelivery(.1f,1,1,run.Berth);run.RecordEngineHandling(.1f,.5f,true);}
        }
        public static void RunBatch()=>Run(true);
        [MenuItem("Meridian/Fleet - Run Mission and Tug Integration")]
        public static void RunInteractive()=>Run(false);
        private static void Run(bool batch)
        {CheckRules();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.FleetReview",true);SessionState.SetBool("Meridian.FleetBatch",batch);EditorApplication.isPlaying=true;}
        [MenuItem("Meridian/Fleet - Open Careers")]
        public static void OpenDemo()
        {EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.FleetDemo",true);EditorApplication.isPlaying=true;}
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.FleetDemo",false)){SessionState.SetBool("Meridian.FleetDemo",false);session.GetComponent<ModeSelectionMenu>().ShowMissionOptions(VesselId.Barbaros);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();return;}
            if(!SessionState.GetBool("Meridian.FleetReview",false))return;SessionState.SetBool("Meridian.FleetReview",false);
            try
            {
                checks=0;var memory=new Memory();session.Missions.SetStorageForReview(memory);
                foreach(var vessel in VesselCatalog.All)
                {
                    Check(!session.Missions.StartMission(vessel,1),"ship-specific lock before reset "+vessel);
                    for(int i=0;i<5;i++)
                    {
                        Check(session.Missions.StartMission(vessel,i),"correct unlocked mission "+vessel+i);var run=session.Missions.Run;
                        Check(session.ActiveVessel==vessel&&session.PlayerShip.Definition.Id==vessel&&!session.CanControl&&Time.timeScale==0,"correct vessel and briefing "+vessel+i);
                        Check(session.Tugs.Available==run.Definition.RequiresTugs&&!session.Tugs.IntentOpen,"mission tug setup without extra intent popup "+vessel+i);
                        if(run.Definition.RequiresTugs){session.Tugs.RequestIntent();Check(!session.Tugs.IntentOpen&&!session.Tugs.SelectBerth(run.Berth==HarborBerth.East?HarborBerth.West:HarborBerth.East),"fixed mission berth cannot be cheated");}
                        Check(session.Audio.LoadRadio(run.Definition.RadioId)!=null,"own mission voice "+run.Definition.RadioId);
                        session.Missions.BeginMission();Check(session.CanControl,"acknowledge all vessel briefings "+vessel+i);
                        FeedGates(run);Fulfill(run);run.SetMooringComplete(true);for(int n=0;n<55;n++)run.Tick(.1f,run.Target,0,0,0,false,0);session.Missions.RecordResult();
                        Check(run.Phase==DockingMissionPhase.Passed&&session.Missions.Progress.Vessel==vessel&&session.Missions.Progress.IsCompleted(i),"record correct ship result "+vessel+i);
                    }
                }
                foreach(var vessel in VesselCatalog.All)Check(new MissionProgressStore(memory,vessel).CompletedCount==5,"reload all ship careers "+vessel);
                foreach(var id in new[]{"fleet_entry","fleet_approach","fleet_reassign","fleet_signal_1","fleet_signal_2","fleet_signal_3","fleet_success","fleet_career_complete","fleet_failure"})
                {
                    var clip=session.Audio.LoadRadio(id);Check(clip!=null&&clip.channels==1&&clip.length>1,"generic fleet radio loads "+id);Check(!MaritimeAudio.RadioText(id).Contains("Kumkale"),"no incorrect Kumkale address "+id);
                    float[] data=new float[clip.samples];Check(clip.GetData(data,0),"fleet radio PCM "+id);double power=0;foreach(float value in data)power+=value*value;Check(Math.Sqrt(power/data.Length)>.005,"fleet radio audible "+id);
                }
                var original=session.PlayerShip.gameObject.scene;var isolated=SceneManager.CreateScene("Fleet tug physics",new CreateSceneParameters(LocalPhysicsMode.Physics3D));var physics=isolated.GetPhysicsScene();
                var report=new System.Text.StringBuilder();
                try
                {
                    foreach(var vessel in new[]{VesselId.Barbaros,VesselId.Anadolu})foreach(var berth in new[]{HarborBerth.East,HarborBerth.West})
                    {
                        SceneManager.MoveGameObjectToScene(session.PlayerShip.gameObject,original);Check(session.Missions.StartMission(vessel,berth==HarborBerth.East?0:3),"physics mission fixture "+vessel+berth);
                        // The frigate's first mission is non-tug. Use its third (solo bow) exercise for the east case.
                        if(vessel==VesselId.Barbaros&&berth==HarborBerth.East)Check(session.Missions.StartMission(vessel,2),"frigate bow tug scenario");
                        session.Missions.BeginMission();var run=session.Missions.Run;FeedGates(run);var ship=session.PlayerShip;var body=ship.GetComponent<Rigidbody>();
                        ship.enabled=false;ship.GetComponent<ShipBuoyancy>().enabled=false;session.Tugs.enabled=false;session.Mooring.enabled=false;
                        SceneManager.MoveGameObjectToScene(ship.gameObject,isolated);body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
                        int side=berth==HarborBerth.East?1:-1;ship.ResetSimulation(run.Target-Vector3.right*side*4,Quaternion.identity);WaterWaves.Reset(new SeaStateSettings(1,0),0);
                        session.Tugs.BeginMode(SimulationMode.Harbor,vessel);session.Tugs.SelectBerth(berth); // Place both tugs near the deliberately relocated test ship, not at the exterior spawn.
                        session.Tugs.Toggle(0);if(run.Definition.TugExercise!=TugTraining.BowSolo)session.Tugs.Toggle(1);
                        session.Tugs.SimulateStep(.02f,0);Check(run.BowTugSeconds==0&&run.SternTugSeconds==0,"no credit across initial open gap");
                        Vector3 start=body.position;float peak=0;bool second=run.Definition.TugExercise!=TugTraining.BowSolo;
                        for(int n=0;n<2400;n++)
                        {
                            ship.SimulateForces(.02f);session.Tugs.SimulateStep(.02f,n*.02f);physics.Simulate(.02f);peak=Mathf.Max(peak,session.Tugs.Delivered(0),session.Tugs.Delivered(1));
                            if(!second&&run.SoloTugSeconds>=3){session.Tugs.Toggle(1);second=true;}
                        }
                        report.AppendLine($"{vessel} {berth}: displacement_m={Vector3.Dot(body.position-start,Vector3.right*side)*4:0.000} bow_s={run.BowTugSeconds:0.0} stern_s={run.SternTugSeconds:0.0} solo_s={run.SoloTugSeconds:0.0} delivered_peak={peak:0.000} ready={run.TugWorkReady}");File.WriteAllText(Folder+"/physics.txt",report.ToString());
                        Check(peak>.1f&&Vector3.Dot(body.position-start,Vector3.right*side)>.1f,"actual tug shoreward displacement "+vessel+berth);Check(run.TugWorkReady,"real delivered force fulfills mission "+vessel+berth);
                        float prior=run.BowTugSeconds;session.OpenMenu();session.Tugs.SimulateStep(.1f,50);Check(run.BowTugSeconds==prior&&session.Tugs.Delivered(0)==0,"menu no force or evidence");session.Resume();
                        ship.enabled=true;ship.GetComponent<ShipBuoyancy>().enabled=true;session.Tugs.enabled=true;session.Mooring.enabled=true;body.constraints=RigidbodyConstraints.None;
                    }
                }
                finally{SceneManager.MoveGameObjectToScene(session.PlayerShip.gameObject,original);SceneManager.UnloadSceneAsync(isolated);}
                session.StartMode(SimulationMode.Harbor,VesselId.Ruzgar);Check(!session.Tugs.Available,"no small craft tugs");
                session.StartMode(SimulationMode.StraitPassage,VesselId.Barbaros);Check(!session.Tugs.Available&&!session.Missions.Assigned&&!session.Mooring.Available,"strait scope unchanged");
                var menu=session.GetComponent<ModeSelectionMenu>();session.OpenMenu();foreach(var vessel in VesselCatalog.All){menu.ShowMissionOptions(vessel);Check(menu.SelectedVessel==vessel,"fleet menu selection "+vessel);}
                File.WriteAllText(Folder+"/runtime.txt",$"FLEET_RUNTIME_PASS checks={checks} mission_cycles=20 tug_physics_cases=4 added_radio_clips=24 memory_storage_only\n");Debug.Log($"FLEET_RUNTIME_PASS checks={checks}");if(SessionState.GetBool("Meridian.FleetBatch",false))EditorApplication.Exit(0);
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);if(SessionState.GetBool("Meridian.FleetBatch",false))EditorApplication.Exit(1);}
        }
    }
}
