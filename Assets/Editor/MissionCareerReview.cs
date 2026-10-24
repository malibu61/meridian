using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    /// <summary>Opt-in verification. Every career test uses memory storage, never the player's PlayerPrefs.</summary>
    public static class MissionCareerReview
    {
        private const string Folder="Logs/MissionCareerReview";private static int checks,stage=-1,hornCompletions;private static double deadline,waitUntil;private static MemoryStorage memory;
        private sealed class MemoryStorage:IMissionProgressStorage
        {public readonly Dictionary<string,string> Data=new();public string Read(string key)=>Data.TryGetValue(key,out string value)?value:"";public void Write(string key,string value)=>Data[key]=value;}
        private static void Check(bool condition,string label){checks++;if(!condition)throw new InvalidOperationException("Mission review: "+label);}
        [InitializeOnLoadMethod] private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        [MenuItem("Meridian/Missions - Check Career Rules and Signals")]
        public static void CheckSources()
        {
            checks=0;Directory.CreateDirectory(Folder);var storage=new MemoryStorage();var progress=new MissionProgressStore(storage);
            Check(DockingMissionCatalog.Count==6&&progress.CompletedCount==0&&progress.ContinueIndex==0,"five docking plus one rescue mission, fresh first mission");
            for(int i=0;i<5;i++){Check(progress.IsUnlocked(i)==(i==0),"initial lock "+i);Check(DockingMissionCatalog.Get(i).Spawn.z<-100,"outside harbour spawn "+i);}
            Check(!progress.Complete(3,3,10)&&!progress.Start(2)&&!progress.Start(-1),"cannot skip locked missions");
            for(int i=0;i<5;i++)
            {
                Check(progress.Start(i)&&progress.Complete(i,1,100+i),"one star sufficient to unlock "+i);
                var reloaded=new MissionProgressStore(storage);Check(reloaded.IsCompleted(i)&&reloaded.CompletedCount==i+1,"persist across reconstruction "+i);
            }
            Check(progress.CompletedCount==5&&progress.IsUnlocked(0),"replay unlocked after full completion");
            progress.Complete(0,3,50);progress.Complete(0,1,200);var best=new MissionProgressStore(storage);Check(best.Stars(0)==3&&best.BestSeconds(0)==50,"best results never decrease");
            var other=new MissionProgressStore(storage,VesselId.Barbaros);Check(other.CompletedCount==0,"per-vessel storage key");
            var broken=new MemoryStorage();broken.Data["Meridian.Career.v1.Kumkale"]="not-json";Check(new MissionProgressStore(broken).CompletedCount==0,"corrupt JSON safe fallback");
            broken.Data["Meridian.Career.v1.Kumkale"]="{\"version\":1,\"entries\":[{\"id\":\"kumkale.port.v1\",\"completed\":true,\"stars\":3}]}";
            Check(!new MissionProgressStore(broken).IsUnlocked(2),"corrupt out-of-order completion cannot unlock");
            var signal=new ManeuverSignalMonitor();signal.Tick(.1f,0,true,0,0,3,true);
            Check(signal.Expected==WhistleSignal.Astern,"astern propulsion while moving AHEAD requires three");
            signal.SoundCompleted(WhistleSignal.Starboard,1);Check(signal.Expected==WhistleSignal.Astern&&signal.WrongSignals==1,"wrong selection does not discharge obligation");
            signal.SoundCompleted(WhistleSignal.Astern,6);Check(signal.PendingCount==0&&signal.Complied==1,"completed correct three-short signal");
            for(int i=0;i<100;i++)signal.Tick(.1f,6+i*.1f,true,0,0,-1,true);Check(signal.PendingCount==0,"sustained astern does not spam");
            signal.Tick(2.1f,18,false,0,0,-1,true);signal.Tick(.1f,19,true,0,0,-1,true);Check(signal.Expected==WhistleSignal.Astern,"new astern episode requires new signal");
            var drifting=new ManeuverSignalMonitor();drifting.Tick(1,1,false,0,0,-3,true);Check(drifting.Expected==WhistleSignal.None,"backwards drift without astern propulsion is not a three-short trigger");
            var hidden=new ManeuverSignalMonitor();hidden.Tick(20,20,true,1,20,3,false);Check(!hidden.Violated&&hidden.PendingCount==0,"visibility context gate");
            var missed=new ManeuverSignalMonitor();missed.Tick(.1f,0,true,0,0,1,true);missed.Tick(16,16,true,0,0,1,true);Check(missed.Violated,"missed deadline is mandatory failure condition");
            foreach(int side in new[]{-1,1})
            {
                var turn=new ManeuverSignalMonitor();turn.Tick(.1f,0,false,side*.8f,0,3,true);turn.Tick(.1f,.1f,false,side*.8f,side*4,3,true);
                Check(turn.Expected==(side>0?WhistleSignal.Starboard:WhistleSignal.Port),"actual turn sign "+side);
            }
            var pre=new ManeuverSignalMonitor();pre.SoundCompleted(WhistleSignal.Astern,2);pre.Tick(.1f,5,true,0,0,2,true);Check(pre.PendingCount==0&&pre.Complied==1,"recent pre-announcement accepted once");
            pre.Tick(2.1f,7.2f,false,0,0,2,true);pre.Tick(.1f,8,true,0,0,2,true);Check(pre.Expected==WhistleSignal.Astern,"pre-announcement not reusable");
            var diff=new ManeuverSignalMonitor();diff.Tick(.1f,0,false,0,0,2,true,1);diff.Tick(.1f,.1f,false,0,4,2,true,1);Check(diff.Expected==WhistleSignal.Starboard,"differential engine course change is monitored");
            foreach(var def in DockingMissionCatalog.All)
            {
                if(def.IsRescue)continue; // Rescue has its own ordered actions, not harbour gates.
                var run=new DockingMissionRun(def);Check(run.Phase==DockingMissionPhase.Briefing&&run.Begin(),"briefing then run "+def.Number);
                FeedGates(run);Check(run.GatesPassed==3,"ordered entry gates "+def.Number);
                if(def.Reassign)Check(run.Reassigned&&run.Berth==HarborBerth.West,"reassignment before final approach");
                for(int i=0;i<55;i++)run.Tick(.1f,run.Target,0,0,0,false,0);
                Check(run.Phase==DockingMissionPhase.Passed&&run.Stars==3,"settle five seconds "+def.Number);
            }
            var noEntry=new DockingMissionRun(DockingMissionCatalog.Get(0));noEntry.Begin();for(int i=0;i<60;i++)noEntry.Tick(.1f,noEntry.Target,0,0,0,false,0);Check(noEntry.Phase!=DockingMissionPhase.Passed,"cannot skip gates by appearing at berth");
            var pending=new DockingMissionRun(DockingMissionCatalog.Get(0));pending.Begin();FeedGates(pending);pending.Tick(.1f,pending.Target,0,0,0,true,0);for(int i=0;i<55;i++)pending.Tick(.1f,pending.Target,0,0,0,true,0);Check(pending.Phase==DockingMissionPhase.Running&&pending.HoldSeconds==0,"pending whistle prevents mission completion");pending.SoundCompleted(WhistleSignal.Astern);for(int i=0;i<55;i++)pending.Tick(.1f,pending.Target,0,0,0,true,0);Check(pending.Phase==DockingMissionPhase.Passed,"signal then valid settle passes");
            var severe=new DockingMissionRun(DockingMissionCatalog.Get(0));severe.Begin();severe.Collision(2);Check(severe.Phase==DockingMissionPhase.Failed,"severe impact fails");
            var gentle=new DockingMissionRun(DockingMissionCatalog.Get(0));gentle.Begin();gentle.Collision(.1f);FeedGates(gentle);for(int i=0;i<55;i++)gentle.Tick(.1f,gentle.Target,0,0,0,false,0);Check(gentle.Phase==DockingMissionPhase.Passed&&gentle.Stars==2,"minor nudge reduces stars, does not lock next");
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(2622,1206),new Vector2Int(1024,768)})foreach(bool four in new[]{false,true})
            {
                var layout=new HelmLayout(size.x,size.y,new Rect(0,0,size.x,size.y),four);Rect signals=layout.Signals;
                Check(!signals.Overlaps(layout.Port)&&!signals.Overlaps(layout.Master)&&!signals.Overlaps(layout.Rudder)&&!signals.Overlaps(layout.PortInner)&&!signals.Overlaps(layout.StarboardInner),"signal controls clear existing helm "+size+"/"+four);
                Check(layout.BlocksCamera(signals.center),"signal controls block orbit");
            }
            foreach(WhistleSignal whistle in new[]{WhistleSignal.Starboard,WhistleSignal.Port,WhistleSignal.Astern,WhistleSignal.Prolonged,WhistleSignal.Attention})
            {
                var clip=MaritimeAudio.BuildWhistle(whistle,VesselId.Kumkale);try{Check(Mathf.Abs(clip.length-MaritimeAudio.SignalDuration(whistle))<.001f,"whistle duration "+whistle);float[] data=new float[clip.samples];clip.GetData(data,0);float peak=0;foreach(float value in data){if(!float.IsFinite(value))throw new Exception("Nonfinite audio");peak=Mathf.Max(peak,Mathf.Abs(value));}Check(peak>.1f&&peak<.9f,"audible unclipped whistle "+whistle);if(whistle==WhistleSignal.Astern)Check(Mathf.Abs(data[(int)(1.5f*clip.frequency)])<.0001f,"silence between short blasts");}finally{Object.DestroyImmediate(clip);}
            }
            File.WriteAllText(Folder+"/rules.txt",$"MISSION_CAREER_RULES_PASS checks={checks}\n");Debug.Log($"MISSION_CAREER_RULES_PASS checks={checks}");
        }
        private static void FeedGates(DockingMissionRun run)
        {for(int i=0;i<3;i++){Vector3 p=DockingMissionCatalog.Gate(i)+Vector3.forward*1;run.Tick(.1f,p,0,1,0,false,0);}run.SetMooringComplete(true);}
        [MenuItem("Meridian/Missions - Run Career and Audio Integration")]
        public static void RunInteractive()=>Run(false);
        public static void RunBatch()=>Run(true);
        private static void Run(bool batch)
        {CheckSources();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.MissionReview.Start",true);SessionState.SetBool("Meridian.MissionReview.Batch",batch);EditorApplication.isPlaying=true;}
        [MenuItem("Meridian/Missions - Open Kumkale Career")]
        public static void OpenDemo()
        {EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.MissionDemo",true);EditorApplication.isPlaying=true;}
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.MissionDemo",false))
            {SessionState.SetBool("Meridian.MissionDemo",false);session.GetComponent<ModeSelectionMenu>().ShowMissionOptions(VesselId.Kumkale);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();return;}
            if(SessionState.GetBool("Meridian.MissionReview.Start",false))
            {SessionState.SetBool("Meridian.MissionReview.Start",false);checks=0;stage=0;deadline=EditorApplication.timeSinceStartup+120;memory=new MemoryStorage();session.Missions.SetStorageForReview(memory);}
            if(stage<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Career runtime stage "+stage);
                if(stage==0)
                {
                    Check(!session.Missions.StartMission(1),"locked mission rejected before world reset");Check(session.Missions.StartMission(0),"launch first");
                    Check(session.ActiveVessel==VesselId.Kumkale&&session.ActiveMode==SimulationMode.Harbor&&session.PlayerShip.transform.position.z<-100,"correct ship and exterior start");
                    Check(!session.CanControl&&Time.timeScale==0,"briefing blocks helm and physics");Check(!session.ApplySeaState(new SeaStateSettings(5,0)),"cannot cheat fixed mission sea");
                    foreach(string id in new[]{"brief_1","brief_2","brief_3","brief_4","brief_5","entry","approach","reassign","signal_1","signal_2","signal_3","success","career_complete","failure","chatter_1","chatter_2","chatter_3","free_sail"})
                    {var clip=session.Audio.LoadRadio(id);Check(clip!=null&&clip.length>1&&clip.channels==1,"baked Turkish radio loads "+id);float[] samples=new float[clip.samples];Check(clip.GetData(samples,0),"radio PCM "+id);double power=0;foreach(float value in samples)power+=value*value;Check(Math.Sqrt(power/samples.Length)>.005,"audible radio "+id);}
                    session.Missions.BeginMission();Check(session.CanControl&&Time.timeScale>0,"acknowledge briefing");
                    hornCompletions=0;session.Audio.HornCompleted+=CountHorn;Check(session.Audio.PlayWhistle(WhistleSignal.Astern)&&!session.Audio.PlayWhistle(WhistleSignal.Port),"one sequence at a time");
                    session.OpenMenu();stage=1;waitUntil=EditorApplication.timeSinceStartup+.7;return;
                }
                if(stage==1)
                {if(EditorApplication.timeSinceStartup<waitUntil)return;Check(hornCompletions==0&&session.Audio.HornBusy,"menu pauses sequence without early credit");session.Resume();stage=2;deadline=EditorApplication.timeSinceStartup+10;return;}
                if(stage==2)
                {
                    if(hornCompletions==0)return;Check(hornCompletions==1&&!session.Audio.HornBusy,"credit only after all three audible blasts");session.Audio.HornCompleted-=CountHorn;
                    for(int i=0;i<5;i++)
                    {
                        Check(session.Missions.StartMission(i),"start unlocked mission "+i);session.Missions.BeginMission();var run=session.Missions.Run;
                        FeedGates(run);for(int n=0;n<55;n++)run.Tick(.1f,run.Target,0,0,0,false,0);session.Missions.RecordResult();
                        Check(run.Phase==DockingMissionPhase.Passed&&session.Missions.Progress.IsCompleted(i),"result recorded "+i);Check(!session.CanControl&&Time.timeScale==0,"result freezes helm "+i);
                    }
                    var reloaded=new MissionProgressStore(memory);Check(reloaded.CompletedCount==5&&reloaded.IsUnlocked(4),"saved career survives new store");
                    Check(session.Missions.StartMission(2),"replay completed");session.Missions.BeginMission();session.Missions.ReportCollision(2);session.Missions.RecordResult();Check(session.Missions.Terminal&&session.Missions.Progress.CompletedCount==5,"failed replay preserves prior progress");
                    session.StartMode(SimulationMode.Harbor,VesselId.Barbaros);session.Tugs.SelectBerth(HarborBerth.East);Check(!session.Missions.Assigned&&session.CanControl,"frigate free sail clears mission and allows selected tug berth");
                    session.StartMode(SimulationMode.Harbor,VesselId.Anadolu);Check(session.Tugs.Available&&session.Tugs.IntentOpen&&!session.Missions.Assigned,"existing Anadolu tug flow preserved");
                    session.StartMode(SimulationMode.StraitPassage,VesselId.Kumkale);Check(session.Voyage!=null&&!session.Missions.Assigned,"strait remains playable");
                    Check(session.GetComponents<MaritimeAudio>().Length==1&&session.GetComponents<MissionController>().Length==1&&session.PlayerShip.GetComponents<MissionCollisionReporter>().Length==1,"no duplicate services");
                    session.OpenMenu();session.GetComponent<ModeSelectionMenu>().ShowMissionOptions();
                    File.WriteAllText(Folder+"/runtime.txt",$"MISSION_CAREER_RUNTIME_PASS checks={checks} radio_clips=18 mission_cycles=5 memory_storage_only\n");Debug.Log($"MISSION_CAREER_RUNTIME_PASS checks={checks}");stage=-1;
                    if(SessionState.GetBool("Meridian.MissionReview.Batch",false))EditorApplication.Exit(0);
                }
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);stage=-1;if(SessionState.GetBool("Meridian.MissionReview.Batch",false))EditorApplication.Exit(1);}
        }
        private static void CountHorn(WhistleSignal signal)=>hornCompletions++;
    }
}
