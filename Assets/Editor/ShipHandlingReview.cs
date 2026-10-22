using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    /// <summary>Production forces integrated in isolated Unity PhysicsScenes, not a second mathematical mock.</summary>
    public static class ShipHandlingReview
    {
        private const string Folder="Temp/ShipHandlingReview";
        private const float Dt=.02f;
        private static int stage=-1,checks,failures;
        private static readonly float[,] wavePeaks=new float[4,4];
        private static readonly float[] speeds=new float[4];
        private static int fixtureSerial;
        private static AsyncOperation unloading;
        [InitializeOnLoadMethod] private static void Register(){EditorApplication.update-=Update;EditorApplication.update+=Update;}
        [MenuItem("Meridian/Handling - Run Speed Turn and Roll Review")]
        public static void StartReview(){Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/result.txt","RUNNING\n");SessionState.SetBool("Meridian.HandlingReview",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;}
        private static void Check(bool ok,string name){checks++;if(!ok){failures++;File.AppendAllText(Folder+"/failures.txt",name+"\n");Debug.LogError("Handling: "+name);}}
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            if(unloading!=null&&!unloading.isDone)return;
            if(SessionState.GetBool("Meridian.HandlingReview",false))
            {SessionState.SetBool("Meridian.HandlingReview",false);stage=0;checks=failures=0;File.WriteAllText(Folder+"/measurements.txt","");File.WriteAllText(Folder+"/failures.txt","");}
            if(stage<0)return;
            SeaStateSettings saved=WaterWaves.State;
            try
            {
                if(stage<4)Speed(VesselCatalog.All[stage],1,false);
                else if(stage<8)Speed(VesselCatalog.All[stage-4],-1,false);
                else if(stage<11)Speed(VesselId.Ruzgar,(stage-7)*.2f,false);
                else if(stage<19)Turn(VesselCatalog.All[(stage-11)/2],(stage-11)%2==0?-1:1);
                else if(stage<35)Waves(VesselCatalog.All[(stage-19)/4],5,((stage-19)%4)*90,(stage-19)%4);
                else if(stage<39)Waves(VesselId.Ruzgar,stage-34,90,-1);
                else
                {
                    Check(speeds[(int)VesselId.Ruzgar]>speeds[(int)VesselId.Barbaros]&&speeds[(int)VesselId.Barbaros]>speeds[(int)VesselId.Kumkale]&&speeds[(int)VesselId.Kumkale]>speeds[(int)VesselId.Anadolu],"distinct real-scale maximum speed order");
                    foreach(VesselId id in VesselCatalog.All)
                    {
                        float beam=Mathf.Min(wavePeaks[(int)id,1],wavePeaks[(int)id,3]),head=Mathf.Max(wavePeaks[(int)id,0],wavePeaks[(int)id,2]);
                        Check(beam>head*2,"beam greater than head/following "+id+" beam="+beam+" head="+head);
                        if(VesselCatalog.Get(id).IsSmallCraft)Check(beam>=30,"severe beam sea reaches thirty degrees "+id+" beam="+beam);
                    }
                    string result=failures==0?$"SHIP_HANDLING_PASS checks={checks} cases=39 dt={Dt}":$"FAILED checks={checks} cases=39 failures={failures}";
                    File.WriteAllText(Folder+"/result.txt",result+"\n");Debug.Log(result);stage=-1;return;
                }
                stage++;
            }
            catch(Exception e){File.WriteAllText(Folder+"/result.txt",$"FAILED stage={stage} checks={checks}\n{e}\n");stage=-1;Debug.LogException(e);}
            finally{WaterWaves.Reset(saved,Time.time);}
        }
        private sealed class Fixture:IDisposable
        {
            public readonly Scene Scene;
            public readonly PhysicsScene Physics;
            public readonly ShipController Ship;
            public readonly ShipBuoyancy Buoyancy;
            public readonly Rigidbody Body;
            public float Time;
            public Fixture(VesselId id,SeaStateSettings sea,bool fixedHeading=false)
            {
                Scene=SceneManager.CreateScene("Meridian isolated handling fixture "+(++fixtureSerial),new CreateSceneParameters(LocalPhysicsMode.Physics3D));Physics=Scene.GetPhysicsScene();
                GameObject go=new("Temporary handling fixture");SceneManager.MoveGameObjectToScene(go,Scene);
                var definition=VesselCatalog.Get(id);var collider=go.AddComponent<BoxCollider>();collider.size=definition.ColliderSize;collider.center=definition.ColliderCenter;
                Ship=go.AddComponent<ShipController>();Ship.ConfigureVessel(definition);Buoyancy=go.AddComponent<ShipBuoyancy>();Buoyancy.HorizontalForcesEnabled=false;
                Body=go.GetComponent<Rigidbody>();Body.interpolation=RigidbodyInterpolation.None;
                if(fixedHeading)Body.constraints=RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotationY;
                Ship.ResetSimulation(new Vector3(0,.1f,0),Quaternion.identity);WaterWaves.Reset(sea,0);
            }
            public void Tick(){Ship.SimulateForces(Dt);Buoyancy.SimulateForces(Time);Physics.Simulate(Dt);Time+=Dt;}
            public void Advance(float seconds){for(int i=0,n=Mathf.CeilToInt(seconds/Dt);i<n;i++)Tick();}
            public void Dispose(){unloading=SceneManager.UnloadSceneAsync(Scene);}
        }
        private static void Speed(VesselId id,float command,bool unused)
        {
            using Fixture f=new(id,new SeaStateSettings(1,0));var d=f.Ship.Definition;
            f.Ship.SetCombinedThrottle(command);f.Advance(d.AccelerationSeconds*(command>0&&command<1?16:6)+5);
            float equilibrium=(-.15f+Mathf.Sqrt(.0225f+3.4f*command*command))/1.7f;
            float actual=f.Ship.SpeedKnots,expected=command<0?d.MaximumAsternKnots:equilibrium*d.MaximumAheadKnots;
            File.AppendAllText(Folder+"/measurements.txt",$"speed {id} command={command:F2} measured={actual:F3} target={expected:F3} knots\n");
            Check(float.IsFinite(actual)&&Mathf.Abs(actual-expected)<Mathf.Max(.3f,expected*.06f),"calibrated surge "+id+" "+command+" actual="+actual+" target="+expected);
            Check(command<0?Vector3.Dot(f.Body.linearVelocity,f.Ship.ForwardOnWater)<0:Vector3.Dot(f.Body.linearVelocity,f.Ship.ForwardOnWater)>0,"correct signed propulsion "+id);
            if(command==1)speeds[(int)id]=actual;
        }
        private static void Turn(VesselId id,int sign)
        {
            using Fixture f=new(id,new SeaStateSettings(1,0));ShipController ship=f.Ship;var d=ship.Definition;
            ship.SetCombinedThrottle(1);f.Advance(d.AccelerationSeconds*5+5);
            ship.ResetSimulation(new Vector3(0,.1f,0),Quaternion.identity);ship.SetCombinedThrottle(1);f.Body.linearVelocity=Vector3.forward*d.MaximumWorldSpeed;
            f.Advance(4);float heading=ship.Heading;ship.SetRudder(sign);
            float start=f.Time,accumulated=0,tactical=0,advance=0,meanRoll=0;int rollSamples=0;float previous=heading,initialYaw=0;
            Vector3 origin=f.Body.position;float maxRadiusError=0;bool sampled180=false;
            // Two circles; the first includes rudder travel and yaw build-up, the second checks steady curvature.
            float sumX=0,sumZ=0;int circleSamples=0;System.Collections.Generic.List<Vector3> path=new();
            for(int i=0;i<60000&&accumulated<720;i++)
            {
                f.Tick();float current=ship.Heading;accumulated+=Mathf.DeltaAngle(previous,current)*sign;previous=current;
                float elapsed=f.Time-start;
                if(elapsed<1)initialYaw=Mathf.Max(initialYaw,Mathf.Abs(accumulated));
                if(accumulated<90)advance=Mathf.Max(advance,f.Body.position.z-origin.z);
                if(!sampled180&&accumulated>=180){tactical=Mathf.Abs(f.Body.position.x-origin.x);sampled180=true;}
                if(accumulated>90&&accumulated<270){meanRoll+=f.Buoyancy.RollDegrees;rollSamples++;}
                if(accumulated>360&&i%20==0){path.Add(f.Body.position);sumX+=f.Body.position.x;sumZ+=f.Body.position.z;circleSamples++;}
            }
            float mean=meanRoll/Mathf.Max(1,rollSamples),length=d.ColliderSize.z;
            Check(accumulated>=720&&sampled180,"two complete circles "+id+" sign="+sign+" angle="+accumulated);
            Check(initialYaw<2,"rudder does not instantly rotate ship "+id+" initial="+initialYaw);
            Check(tactical/length>2&&tactical/length<6.5f,"bounded tactical circle "+id+" diameter/L="+tactical/length);
            Check(mean*sign>.4f,"outside heel during turn "+id+" heel="+mean);
            Vector3 centre=new(sumX/circleSamples,0,sumZ/circleSamples);float radius=0;
            foreach(Vector3 p in path)radius+=Vector2.Distance(new(p.x,p.z),new(centre.x,centre.z));radius/=circleSamples;
            foreach(Vector3 p in path)maxRadiusError=Mathf.Max(maxRadiusError,Mathf.Abs(Vector2.Distance(new(p.x,p.z),new(centre.x,centre.z))-radius));
            Check(maxRadiusError/radius<.18f,"steady circular track "+id+" error="+maxRadiusError/radius);
            File.AppendAllText(Folder+"/measurements.txt",$"turn {id} sign={sign} tactical={tactical*4:F2}m diameter/L={tactical/length:F3} advance/L={advance/length:F3} heel={mean:F3}deg radialError={maxRadiusError/radius:P2}\n");
            using StreamWriter csv=new(Folder+$"/turn-{id}-{sign}.csv");csv.WriteLine("x_m,z_m");foreach(Vector3 p in path)csv.WriteLine((p.x*VesselCatalog.ModelMetresPerUnit).ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+(p.z*VesselCatalog.ModelMetresPerUnit).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
        }
        private static void Waves(VesselId id,int level,int bearing,int directionSlot)
        {
            using Fixture f=new(id,new SeaStateSettings(level,bearing),true);
            float maximum=0,squares=0,pitchMaximum=0;int samples=0;
            for(int i=0;i<7500;i++)
            {
                f.Tick();if(i<500)continue;float roll=f.Buoyancy.RollDegrees;
                maximum=Mathf.Max(maximum,Mathf.Abs(roll));squares+=roll*roll;samples++;
                pitchMaximum=Mathf.Max(pitchMaximum,Mathf.Abs(f.Buoyancy.PitchDegrees));
            }
            File.AppendAllText(Folder+"/measurements.txt",$"waves {id} level={level} from={bearing} peak={maximum:F3} rollRms={Mathf.Sqrt(squares/samples):F3} pitchPeak={pitchMaximum:F3}deg\n");
            Check(float.IsFinite(maximum)&&maximum<f.Ship.Definition.MaximumGameRoll+12,"finite stable severe roll "+id+" "+bearing+" max="+maximum);
            if(directionSlot>=0)wavePeaks[(int)id,directionSlot]=maximum;
        }
    }
}
