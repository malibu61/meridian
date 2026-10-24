using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class ManOverboardReview
    {
        private const string Folder="Logs/ManOverboardReview";private static int checks,stage=-1,vesselIndex;private static double deadline,nextReport;private static Memory memory;
        private sealed class Memory:IMissionProgressStorage
        {public Dictionary<string,string> Data=new();public string Read(string key)=>Data.TryGetValue(key,out var value)?value:"";public void Write(string key,string value)=>Data[key]=value;}
        private static void Check(bool ok,string name){checks++;if(!ok)throw new InvalidOperationException("MOB review: "+name);}
        private static Vector3 SafePosition(ManOverboardRun r)=>r.Casualty-Vector3.right*r.Side*(r.Vessel.BeamMetres/8+1.1f);
        private static void Step(DockingMissionRun run,int count,bool stop=true,float speed=0,float yaw=0,float roll=0)
        {for(int n=0;n<count;n++)run.TickRescue(.1f,SafePosition(run.Rescue),0,speed,stop,yaw,roll);}
        private static void Land(DockingMissionRun run)
        {run.Begin();while(!run.Rescue.InWater)run.TickRescue(.1f,run.Definition.Spawn,0,run.Definition.InitialKnots,false,0,0);}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        [MenuItem("Meridian/MOB - Check Rescue Rules")]
        public static void CheckRules()
        {
            Directory.CreateDirectory(Folder);checks=0;Check(DockingMissionCatalog.TotalCount==24&&DockingMissionCatalog.Count==6,"24 missions");
            foreach(var vessel in VesselCatalog.All)
            {
                var d=DockingMissionCatalog.Get(vessel,5);var memory=new Memory();var progress=new MissionProgressStore(memory,vessel);
                Check(d.IsRescue&&d.Vessel==vessel&&!d.RequiresTugs&&!d.IndependentEngines&&d.Spawn.z<-250,"rescue profile "+vessel);
                Check(!progress.IsUnlocked(5)&&!progress.Complete(5,3,30),"rescue locked "+vessel);
                for(int i=0;i<5;i++)Check(progress.Start(i)&&progress.Complete(i,2,100+i),"previous chain "+vessel+i);
                // Round-trip the old five-entry v1 format (new entry is absent, key unchanged).
                string key="Meridian.Career.v1."+vessel;string json=memory.Data[key];int last=json.LastIndexOf(",{\"id\":",StringComparison.Ordinal);Check(last>0,"legacy fixture");memory.Data[key]=json.Substring(0,last)+"]}";
                var reload=new MissionProgressStore(memory,vessel);Check(reload.CompletedCount==5&&reload.IsUnlocked(5)&&!reload.IsCompleted(5)&&reload.Stars(0)==2&&reload.BestSeconds(0)==100,"five-entry save preserved "+vessel);
                var run=new DockingMissionRun(d);Check(!run.Rescue.Alarm()&&!run.Rescue.Mark()&&!run.Rescue.Recover(),"briefing cannot advance");run.Begin();var r=run.Rescue;
                Check(!r.Mark()&&!r.Recover()&&r.Alarm()&&!r.Alarm()&&r.Mark()&&!r.Mark(),"ordered alarm/mark "+vessel);
                Step(run,60,false);Check(!r.CanRecover&&r.StableSeconds==0,"actual engines running");
                Step(run,60,true,.5f);Check(!r.CanRecover,"speed limit");Step(run,60,true,0,1);Check(!r.CanRecover,"yaw limit");Step(run,60,true,0,0,11);Check(!r.CanRecover,"roll limit");
                // A change in position must not allow bow/stern pickup, even near a station-distance sphere.
                var p=SafePosition(r)-Vector3.forward*10;run.TickRescue(.1f,p,0,0,true,0,0);Check(!r.CanRecover,"longitudinal alignment");
                Step(run,42);Check(r.CanRecover&&r.Recover(),"stable alongside -> explicit recovery");Step(run,30);Check(run.Phase==DockingMissionPhase.Running,"not instant rescue");Step(run,1,false);Check(r.Stage==RescueStage.Approach&&r.RecoverySeconds==0,"propulsion cancels lift");
                Step(run,42);Check(r.Recover(),"can retry safe lift");Step(run,82);Check(run.Phase==DockingMissionPhase.Passed&&r.Completed,"eight second rescue complete");
                Check(reload.Complete(5,run.Stars,run.Elapsed),"rescue saves");var saved=new MissionProgressStore(memory,vessel);Check(saved.CompletedCount==6&&saved.ContinueIndex==5&&saved.Stars(5)==3,"sixth results persist");
                Check(new MissionProgressStore(memory,vessel==VesselId.Kumkale?VesselId.Barbaros:VesselId.Kumkale).CompletedCount==0,"independent career");
                var hull=new DockingMissionRun(d);Land(hull);hull.TickRescue(.1f,hull.Rescue.Casualty,0,0,true,0,0);Check(hull.Phase==DockingMissionPhase.Failed,"overrun fails");
                var stern=new DockingMissionRun(d);Land(stern);stern.TickRescue(.1f,stern.Rescue.Casualty+Vector3.forward*(r.Vessel.LengthMetres/8+2),0,0,true,0,0);Check(stern.Phase==DockingMissionPhase.Failed,"stern propeller exclusion");
                var sweep=new DockingMissionRun(d);Land(sweep);sweep.TickRescue(.1f,sweep.Rescue.Casualty-Vector3.right*(r.Vessel.BeamMetres/8+3),0,0,true,0,0);sweep.TickRescue(.1f,sweep.Rescue.Casualty+Vector3.right*(r.Vessel.BeamMetres/8+3),0,0,true,0,0);Check(sweep.Phase==DockingMissionPhase.Failed,"swept hull detects crossing");
                var timeout=new DockingMissionRun(d);timeout.Begin();for(int i=0;i<602;i++)timeout.TickRescue(.1f,d.Spawn,0,0,true,0,0);Check(timeout.Phase==DockingMissionPhase.Failed,"alarm timeout");
                var noSkip=new DockingMissionRun(d);noSkip.Begin();noSkip.SetMooringComplete(true);for(int i=0;i<80;i++)noSkip.Tick(.1f,noSkip.Target,0,0,0,false,0);Check(noSkip.Phase==DockingMissionPhase.Running,"docking cannot complete rescue");
                foreach(var id in new[]{"mob_brief","mob_alarm","mob_mark","mob_recover","mob_success","mob_failure"})Check(Resources.Load<AudioClip>("Audio/Radio/"+id)!=null&&MaritimeAudio.RadioText(id)!=id,"radio asset "+id);
            }
            FleetCareerReview.CheckRules();MissionCareerReview.CheckSources();
            File.WriteAllText(Folder+"/rules.txt",$"MOB_RULES_PASS checks={checks} vessels=4 legacy_five_entry_saves_preserved ordered_actions neutral_speed_yaw_roll hull_sweep recovery_abort\n");Debug.Log($"MOB_RULES_PASS checks={checks}");
        }
        public static void RunBatch(){CheckRules();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.MobReview",true);EditorApplication.isPlaying=true;}
        [MenuItem("Meridian/MOB - Open Training Preview")]
        public static void OpenPreview(){EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.MobPreview",true);EditorApplication.isPlaying=true;}
        private static void Unlock(MissionController controller,VesselId vessel)
        {var p=controller.GetProgress(vessel);for(int i=0;i<5;i++)p.Complete(i,1,100+i);}
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.MobPreview",false))
            {SessionState.SetBool("Meridian.MobPreview",false);session.Missions.SetStorageForReview(new Memory());Unlock(session.Missions,VesselId.Kumkale);session.Missions.StartMission(VesselId.Kumkale,5);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();return;}
            if(SessionState.GetBool("Meridian.MobReview",false)){SessionState.SetBool("Meridian.MobReview",false);checks=0;memory=new Memory();session.Missions.SetStorageForReview(memory);vesselIndex=0;stage=0;deadline=EditorApplication.timeSinceStartup+120;}
            if(stage<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("MOB runtime stage "+stage);
                var controller=session.Missions;
                if(controller.Run?.Rescue!=null&&EditorApplication.timeSinceStartup>nextReport)
                {
                    nextReport=EditorApplication.timeSinceStartup+5;var r=controller.Run.Rescue;var ship=session.PlayerShip;
                    File.WriteAllText(Folder+"/live.txt",$"vessel={ship.Definition.Id} test_stage={stage} rescue_stage={r.Stage} mission={controller.Run.Phase} elapsed={r.Elapsed} stable={r.StableSeconds} dt={Time.deltaTime} timeScale={Time.timeScale} speed={ship.SpeedKnots} station_gap={r.StationGapMetres} hold={r.HoldReason} failure={r.Failure}\n");
                }
                if(stage==0)
                {
                    var vessel=VesselCatalog.All[vesselIndex];Unlock(controller,vessel);Check(controller.StartMission(vessel,5),"start sixth "+vessel);
                    Check(session.ActiveVessel==vessel&&controller.BlocksControl&&!session.Tugs.Available&&!session.Mooring.Available,"briefing and no harbor tools");
                    Check(Object.FindAnyObjectByType<ManOverboardVisual>()!=null,"casualty spawned");Check(controller.BeginMission()&&controller.RescueAction()&&controller.RescueAction(),"production alarm and mark");
                    var r=controller.Run.Rescue;session.PlayerShip.ResetSimulation(SafePosition(r),Quaternion.identity);
                    // Controlled safe-pose fixture. Test the real mission Update/animation/save path,
                    // not an autopilot fighting free wave drift. Pure rules separately test unsafe telemetry.
                    session.PlayerShip.GetComponent<Rigidbody>().constraints=RigidbodyConstraints.FreezeAll;
                    session.OpenMenu();Check(!controller.RescueAction(),"menu blocks action");session.Resume();stage=1;return;
                }
                if(stage==1)
                {if(!controller.Run.Rescue.CanRecover)return;Check(controller.RescueAction(),"production safe recovery action");stage=2;return;}
                if(stage==2)
                {
                    if(!controller.Terminal)return;Check(controller.Run.Phase==DockingMissionPhase.Passed,"real update recovery completed");controller.RecordResult();
                    var vessel=VesselCatalog.All[vesselIndex];Check(new MissionProgressStore(memory,vessel).CompletedCount==6,"result saved per vessel");
                    if(++vesselIndex<4){stage=0;return;}
                    session.PlayerShip.GetComponent<Rigidbody>().constraints=RigidbodyConstraints.None;
                    session.StartMode(SimulationMode.Harbor,VesselId.Kumkale);Check(controller.Run==null&&session.Mooring.Available,"free sail clears rescue and restores mooring");
                    session.StartMode(SimulationMode.StraitPassage,VesselId.Kumkale);Check(controller.Run==null&&!session.Mooring.Available,"strait clean");
                    File.WriteAllText(Folder+"/runtime.txt",$"MOB_RUNTIME_PASS checks={checks} vessels=4 real_update_cycles=4 in_memory_saves_only\n");Debug.Log($"MOB_RUNTIME_PASS checks={checks}");stage=-1;EditorApplication.Exit(0);
                }
            }
            catch(Exception ex){Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);stage=-1;EditorApplication.Exit(1);}
        }
    }
}
