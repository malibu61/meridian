using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class CompactHudReview
    {
        private const string Folder="Logs/CompactHudReview";private static int checks,stage=-1,completions;private static double waitUntil,deadline;
        private static void Check(bool ok,string name){checks++;if(!ok)throw new InvalidOperationException("Compact HUD: "+name);}
        [InitializeOnLoadMethod]private static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        public static void CheckRules()
        {
            checks=0;Directory.CreateDirectory(Folder);
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2622,1206),new Vector2Int(1024,768),new Vector2Int(852,393)})foreach(bool safeInset in new[]{false,true})foreach(bool four in new[]{false,true})
            {
                Rect safe=safeInset?new Rect(48,12,size.x-96,size.y-24):new Rect(0,0,size.x,size.y);var l=new HelmLayout(size.x,size.y,safe,four);
                Check(l.Header.Contains(l.RollInstrument.min)&&l.Header.Contains(l.RollInstrument.max),"one roll dial inside header "+size+four);
                Check(l.RollInstrument.height<=l.Header.height&&l.RollInstrument.width>100,"compact dial proportions");
                Check(HelmLayout.SignalRows==2&&HelmLayout.SignalColumns==3,"two rows and three columns");
                Check(Mathf.Abs(l.Signals.xMax-(l.Width-24))<.01f&&l.Signals.y==152,"upper right grid");
                Check(!l.Signals.Overlaps(l.Header)&&!l.Signals.Overlaps(l.Status)&&!l.Signals.Overlaps(l.Port)&&!l.Signals.Overlaps(l.Starboard)&&!l.Signals.Overlaps(l.Master)&&!l.Signals.Overlaps(l.Rudder)&&!l.Signals.Overlaps(l.PortInner)&&!l.Signals.Overlaps(l.StarboardInner),"grid clear of helm");
                Check(!l.Signals.Overlaps(MissionController.HudRect(l))&&!l.Signals.Overlaps(MooringController.PanelRect(l))&&!l.Signals.Overlaps(TugAssistController.PanelRect(l)),"grid clear of mission/tug/mooring cards");
                Check(!l.RouteMap.Overlaps(l.Signals)&&!l.RouteMap.Overlaps(l.RollInstrument),"strait map not under whistles");
                Check(!l.BlocksCamera(new Vector2(100,200))&&!l.BlocksCamera(new Vector2(600,320)),"old roll and horn areas freed for orbit");
                for(int i=0;i<6;i++)
                {
                    Rect button=l.SignalButton(i);Check(Mathf.Abs(button.width-button.height)<.001f&&button.width>=54,"square button "+i);
                    Check(l.Signals.Contains(button.min)&&l.Signals.Contains(button.max),"button in grid "+i);
                    Check(HelmPointerRouter.Hit(l,button.center)==-1&&l.BlocksCamera(button.center),"button neither steers nor orbits "+i);
                    for(int j=i+1;j<6;j++)Check(!button.Overlaps(l.SignalButton(j)),"separate buttons "+i+j);
                    if(i<3)Check(Mathf.Abs(button.y-l.SignalButton(0).y)<.01f,"first row");else Check(button.y>l.SignalButton(0).y,"second row");
                }
                Vector2 pivot=RollGauge.CompactPivot(l.RollInstrument);for(int roll=-40;roll<=40;roll+=5)Check(l.RollInstrument.Contains(RollGauge.Point(pivot,RollGauge.CompactRadius+10,roll)),"dial scale fits header "+roll);
                Rect physical=l.Physical(l.Signals);Check(physical.x>=safe.x&&physical.xMax<=safe.xMax+.1f&&physical.y>=size.y-safe.yMax&&physical.yMax<=size.y-safe.y+.1f,"safe area grid");
            }
            HelmControlReview.RunChecks();SmallCraftControlReview.RunChecks();MissionCareerReview.CheckSources();MooringReview.CheckRules();TugAssistReview.CheckSources();
            File.WriteAllText(Folder+"/layout.txt",$"COMPACT_HUD_LAYOUT_PASS checks={checks} layouts=20 square_buttons=6 roll_header_only\n");Debug.Log($"COMPACT_HUD_LAYOUT_PASS checks={checks}");
        }
        public static void RunBatch(){CheckRules();EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.CompactHudReview",true);EditorApplication.isPlaying=true;}
        [MenuItem("Meridian/HUD - Open Compact Harbour Demo")]
        public static void OpenDemo(){EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");SessionState.SetBool("Meridian.CompactHudDemo",true);EditorApplication.isPlaying=true;}
        private static void OnHorn(WhistleSignal signal)=>completions++;
        private static bool CameraBlocks(GameSessionController session,Rect gui)
        {
            var l=HelmLayout.Current;Vector2 physical=l.Physical(gui).center;Vector2 screen=new(physical.x,Screen.height-physical.y);
            var method=typeof(FollowCamera).GetMethod("IsHudRegion",BindingFlags.NonPublic|BindingFlags.Instance);
            return (bool)method.Invoke(Object.FindAnyObjectByType<FollowCamera>(),new object[]{screen});
        }
        private static void Tick()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.CompactHudDemo",false))
            {
                SessionState.SetBool("Meridian.CompactHudDemo",false);session.StartMode(SimulationMode.Harbor,VesselId.Kumkale,new SeaStateSettings(1,315));
                session.PlayerShip.ResetSimulation(MooringRules.Destination(session.PlayerShip.Definition,HarborBerth.East)-Vector3.right*1.7f,Quaternion.identity);session.Mooring.RefreshBerth();
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();return;
            }
            if(SessionState.GetBool("Meridian.CompactHudReview",false)){SessionState.SetBool("Meridian.CompactHudReview",false);checks=0;stage=0;deadline=EditorApplication.timeSinceStartup+30;}
            if(stage<0)return;
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Compact HUD stage "+stage);
                if(stage==0)
                {
                    session.StartMode(SimulationMode.Harbor,VesselId.Kumkale);var l=HelmLayout.Current;
                    Check(CameraBlocks(session,l.Signals)&&CameraBlocks(session,l.RollInstrument),"live header and grid block camera");
                    Check(!CameraBlocks(session,new Rect(96,196,8,8))&&!CameraBlocks(session,new Rect(596,316,8,8)),"live freed space accepts orbit");
                    completions=0;session.Audio.HornCompleted+=OnHorn;
                    Check(session.Audio.PlayWhistle(WhistleSignal.Astern)&&session.Audio.StatusFeedback.Contains("3 KISA"),"three shorts feedback in status strip");
                    Check(!session.Audio.PlayWhistle(WhistleSignal.Port),"busy sequence protected");session.OpenMenu();waitUntil=EditorApplication.timeSinceStartup+.7;stage=1;return;
                }
                if(stage==1)
                {if(EditorApplication.timeSinceStartup<waitUntil)return;Check(completions==0&&session.Audio.HornBusy,"menu still pauses full horn sequence");session.Resume();stage=2;return;}
                if(stage==2)
                {if(completions==0)return;Check(completions==1&&!session.Audio.HornBusy,"one completion at end of sequence");session.Audio.HornCompleted-=OnHorn;session.Audio.Say("fleet_approach",true);stage=3;return;}
                if(stage==3)
                {
                    if(!session.Audio.StatusFeedback.StartsWith("TELSİZ"))return;Check(true,"radio text reused status strip");
                    session.StartMode(SimulationMode.StraitPassage,VesselId.Kumkale);Check(CameraBlocks(session,HelmLayout.Current.RouteMap),"moved strait map still blocks orbit");
                    session.StartMode(SimulationMode.Harbor,VesselId.Ruzgar);Check(HelmLayout.Current.FourEngines&&!session.Audio.SettingsOpen,"four-engine layout and settings reset");
                    Check(!CameraBlocks(session,HelmLayout.Current.RouteMap),"invisible harbour map no longer consumes orbit");
                    File.WriteAllText(Folder+"/runtime.txt",$"COMPACT_HUD_RUNTIME_PASS checks={checks} horn_pause_completion radio_status camera_regions four_engines\n");Debug.Log($"COMPACT_HUD_RUNTIME_PASS checks={checks}");stage=-1;EditorApplication.Exit(0);
                }
            }
            catch(Exception ex){File.WriteAllText(Folder+"/runtime.txt","FAILED\n"+ex);Debug.LogException(ex);stage=-1;EditorApplication.Exit(1);}
        }
    }
}
