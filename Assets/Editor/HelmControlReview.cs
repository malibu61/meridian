using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class HelmControlReview
    {
        private static double captureDeadline;
        private static int captureStage;
        private static Quaternion savedRotation;
        private static Vector3 savedPosition;
        private static float savedTimeScale;
        private const string Folder="Temp/HelmControlReview";
        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.delayCall+=RunChecks;
            EditorApplication.update-=Update;EditorApplication.update+=Update;
        }
        [MenuItem("Meridian/Controls - Check Detents and Layout")]
        public static void RunChecks()
        {
            int checks=0;void Check(bool condition,string name){checks++;if(!condition)throw new InvalidOperationException("Helm check: "+name);}
            float value=0;
            for(int i=1;i<=10;i++){value=ThrottleDetents.Step(value,1);Check(Mathf.Abs(value-i/10f)<.001f,"ahead notch "+i);}
            Check(ThrottleDetents.Step(1,1)==1,"ahead clamp");value=0;
            for(int i=1;i<=5;i++){value=ThrottleDetents.Step(value,-1);Check(Mathf.Abs(value+i/5f)<.001f,"astern notch "+i);}
            Check(ThrottleDetents.Step(-1,-1)==-1,"astern clamp");
            Check(ThrottleDetents.Step(-.2f,1)==0&&ThrottleDetents.Step(.1f,-1)==0,"neutral transition");
            Check(ThrottleDetents.Snap(float.NaN)==0&&ThrottleDetents.Snap(float.PositiveInfinity)==0,"finite input");
            for(int i=-100;i<=100;i++)
            {
                float command=ThrottleDetents.Snap(i/100f);
                Check(Mathf.Abs(ThrottleDetents.FromTravel(ThrottleDetents.ToTravel(command))-command)<.001f,"travel roundtrip "+i);
                Check(Mathf.Abs(command*(command<0?5:10)-Mathf.Round(command*(command<0?5:10)))<.001f,"valid lattice "+i);
            }
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2622,1206),new Vector2Int(852,393)})
            {
                Rect safe=new(48,12,size.x-96,size.y-24);HelmLayout layout=new(size.x,size.y,safe);
                for(int metric=0;metric<HelmLayout.HeaderMetricCount;metric++)
                {
                    Rect cell=layout.HeaderMetric(metric);
                    Check(cell.x>=layout.Header.x&&cell.xMax<=layout.Header.xMax&&cell.width>=150,"five header metrics fit "+size+"/"+metric);
                    if(metric>0)Check(cell.x>=layout.HeaderMetric(metric-1).xMax-.01f,"header cells do not overlap "+size+"/"+metric);
                }
                Rect[] areas={layout.Port,layout.Starboard,layout.Master,layout.Rudder,layout.RollInstrument};
                for(int i=0;i<areas.Length;i++)
                {
                    Rect physical=layout.Physical(areas[i]);
                    Check(physical.x>=safe.x&&physical.xMax<=safe.xMax+.1f&&physical.y>=0&&physical.yMax<=size.y,"safe area "+size+"/"+i);
                    Check(layout.BlocksCamera(areas[i].center),"camera excluded "+i);
                    for(int j=i+1;j<areas.Length;j++)Check(!areas[i].Overlaps(areas[j]),"no overlap "+i+"/"+j);
                }
                Check(HelmPointerRouter.Hit(layout,HelmLayout.Rail(layout.Master).center)==2,"master hit");
                Check(HelmPointerRouter.Hit(layout,new Vector2(layout.Port.center.x,layout.Port.y+25))==-1,"title does not accelerate");
                Check(!layout.RollInstrument.Overlaps(layout.Header)&&!layout.RollInstrument.Overlaps(layout.Status)&&!layout.RollInstrument.Overlaps(layout.RouteMap),"instrument clear of top telemetry "+size);
                Check(HelmPointerRouter.Hit(layout,layout.RollInstrument.center)==-1,"instrument is not an engine control "+size);
            }
            HelmPointerRouter router=new();
            Check(router.Begin(1,0)&&router.Begin(2,1),"two independent fingers");
            Check(!router.Begin(3,0),"one pointer per control");
            Check(router.Begin(4,2)&&router.Count==1&&!router.TryGet(1,out _)&&!router.TryGet(2,out _),"master takes engine ownership");
            Check(!router.Begin(5,0)&&router.Begin(6,3),"master locks engines not rudder");
            router.End(4);Check(router.Begin(5,0),"independent after master release");router.Clear();Check(router.Count==0,"pause cancel");
            Check(PrototypeHud.RollSide(0)=="DENGE"&&PrototypeHud.RollSide(.049f)=="DENGE"&&PrototypeHud.RollSide(-.049f)=="DENGE","display precision deadband");
            Check(PrototypeHud.RollSide(5)=="İSKELE"&&PrototypeHud.RollSide(-5)=="SANCAK","roll direction labels");
            Vector2 pivot=new(100,40);
            Check(RollGauge.Point(pivot,60,0)==pivot+Vector2.up*60,"zero pendulum points down in GUI coordinates");
            Check(RollGauge.Point(pivot,60,10).x<pivot.x&&RollGauge.Point(pivot,60,-10).x>pivot.x,"port left, starboard right");
            for(int degree=-40;degree<=40;degree++)
            {
                Check(Mathf.Abs(Vector2.Distance(pivot,RollGauge.Point(pivot,60,degree))-60)<.001f,"constant pendulum length "+degree);
                Check(Mathf.Abs(RollGauge.NeedleAngle(degree))<=RollGauge.SweepDegrees,"bounded needle "+degree);
            }
            Check(RollGauge.NeedleAngle(60)==RollGauge.NeedleAngle(40)&&RollGauge.NeedleAngle(-60)==RollGauge.NeedleAngle(-40),"40 degree scale stops while numeric angle stays physical");
            Check(RollGauge.NeedleAngle(float.NaN)==0&&RollGauge.NeedleAngle(float.PositiveInfinity)==0,"nonfinite gauge safety");
            Debug.Log($"HELM_LOGIC_CHECK_PASS checks={checks}");
            Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/logic-result.txt",$"HELM_LOGIC_CHECK_PASS checks={checks}\n");
        }
        [MenuItem("Meridian/Controls - Run Live Review")]
        public static void StartReview()
        {
            RunChecks();Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/runtime-result.txt","RUNNING\n");SessionState.SetBool("Meridian.HelmReview",true);
            if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;
        }
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            GameSessionController session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.HelmReview",false))
            {
                SessionState.SetBool("Meridian.HelmReview",false);int checks=0;
                void Check(bool condition,string name){checks++;if(!condition)throw new InvalidOperationException("Helm runtime: "+name);}
                foreach(VesselId id in new[]{VesselId.Barbaros,VesselId.Anadolu})
                {
                    session.StartMode(SimulationMode.Harbor,id);ShipController ship=session.PlayerShip;
                    session.OpenMenu();Vector3 spawn=ship.transform.position;
                    PrototypeHud hud=session.GetComponent<PrototypeHud>();
                    ship.ResetSimulation(spawn,Quaternion.Euler(0,0,10));
                    Check(Mathf.Abs(hud.RollDegrees-10)<.001f&&PrototypeHud.RollSide(hud.RollDegrees)=="İSKELE","real port roll "+id);
                    ship.ResetSimulation(spawn,Quaternion.Euler(0,0,-10));
                    Check(Mathf.Abs(hud.RollDegrees+10)<.001f&&PrototypeHud.RollSide(hud.RollDegrees)=="SANCAK","real starboard roll "+id);
                    ship.ResetSimulation(spawn,Quaternion.Euler(0,80,0));
                    Check(Mathf.Abs(hud.RollDegrees)<.001f,"heading is not roll "+id);
                    session.Resume();
                    ship.SetPortThrottle(.74f);ship.SetStarboardThrottle(-.35f);
                    Check(Mathf.Abs(ship.PortThrottle-.7f)<.001f&&Mathf.Abs(ship.StarboardThrottle+.4f)<.001f,"independent snapped "+id);
                    ship.SetCombinedThrottle(.83f);Check(ship.PortThrottle==.8f&&ship.StarboardThrottle==.8f,"master ahead "+id);
                    ship.SetPortThrottle(.1f);Check(ship.StarboardThrottle==.8f,"master not permanent link "+id);
                    ship.SetCombinedThrottle(-.76f);Check(ship.PortThrottle==-.8f&&ship.StarboardThrottle==-.8f,"master astern "+id);
                    ship.SetRudder(.5f);ship.EmergencyStop();Check(ship.PortThrottle==0&&ship.StarboardThrottle==0&&ship.Rudder==.5f,"stop preserves rudder "+id);
                    session.OpenMenu();Check(Time.timeScale==0,"pause "+id);session.Resume();Check(session.ActiveVessel==id,"resume "+id);
                    session.StartMode(SimulationMode.StraitPassage,id);Check(ship.PortThrottle==0&&ship.StarboardThrottle==0&&ship.Rudder==0,"new voyage resets "+id);
                }
                session.StartMode(SimulationMode.Harbor,VesselId.Barbaros);session.PlayerShip.SetCombinedThrottle(.6f);
                Debug.Log($"HELM_RUNTIME_CHECK_PASS checks={checks}");File.WriteAllText(Folder+"/runtime-result.txt",$"HELM_RUNTIME_CHECK_PASS checks={checks}\n");captureStage=1;captureDeadline=EditorApplication.timeSinceStartup+1;
            }
            if(captureStage==0||EditorApplication.timeSinceStartup<captureDeadline)return;
            if(captureStage==1)
            {
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/hud-barbaros.png"));captureStage=2;
            }
            else if(captureStage==2)
            {
                session.OpenMenu();ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/menu.png"));captureStage=3;
            }
            else if(captureStage==3)
            {
                session.Resume();savedTimeScale=Time.timeScale;Time.timeScale=0;
                savedRotation=session.PlayerShip.transform.rotation;savedPosition=session.PlayerShip.transform.position;
                session.PlayerShip.ResetSimulation(savedPosition,Quaternion.Euler(0,0,10));captureStage=4;
            }
            else if(captureStage==4){ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/roll-gauge-port.png"));captureStage=5;}
            else if(captureStage==5){session.PlayerShip.ResetSimulation(savedPosition,Quaternion.identity);captureStage=6;}
            else if(captureStage==6){ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/roll-gauge-level.png"));captureStage=7;}
            else if(captureStage==7){session.PlayerShip.ResetSimulation(savedPosition,Quaternion.Euler(0,0,-10));captureStage=8;}
            else if(captureStage==8){ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/roll-gauge-starboard.png"));captureStage=9;}
            else
            {
                session.PlayerShip.ResetSimulation(savedPosition,savedRotation);Time.timeScale=savedTimeScale;
                session.OpenMenu();captureStage=0;Debug.Log("HELM_VISUAL_REVIEW_READY "+Path.GetFullPath(Folder));
            }
            captureDeadline=EditorApplication.timeSinceStartup+.6;
        }
    }
}
