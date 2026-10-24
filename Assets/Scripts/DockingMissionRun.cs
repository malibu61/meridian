using UnityEngine;
namespace Meridian
{
    public enum DockingMissionPhase{Briefing,Running,Passed,Failed}
    /// <summary>Deterministic mission state; no scene mutations or persistence in this class.</summary>
    public sealed class DockingMissionRun
    {
        public DockingMissionDefinition Definition{get;}public DockingMissionPhase Phase{get;private set;}
        public ManOverboardRun Rescue{get;}
        public ManeuverSignalMonitor Signals{get;}=new();public HarborBerth Berth{get;private set;}
        public float Elapsed{get;private set;}public int GatesPassed{get;private set;}public int Contacts{get;private set;}
        public int SpeedViolations{get;private set;}public float HoldSeconds{get;private set;}public string FailureReason{get;private set;}
        public bool Reassigned{get;private set;}public bool TargetChanged{get;private set;}public bool Entered{get;private set;}
        public bool MooringComplete{get;private set;}
        public float BowTugSeconds{get;private set;}public float SternTugSeconds{get;private set;}
        public float SoloTugSeconds{get;private set;}public float IndependentEngineSeconds{get;private set;}
        public bool TugWorkReady=>!Definition.RequiresTugs||BowTugSeconds>=3&&SternTugSeconds>=3&&(Definition.TugExercise==TugTraining.Both||SoloTugSeconds>=3);
        public bool ApproachWorkReady=>TugWorkReady&&(!Definition.IndependentEngines||IndependentEngineSeconds>=3);
        public string ExerciseHint=>!TugWorkReady?(Definition.TugExercise!=TugTraining.Both&&SoloTugSeconds<3?$"ÖNCE TEK {(Definition.TugExercise==TugTraining.BowSolo?"BAŞ":"KIÇ")} RÖMORKÖRÜ {SoloTugSeconds:0.0}/3 sn":$"GERÇEK İTME · BAŞ {BowTugSeconds:0.0}/3 · KIÇ {SternTugSeconds:0.0}/3 sn"):!ApproachWorkReady?$"İÇ/DIŞ MAKİNE FARKLI YÜK · {IndependentEngineSeconds:0.0}/3 sn · 5 kn altı":"YAKLAŞMA ÇALIŞMASI TAMAM · STOP VE İKİ HALAT";
        public void RecordTugDelivery(float dt,float bow,float stern,HarborBerth berth)
        {
            if(Phase!=DockingMissionPhase.Running||!Definition.RequiresTugs||GatesPassed<1||berth!=Berth||!float.IsFinite(dt)||dt<=0||dt>.25f||!float.IsFinite(bow)||!float.IsFinite(stern))return;
            bool b=bow>.02f,s=stern>.02f;
            if(Definition.TugExercise==TugTraining.BowSolo&&b&&!s||Definition.TugExercise==TugTraining.SternSolo&&s&&!b)SoloTugSeconds=Mathf.Min(3,SoloTugSeconds+dt);
            if(Definition.TugExercise!=TugTraining.Both&&SoloTugSeconds<3)return;
            if(b)BowTugSeconds=Mathf.Min(3,BowTugSeconds+dt);if(s)SternTugSeconds=Mathf.Min(3,SternTugSeconds+dt);
        }
        public void RecordEngineHandling(float dt,float innerOuterDifference,bool inApproach)
        {if(Phase==DockingMissionPhase.Running&&Definition.IndependentEngines&&GatesPassed>=1&&inApproach&&float.IsFinite(dt)&&dt>0&&dt<=.25f&&float.IsFinite(innerOuterDifference)&&innerOuterDifference>=.15f)IndependentEngineSeconds=Mathf.Min(3,IndependentEngineSeconds+dt);}
        public void SetMooringComplete(bool complete){if(Phase==DockingMissionPhase.Running)MooringComplete=complete;}
        public Vector3 Target=>Definition.Target(Berth);public float LastForwardKnots{get;private set;}public float LastSwayKnots{get;private set;}
        public float LateralError{get;private set;}public float LongitudinalError{get;private set;}public float HeadingError{get;private set;}
        private Vector3 previous;private float speeding,lastCollision=-100;private bool speedingEpisode;
        public int Stars=>Phase!=DockingMissionPhase.Passed?0:Mathf.Max(1,3-(Contacts>0?1:0)-(SpeedViolations>0||Signals.WrongSignals>0?1:0));
        public DockingMissionRun(DockingMissionDefinition definition){Definition=definition;Berth=definition.InitialBerth;previous=definition.Spawn;Phase=DockingMissionPhase.Briefing;if(definition.IsRescue)Rescue=new ManOverboardRun(definition);}
        public bool Begin(){if(Phase!=DockingMissionPhase.Briefing)return false;Phase=DockingMissionPhase.Running;Rescue?.Begin();return true;}
        public void TickRescue(float dt,Vector3 position,float heading,float speed,bool stopped,float yaw,float roll)
        {
            if(Phase!=DockingMissionPhase.Running||Rescue==null||!float.IsFinite(dt)||dt<=0||dt>1)return;
            int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.1f));
            for(int i=0;i<steps;i++)Rescue.Tick(dt/steps,position,heading,speed,stopped,yaw,roll);
            Elapsed=Rescue.Elapsed;
            if(Rescue.Failure!=null)Fail(Rescue.Failure);else if(Rescue.Completed)Phase=DockingMissionPhase.Passed;
        }
        public void Tick(float dt,Vector3 position,float heading,float forwardKnots,float swayKnots,bool asternPropulsion,float actualRudder,float differential=0)
        {
            if(Phase!=DockingMissionPhase.Running||Definition.IsRescue||!float.IsFinite(dt)||dt<=0)return;
            if(!float.IsFinite(position.x)||!float.IsFinite(position.z)||!float.IsFinite(heading)||!float.IsFinite(forwardKnots)||!float.IsFinite(swayKnots)){Fail("Geçersiz simülasyon verisi.");return;}
            Elapsed+=dt;LastForwardKnots=forwardKnots;LastSwayKnots=swayKnots;TargetChanged=false;
            Signals.Tick(dt,Elapsed,asternPropulsion,actualRudder,heading,forwardKnots,true,differential);
            if(Signals.Violated){Fail("Zorunlu manevra düdüğü süresi içinde verilmedi. Doğru sinyal ile yeniden deneyin.");return;}
            if(Mathf.Abs(position.x)>90||position.z>175||position.z<-240){Fail("Görev yaklaşma sahasından çıktınız.");return;}
            if(position.z>0&&Mathf.Sqrt(forwardKnots*forwardKnots+swayKnots*swayKnots)>Definition.HarborSpeedLimit)
            {speeding+=dt;if(speeding>=3&&!speedingEpisode){SpeedViolations++;speedingEpisode=true;}}
            else{speeding=0;speedingEpisode=false;}
            Vector2 a=new(previous.x,previous.z),b=new(position.x,position.z);
            while(GatesPassed<3&&position.z>previous.z)
            {
                Vector3 gate=DockingMissionCatalog.Gate(GatesPassed);Vector2 c=new(gate.x,gate.z),v=b-a;
                float t=v.sqrMagnitude>1e-8f?Mathf.Clamp01(Vector2.Dot(c-a,v)/v.sqrMagnitude):0;
                if(Vector2.Distance(a+v*t,c)>DockingMissionCatalog.GateRadius(GatesPassed)||previous.z>gate.z+DockingMissionCatalog.GateRadius(GatesPassed))break;
                GatesPassed++;
            }
            Entered=GatesPassed>=1;
            if(Definition.Reassign&&!Reassigned&&GatesPassed>=2){Reassigned=true;Berth=HarborBerth.West;TargetChanged=true;HoldSeconds=0;MooringComplete=false;BowTugSeconds=SternTugSeconds=SoloTugSeconds=IndependentEngineSeconds=0;}
            LateralError=Mathf.Abs(position.x-Target.x)*4;LongitudinalError=Mathf.Abs(position.z-Target.z)*4;HeadingError=Mathf.Abs(Mathf.DeltaAngle(heading,0));
            bool settled=MooringComplete&&ApproachWorkReady&&GatesPassed==3&&LateralError<3&&LongitudinalError<(Definition.TightBerth?4.8f:8)&&HeadingError<6&&Mathf.Abs(forwardKnots)<.3f&&Mathf.Abs(swayKnots)<.15f&&Signals.PendingCount==0;
            HoldSeconds=settled?HoldSeconds+dt:0;if(HoldSeconds>=5)Phase=DockingMissionPhase.Passed;
            previous=position;
        }
        public void SoundCompleted(WhistleSignal signal){if(Phase==DockingMissionPhase.Running)Signals.SoundCompleted(signal,Elapsed);}
        public void Collision(float horizontalMetresPerSecond)
        {
            if(Phase!=DockingMissionPhase.Running||Elapsed-lastCollision<2)return;lastCollision=Elapsed;Contacts++;
            if(horizontalMetresPerSecond>.65f)Fail("Sert temas / çarpışma. Yaklaşma süratinizi daha erken azaltın.");
        }
        public void Fail(string reason){if(Phase!=DockingMissionPhase.Running)return;FailureReason=reason;Phase=DockingMissionPhase.Failed;HoldSeconds=0;Rescue?.Stop();}
    }
}
