using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class SmallCraftControlReview
    {
        private const string Folder="Temp/SmallCraftControls";
        private static int stage=-1,checks,engine;
        private static double deadline;
        private static readonly float[] yaw=new float[4];
        private static readonly MethodInfo Apply=typeof(PrototypeHud).GetMethod("ApplyPointer",BindingFlags.Instance|BindingFlags.NonPublic);
        private static void Check(bool ok,string name){checks++;if(!ok){stage=-1;Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/runtime-result.txt","FAILED: "+name);throw new InvalidOperationException("Small craft controls: "+name);}}
        [InitializeOnLoadMethod] private static void Register(){EditorApplication.update-=Update;EditorApplication.update+=Update;}
        [MenuItem("Meridian/Small craft - Check Four Controls")]
        public static void RunChecks()
        {
            checks=0;Directory.CreateDirectory(Folder);
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2622,1206),new Vector2Int(852,393)})
            foreach(bool four in new[]{false,true})
            {
                Rect safe=new(48,12,size.x-96,size.y-24);HelmLayout layout=new(size.x,size.y,safe,four);
                Rect[] panels=four?new[]{layout.Port,layout.Starboard,layout.PortInner,layout.StarboardInner,layout.Master,layout.Rudder,layout.RollInstrument,layout.RouteMap,layout.Header,layout.Status}:new[]{layout.Port,layout.Starboard,layout.Master,layout.Rudder,layout.RollInstrument,layout.RouteMap,layout.Header,layout.Status};
                for(int i=0;i<panels.Length;i++)
                {
                    Rect p=layout.Physical(panels[i]);
                    Check(p.x>=safe.x-.01f&&p.xMax<=safe.xMax+.1f&&p.y>=size.y-safe.yMax-.01f&&p.yMax<=size.y-safe.y+.1f,"safe area "+size+"/"+four+"/"+i);
                    Check(layout.BlocksCamera(panels[i].center),"camera exclusion "+four+"/"+i);
                    for(int j=i+1;j<panels.Length;j++)Check(!panels[i].Overlaps(panels[j]),"separated controls "+size+"/"+four+"/"+i+"/"+j);
                }
                Check(HelmPointerRouter.Hit(layout,HelmLayout.Rail(layout.Port).center)==0&&HelmPointerRouter.Hit(layout,HelmLayout.Rail(layout.Starboard).center)==1,"outer controls "+four);
                Check(HelmPointerRouter.Hit(layout,HelmLayout.Rail(layout.Master).center)==2,"master "+four);
                Check(HelmPointerRouter.Hit(layout,HelmLayout.SteeringRail(layout.Rudder).center)==3,"rudder "+four);
                if(four)
                {
                    Check(HelmPointerRouter.Hit(layout,HelmLayout.Rail(layout.PortInner).center)==4&&HelmPointerRouter.Hit(layout,HelmLayout.Rail(layout.StarboardInner).center)==5,"independent inner controls");
                    Check(layout.PortInner.width<layout.Port.width&&layout.PortInner.height<layout.Port.height,"smaller inner levers");
                    Check(Mathf.Abs((layout.Rudder.x+layout.Master.xMax)*.5f-layout.Width*.5f)<.01f,"central group centered");
                    HelmLayout legacy=new(size.x,size.y,safe);
                    Check(layout.Rudder.width<legacy.Rudder.width&&layout.Master.height<legacy.Master.height,"smaller central controls");
                    for(int c=0;c<4;c++)
                    {
                        Rect panel=c==0?layout.Port:c==1?layout.Starboard:c==2?layout.PortInner:layout.StarboardInner;
                        Check(HelmPointerRouter.Hit(layout,new Vector2(panel.center.x,panel.y+25))==-1,"title never accelerates "+c);
                    }
                }
                else Check(layout.PortInner==default&&layout.StarboardInner==default,"inner controls absent on other ships");
            }
            HelmPointerRouter router=new();
            Check(router.Begin(10,0)&&router.Begin(11,1)&&router.Begin(12,4)&&router.Begin(13,5)&&router.Begin(14,3),"four fingers plus rudder");
            Check(!router.Begin(15,4),"inner lever single owner");
            Check(router.Begin(16,2)&&router.Count==2&&router.TryGet(14,out int retained)&&retained==3,"master takes four, retains rudder");
            foreach(int finger in new[]{10,11,12,13})Check(!router.TryGet(finger,out _),"master releases previous finger "+finger);
            Check(!router.Begin(17,4)&&!router.Begin(18,5),"master locks inner controls");router.End(16);
            Check(router.Begin(17,4)&&router.Begin(18,5),"inner ownership released");router.Clear();Check(router.Count==0,"clear all touches");
            foreach(VesselId id in VesselCatalog.All)
            {
                var definition=VesselCatalog.Get(id);Check(definition.HasFourEngines==(id==VesselId.Ruzgar),"exclusive four engine identity "+id);
                HelmLayout.Configure(definition);Check(HelmLayout.Current.FourEngines==definition.HasFourEngines,"live layout follows vessel "+id);
            }
            var active=Object.FindAnyObjectByType<ShipController>();HelmLayout.Configure(active!=null?active.Definition:VesselCatalog.Get(VesselId.Barbaros));
            Check(Apply!=null,"production pointer handler found");
            File.WriteAllText(Folder+"/logic-result.txt",$"FOUR_CONTROL_LOGIC_PASS checks={checks}\n");Debug.Log($"FOUR_CONTROL_LOGIC_PASS checks={checks}");
        }
        [MenuItem("Meridian/Small craft - Run Four Control Review")]
        public static void StartReview(){RunChecks();SessionState.SetBool("Meridian.FourControls",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;}
        private static void Pointer(PrototypeHud hud,HelmLayout layout,int control,float value)
        {
            Rect panel=control==0?layout.Port:control==1?layout.Starboard:control==4?layout.PortInner:control==5?layout.StarboardInner:layout.Master;
            Rect rail=HelmLayout.Rail(panel);Vector2 point=new(rail.center.x,rail.y+(1-ThrottleDetents.ToTravel(value))*rail.height);
            Apply.Invoke(hud,new object[]{layout,point,control});
        }
        private static void StartEngine(GameSessionController session,int index)
        {
            session.StartMode(SimulationMode.Harbor,VesselId.Ruzgar,new SeaStateSettings(1,0));
            ShipController ship=session.PlayerShip;ship.GetComponent<ShipBuoyancy>().HorizontalForcesEnabled=false;
            ship.ResetSimulation(ship.transform.position,Quaternion.identity);ship.SetEngineThrottle(index,1);deadline=EditorApplication.timeSinceStartup+4;
        }
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.FourControls",false))
            {
                SessionState.SetBool("Meridian.FourControls",false);checks=0;stage=0;Directory.CreateDirectory(Folder);
                File.WriteAllText(Folder+"/measurements.txt","");
                foreach(VesselId id in VesselCatalog.All)
                {
                    session.StartMode(SimulationMode.Harbor,id,new SeaStateSettings(1,0));ShipController ship=session.PlayerShip;
                    var hud=session.GetComponent<PrototypeHud>();HelmLayout layout=HelmLayout.Current;
                    Check(layout.FourEngines==(id==VesselId.Ruzgar)&&ship.EngineCount==(layout.FourEngines?4:2),"vessel specific UI "+id);
                    Pointer(hud,layout,0,.7f);Pointer(hud,layout,1,-.4f);
                    Check(ship.GetEngineThrottle(0)==.7f&&ship.GetEngineThrottle(1)==-.4f,"production outer pointer "+id);
                    if(layout.FourEngines)
                    {
                        Check(ship.GetEngineThrottle(2)==0&&ship.GetEngineThrottle(3)==0,"outer levers leave inner untouched");
                        Pointer(hud,layout,4,.3f);Pointer(hud,layout,5,.8f);
                        Check(ship.GetEngineThrottle(2)==.3f&&ship.GetEngineThrottle(3)==.8f&&ship.GetEngineThrottle(0)==.7f&&ship.GetEngineThrottle(1)==-.4f,"four production levers independent");
                        Check(!ship.AllEnginesSynchronized,"four way split readout");
                        ship.EmergencyStop();Pointer(hud,layout,4,.5f);
                        Check(!ship.AllEnginesSynchronized&&ship.CombinedThrottle>0&&ship.PortCommandLoad>0,"inner only load cannot read STOP");
                    }
                    Pointer(hud,layout,2,.9f);for(int i=0;i<ship.EngineCount;i++)Check(ship.GetEngineThrottle(i)==.9f,"master ahead "+id+"/"+i);
                    Pointer(hud,layout,2,-.8f);for(int i=0;i<ship.EngineCount;i++)Check(ship.GetEngineThrottle(i)==-.8f,"master astern "+id+"/"+i);
                    ship.SetRudder(.5f);ship.EmergencyStop();for(int i=0;i<ship.EngineCount;i++)Check(ship.GetEngineThrottle(i)==0,"all stop "+id+"/"+i);
                    Check(ship.Rudder==.5f,"stop leaves helm "+id);
                    bool rejected=false;try{ship.SetEngineThrottle(ship.EngineCount,1);}catch(ArgumentOutOfRangeException){rejected=true;}Check(rejected,"reject nonexistent engine "+id);
                    ship.SetCombinedThrottle(.6f);session.OpenMenu();Check(Time.timeScale==0&&ship.CombinedThrottle==.6f,"pause retains command "+id);session.Resume();
                    session.StartMode(SimulationMode.StraitPassage,id);Check(ship.CombinedThrottle==0&&ship.Rudder==0,"new mode clears all "+id);
                }
                engine=0;StartEngine(session,engine);return;
            }
            if(stage<0||EditorApplication.timeSinceStartup<deadline)return;
            ShipController current=session.PlayerShip;
            if(stage==0)
            {
                yaw[engine]=Mathf.DeltaAngle(0,current.Heading);
                Check(engine%2==0?yaw[engine]>.10f:yaw[engine]<-.10f,"physical differential shaft yaw "+engine+" angle="+yaw[engine]);
                Check(current.GetEngineSignedLoad(engine)>.9f,"physical engine spools up "+engine);
                File.AppendAllText(Folder+"/measurements.txt",$"engine={engine} yaw={yaw[engine]:F3} speed={current.SpeedKnots:F3}\n");
                engine++;if(engine<4){StartEngine(session,engine);return;}
                Check(yaw[0]>yaw[2]*1.5f&&Mathf.Abs(yaw[1])>Mathf.Abs(yaw[3])*1.5f,"outer shaft greater lever arm than inner");
                current.GetComponent<ShipBuoyancy>().HorizontalForcesEnabled=true;
                current.SetRudder(.4f);current.EmergencyStop();
                Check(current.GetEngineSignedLoad(3)>.9f&&current.Rudder==.4f,"stop commands zero, does not teleport engine RPM or helm");
                session.StartMode(SimulationMode.Harbor,VesselId.Ruzgar);current.SetEngineThrottle(0,.7f);current.SetEngineThrottle(1,-.4f);current.SetEngineThrottle(2,.3f);current.SetEngineThrottle(3,.8f);
                current.SetRudder(.25f);stage=1;deadline=EditorApplication.timeSinceStartup+1;return;
            }
            if(stage==1){ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/ruzgar-four-controls.png"));stage=2;}
            else if(stage==2){session.StartMode(SimulationMode.Harbor,VesselId.Kumkale);current.SetCombinedThrottle(.5f);stage=3;}
            else if(stage==3){Check(!HelmLayout.Current.FourEngines&&current.EngineCount==2,"return to two controls");ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/kumkale-two-controls.png"));stage=4;}
            else if(stage==4){session.StartMode(SimulationMode.Harbor,VesselId.Ruzgar);session.OpenMenu();session.GetComponent<ModeSelectionMenu>().SelectVessel(VesselId.Ruzgar);stage=5;}
            else if(stage==5)
            {
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/four-vessel-menu.png"));
                File.WriteAllText(Folder+"/runtime-result.txt",$"FOUR_CONTROL_RUNTIME_PASS checks={checks}\n");Debug.Log($"FOUR_CONTROL_RUNTIME_PASS checks={checks}");stage=-1;
            }
            deadline=EditorApplication.timeSinceStartup+.6;
        }
    }
}
