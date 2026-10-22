using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class SeaStateReview
    {
        private const string Folder="Temp/SeaStateReview";
        private static int stage=-1,checks,samples;
        private static double deadline;
        private static float rollSquares,pitchSquares,maxHeave,lowRoll,highRoll,beamRoll,headRoll,headPitch;
        private static Quaternion pausedRotation;
        private static Vector3 pausedPosition;
        private static bool waitingPause;
        [InitializeOnLoadMethod]
        private static void Register(){EditorApplication.update-=Update;EditorApplication.update+=Update;}
        [MenuItem("Meridian/Sea - Check Levels and Directions")]
        public static void RunChecks()
        {
            int count=0;void Check(bool condition,string name){count++;if(!condition)throw new InvalidOperationException("Sea check: "+name);}
            Check(!new SeaStateSettings(0,0).IsValid&&!new SeaStateSettings(6,0).IsValid&&!new SeaStateSettings(2,float.NaN).IsValid,"invalid settings");
            Check(new SeaStateSettings(2,-90).FromDegrees==270&&new SeaStateSettings(2,720).FromDegrees==0,"bearing wraps");
            float previous=0;
            for(int level=1;level<=5;level++)
            {
                SeaStateSettings s=new(level,90);Check(s.Amplitude>previous,"strictly increasing amplitude "+level);previous=s.Amplitude;
                for(int direction=0;direction<8;direction++)
                {
                    s=new(level,direction*45);Vector2 incoming=new(Mathf.Sin(s.FromDegrees*Mathf.Deg2Rad),Mathf.Cos(s.FromDegrees*Mathf.Deg2Rad));
                    Check(Vector2.Dot(incoming,s.TravelDirection)<-.999f,"from not towards "+level+"/"+direction);
                    float correct=0,opposite=0,phaseSpeed=Mathf.Sqrt(2.4f/(Mathf.PI*2/s.Wavelength));
                    for(int i=0;i<30;i++)
                    {
                        Vector2 p=new(i*1.73f,i*.91f),delta=s.TravelDirection*phaseSpeed*.4f;float h=WaterWaves.SampleHeight(s,p.x,p.y,2);
                        float a=WaterWaves.SampleHeight(s,p.x+delta.x,p.y+delta.y,2.4f),b=WaterWaves.SampleHeight(s,p.x-delta.x,p.y-delta.y,2.4f);
                        correct+=(h-a)*(h-a);opposite+=(h-b)*(h-b);
                        Check(Mathf.Abs(h)<=s.Amplitude+.001f,"bounded height "+level+"/"+direction+"/"+i);
                    }
                    Check(correct<opposite,"crest advances away from source "+level+"/"+direction);
                }
            }
            SeaStateSettings saved=WaterWaves.State;
            try
            {
                WaterWaves.Reset(new SeaStateSettings(1,0),0);WaterWaves.SetState(new SeaStateSettings(5,90),1);
                float before=WaterWaves.Height(7,13,2);Vector3 normal=WaterWaves.Normal(7,13,2);
                WaterWaves.SetState(new SeaStateSettings(3,225),2);
                Check(Mathf.Abs(before-WaterWaves.Height(7,13,2))<.00001f,"interrupted transition height stays continuous");
                Check(Vector3.Dot(normal,WaterWaves.Normal(7,13,2))>.99999f,"interrupted transition normal stays continuous");
            }
            finally{WaterWaves.Reset(saved,Time.time);}
            Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/logic-result.txt",$"SEA_LOGIC_CHECK_PASS checks={count}\n");Debug.Log($"SEA_LOGIC_CHECK_PASS checks={count}");
        }
        [MenuItem("Meridian/Sea - Run Physical Review")]
        public static void StartReview()
        {RunChecks();SessionState.SetBool("Meridian.SeaReview",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;}
        private static void Check(bool condition,string name)
        {checks++;if(!condition){stage=-1;throw new InvalidOperationException("Sea runtime: "+name);}}
        private static void Begin(GameSessionController session)
        {
            int level=stage<5?stage+1:5;float direction=stage==5?0:90;VesselId vessel=stage==6?VesselId.Anadolu:VesselId.Barbaros;
            Check(session.StartMode(stage==6?SimulationMode.StraitPassage:SimulationMode.Harbor,vessel,new SeaStateSettings(level,direction)),"launch "+stage);
            // Strait heading is not north. Rebase test fixture to north to compare beam/head response.
            if(stage==6)session.PlayerShip.ResetSimulation(StraitRoute.Point(0),Quaternion.identity);
            Check(session.PlayerShip.GetComponent<Rigidbody>().constraints==RigidbodyConstraints.None,"real physical freedom "+stage);
            Check(session.PlayerShip.GetComponent<ShipBuoyancy>()!=null,"buoyancy component "+stage);
            rollSquares=pitchSquares=maxHeave=0;samples=0;waitingPause=false;deadline=EditorApplication.timeSinceStartup+9;
        }
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            GameSessionController session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.SeaReview",false))
            {
                SessionState.SetBool("Meridian.SeaReview",false);stage=0;checks=0;
                ModeSelectionMenu menu=session.GetComponent<ModeSelectionMenu>();
                SeaStateSettings old=session.ActiveSea;
                Check(menu.SelectSea(new SeaStateSettings(5,90))&&session.ActiveSea.Equals(old),"pending selection leaves active sea alone");
                Check(!menu.SelectSea(new SeaStateSettings(9,0))&&!session.ApplySeaState(new SeaStateSettings(0,0)),"reject invalid");
                Check(!session.StartMode(SimulationMode.BuoyMooring,VesselId.Barbaros,new SeaStateSettings(5,90))&&session.ActiveSea.Equals(old),"unavailable mode preserves sea");
                Begin(session);return;
            }
            if(stage<0)return;
            ShipController ship=session.PlayerShip;ShipBuoyancy motion=ship.GetComponent<ShipBuoyancy>();
            if(waitingPause)
            {
                if(EditorApplication.timeSinceStartup<deadline)return;
                waitingPause=false;
                Check(Quaternion.Angle(ship.transform.rotation,pausedRotation)<.001f&&Vector3.Distance(ship.transform.position,pausedPosition)<.001f,"physical pause "+stage);
                stage++;if(stage<7){Begin(session);return;}
                Check(highRoll>lowRoll*1.5f&&highRoll>.35f,"strong sea rocks more than calm");
                Check(headPitch>.1f&&beamRoll>headRoll*1.15f,"incoming side changes response axis");
                // Applying sea during an existing voyage must preserve command/position/progress.
                session.Resume();ship.SetCombinedThrottle(.6f);Vector3 before=ship.transform.position;var voyage=session.Voyage;
                Check(session.ApplySeaState(new SeaStateSettings(3,225)),"apply mid voyage");
                Check(ship.PortThrottle==.6f&&ship.StarboardThrottle==.6f&&ship.transform.position==before&&session.Voyage==voyage,"no voyage reset on apply");
                session.OpenMenu();session.GetComponent<ModeSelectionMenu>().SelectSea(session.ActiveSea);
                stage=-1;File.WriteAllText(Folder+"/runtime-result.txt",$"SEA_RUNTIME_CHECK_PASS checks={checks}\nlowRoll={lowRoll:F3} highRoll={highRoll:F3} beamRoll={beamRoll:F3} headRoll={headRoll:F3} headPitch={headPitch:F3}\n");
                Debug.Log($"SEA_RUNTIME_CHECK_PASS checks={checks} lowRoll={lowRoll:F3} highRoll={highRoll:F3} headPitch={headPitch:F3}");return;
            }
            float roll=motion.RollDegrees,pitch=motion.PitchDegrees;
            if(Time.timeScale>0){rollSquares+=roll*roll;pitchSquares+=pitch*pitch;samples++;maxHeave=Mathf.Max(maxHeave,Mathf.Abs(ship.transform.position.y-.1f));}
            if(EditorApplication.timeSinceStartup<deadline)return;
            float rollRms=Mathf.Sqrt(rollSquares/Mathf.Max(1,samples)),pitchRms=Mathf.Sqrt(pitchSquares/Mathf.Max(1,samples));
            Check(samples>30&&!float.IsNaN(rollRms)&&!float.IsNaN(pitchRms),"finite sampled physics "+stage);
            Check(rollRms<22&&pitchRms<18&&maxHeave<2.8f,"stable bounded response "+stage);
            Check(rollRms+pitchRms>.02f,"body actually rotates "+stage);
            Check(Mathf.Abs(Mathf.DeltaAngle(0,Object.FindAnyObjectByType<FollowCamera>().transform.eulerAngles.z))<.01f,"level camera horizon "+stage);
            if(stage==0)lowRoll=rollRms;if(stage==4)highRoll=beamRoll=rollRms;if(stage==5){headRoll=rollRms;headPitch=pitchRms;}
            File.AppendAllText(Folder+"/measurements.txt",$"stage={stage} vessel={session.ActiveVessel} level={session.ActiveSea.Level} from={session.ActiveSea.FromDegrees} rollRms={rollRms:F3} pitchRms={pitchRms:F3} maxHeave={maxHeave:F3}\n");
            if(stage==4||stage==6)CapturePhysical(session);
            session.OpenMenu();pausedPosition=ship.transform.position;pausedRotation=ship.transform.rotation;waitingPause=true;deadline=EditorApplication.timeSinceStartup+.5;
        }
        [MenuItem("Meridian/Sea - Capture Menu")]
        public static void CaptureMenu()
        {
            GameSessionController session=Object.FindAnyObjectByType<GameSessionController>();if(session==null||!Application.isPlaying)return;
            Directory.CreateDirectory(Folder);session.OpenMenu();session.GetComponent<ModeSelectionMenu>().ShowSeaOptions();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/sea-menu.png"));
        }
        private static void CapturePhysical(GameSessionController session)
        {
            ShipController ship=session.PlayerShip;GameObject go=new("Temporary sea motion review");Camera camera=go.AddComponent<Camera>();
            camera.CopyFrom(Object.FindAnyObjectByType<FollowCamera>().GetComponent<Camera>());camera.enabled=false;
            Vector3 focus=ship.transform.position+Vector3.up*ship.Definition.CameraFocusHeight;
            camera.transform.position=focus+Quaternion.Euler(0,ship.Heading,0)*(Quaternion.Euler(12,90,0)*Vector3.back*(38*ship.Definition.CameraScale));camera.transform.LookAt(focus);
            RenderTexture previous=RenderTexture.active;RenderTexture rt=new(1280,720,24);Texture2D image=new(1280,720,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Folder+$"/sea-level5-{session.ActiveVessel}.png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(go);}
        }
    }
}
