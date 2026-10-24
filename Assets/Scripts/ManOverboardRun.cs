using UnityEngine;
namespace Meridian
{
    public enum RescueStage{Alarm,Mark,Approach,Recovering,Recovered}
    /// <summary>Game exercise only. Horizontal geometry is planar; 1 unit = 4 m. No real-world recovery certification.</summary>
    public sealed class ManOverboardRun
    {
        public readonly VesselDefinition Vessel;public readonly int Side;public readonly Vector3 Casualty,FallStart;
        public readonly float FallSeconds;
        public const float StumbleSeconds=.45f;
        public bool InWater=>Elapsed>=FallSeconds;
        public float FallProgress=>Mathf.Clamp01(Elapsed/FallSeconds);
        public RescueStage Stage{get;private set;}=RescueStage.Alarm;
        public float Elapsed{get;private set;}public float StableSeconds{get;private set;}public float RecoverySeconds{get;private set;}
        public float DistanceMetres{get;private set;}public float RelativeBearing{get;private set;}public float StationGapMetres{get;private set;}
        public string Failure{get;private set;}public string HoldReason{get;private set;}="Önce alarm ver";
        public bool Active{get;private set;}public bool Completed=>Stage==RescueStage.Recovered;
        public bool CanRecover=>Active&&InWater&&Stage==RescueStage.Approach&&StableSeconds>=4&&HoldReason==null;
        public bool Marked=>Stage>=RescueStage.Approach;
        private Vector3 previousPosition;private float previousHeading;private bool hasPrevious;
        public Vector3 LocalStation=>new(Side*(Vessel.BeamMetres/8+.35f),DeckHeight+.2f,0);
        public float DeckHeight=>Vessel.IsSmallCraft?SmallCraftHull.DeckHeight(Vessel.Id,0):Vessel.UsesPods?AnadoluModelBuilder.FlightDeckHeight(0,0):1.035f;
        public ManOverboardRun(DockingMissionDefinition definition)
        {
            Vessel=VesselCatalog.Get(definition.Vessel);Side=definition.RescueSide;
            float halfBeam=Vessel.BeamMetres/8;
            FallStart=definition.Spawn+new Vector3(Side*(halfBeam-.06f),DeckHeight+.26f,-Vessel.LengthMetres/4*.18f);
            // Scaled gravity (1 unit = 4 m). Start on deck and retain the ship's surge while falling.
            FallSeconds=StumbleSeconds+Mathf.Sqrt(2*Mathf.Max(.1f,FallStart.y-WaterWaves.SeaLevel)/(9.81f/VesselCatalog.ModelMetresPerUnit));
            float surge=definition.InitialKnots/(VesselCatalog.ModelMetresPerUnit*1.9438445f);
            Casualty=new Vector3(definition.Spawn.x+Side*(halfBeam+1.65f),0,FallStart.z+surge*FallSeconds);
        }
        public Vector3 FallPosition(float waterHeight)
        {
            float t=FallProgress;Vector3 p=Vector3.Lerp(FallStart,Casualty,t);
            float drop=Mathf.Clamp01((Elapsed-StumbleSeconds)/(FallSeconds-StumbleSeconds));
            p.y=Mathf.Lerp(FallStart.y,waterHeight,drop*drop);return p;
        }
        public void Begin(){Active=true;}
        public bool Alarm(){if(!Active||Failure!=null||Stage!=RescueStage.Alarm)return false;Stage=RescueStage.Mark;return true;}
        public bool Mark(){if(!Active||Failure!=null||Stage!=RescueStage.Mark)return false;Stage=RescueStage.Approach;return true;}
        public bool Recover(){if(!CanRecover)return false;Stage=RescueStage.Recovering;RecoverySeconds=0;return true;}
        public void Stop(){Active=false;}
        private static Vector2 Local(Vector3 p,Vector3 ship,float heading)
        {Vector3 local=Quaternion.Euler(0,-heading,0)*(p-ship);return new Vector2(local.x,local.z);}
        public bool DangerAt(Vector3 position,float heading)
        {
            Vector2 p=Local(Casualty,position,heading);float halfBeam=Vessel.BeamMetres/8,halfLength=Vessel.LengthMetres/8;
            return Mathf.Abs(p.x)<halfBeam+.15f&&Mathf.Abs(p.y)<halfLength || Mathf.Abs(p.x)<halfBeam+1.25f&&p.y<=-halfLength&&p.y>-halfLength-4;
        }
        public void Tick(float dt,Vector3 position,float heading,float speedKnots,bool stopped,float yawDegrees,float rollDegrees)
        {
            if(!Active||Failure!=null||Completed||!float.IsFinite(dt)||dt<=0||dt>.25f)return;
            if(!float.IsFinite(position.x)||!float.IsFinite(position.z)||!float.IsFinite(heading)||!float.IsFinite(speedKnots)||!float.IsFinite(yawDegrees)||!float.IsFinite(rollDegrees)){Failure="Geçersiz simülasyon verisi.";return;}
            Elapsed+=dt;Vector2 p=Local(Casualty,position,heading);
            DistanceMetres=p.magnitude*4;RelativeBearing=Mathf.Atan2(p.x,p.y)*Mathf.Rad2Deg;
            StationGapMetres=Vector2.Distance(p,new Vector2(LocalStation.x,0))*4;
            // The on-deck/falling actor is not yet a person-in-water collision target.
            if(!InWater){hasPrevious=false;HoldReason="Personel denize düşüyor · alarmı ver ve görüşte tut";return;}
            // Sweep the hull, not only the end point, so crossing a person in a long frame is never rewarded.
            int samples=hasPrevious?Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(previousPosition,position)/.25f+Mathf.Abs(Mathf.DeltaAngle(previousHeading,heading))/2),1,256):1;
            bool danger=DangerAt(position,heading);
            if(hasPrevious)for(int i=0;i<=samples&&!danger;i++)danger=DangerAt(Vector3.Lerp(previousPosition,position,i/(float)samples),Mathf.LerpAngle(previousHeading,heading,i/(float)samples));
            previousPosition=position;previousHeading=heading;hasPrevious=true;
            if(danger){Failure="Kazazede gövde / kıç-pervane tehlike sahasına girdi. Güvenli borda yaklaşmasını yeniden dene.";return;}
            if(Elapsed>1200||Stage==RescueStage.Alarm&&Elapsed>60){Failure=Stage==RescueStage.Alarm?"Alarm verilmedi. Kazazede ihbarını geciktirme.":"Eğitim süresi doldu; yeniden dene.";return;}
            HoldReason=Stage==RescueStage.Alarm?"1 · DENİZE ADAM DÜŞTÜ alarmını ver":Stage==RescueStage.Mark?"2 · MOB konumunu işaretle":
                Side*p.x<=Vessel.BeamMetres/8?"Kazazedeyi kurtarma bordasında tut":Mathf.Abs(p.y)>2.5f?"Kurtarma istasyonunu kazazedenin hizasına getir":StationGapMetres>6?"Kurtarma istasyonunu 6 m içine yaklaştır":
                Mathf.Abs(speedKnots)>=.5f?"Sürati 0,50 kn altına indir":!stopped?"Tüm makineler STOP · gerçek yükün düşmesini bekle":Mathf.Abs(yawDegrees)>=.8f?"Dönüşü durdur":Mathf.Abs(rollDegrees)>=10?"Yalpanın 10° altına düşmesini bekle":null;
            if(Stage==RescueStage.Approach)StableSeconds=HoldReason==null?Mathf.Min(4,StableSeconds+dt):0;
            if(Stage==RescueStage.Recovering)
            {
                if(HoldReason!=null){Stage=RescueStage.Approach;StableSeconds=RecoverySeconds=0;return;}
                RecoverySeconds=Mathf.Min(8,RecoverySeconds+dt);if(RecoverySeconds>=8){Stage=RescueStage.Recovered;Active=false;}
            }
        }
    }
}
