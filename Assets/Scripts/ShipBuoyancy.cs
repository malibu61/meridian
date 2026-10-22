using UnityEngine;
namespace Meridian
{
    /// <summary>Force-driven heave, damped roll/pitch and wave drift. Not a certified RAO/GM model.</summary>
    [RequireComponent(typeof(ShipController),typeof(Rigidbody))]
    public sealed class ShipBuoyancy:MonoBehaviour
    {
        private ShipController ship;
        private Rigidbody body;
        public Vector3 LastWaveForce { get; private set; }
        public Vector3 LastWaveTorque { get; private set; }
        public bool HorizontalForcesEnabled { get; set; }=true;
        public float RollDegrees=>Vector3.SignedAngle(Vector3.up,Vector3.ProjectOnPlane(transform.up,ship.ForwardOnWater),ship.ForwardOnWater);
        public float PitchDegrees=>Mathf.Asin(Mathf.Clamp(transform.forward.y,-1,1))*Mathf.Rad2Deg;
        private void Awake(){ship=GetComponent<ShipController>();body=GetComponent<Rigidbody>();}
        private void FixedUpdate()=>SimulateForces(Time.time);
        public void SimulateForces(float time)
        {
            VesselDefinition definition=ship.Definition;
            Vector3 forward=ship.ForwardOnWater,right=ship.RightOnWater,centre=body.position;
            float halfBeam=definition.ColliderSize.x*.38f,halfLength=definition.ColliderSize.z*.32f;
            float meanHeight=0;Vector3 meanNormal=Vector3.zero;
            LastWaveForce=LastWaveTorque=Vector3.zero;
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            {
                // Horizontal supports avoid self-excitation when an edge rolls down into water.
                Vector3 point=centre+right*(x*halfBeam)+forward*(z*halfLength);
                meanHeight+=WaterWaves.Height(point.x,point.z,time)*.25f;
                if(!HorizontalForcesEnabled)continue;
                Vector3 water=WaterWaves.OrbitalVelocity(point,time),relative=water-body.GetPointVelocity(point)*.08f;
                relative.y=0;
                float wet=Mathf.Clamp01(.75f+(.1f+WaterWaves.Height(point.x,point.z,time)-centre.y)*.30f);
                float coupling=Mathf.Clamp01(WaterWaves.CurrentAmplitude(time)/.85f);
                Vector3 force=Vector3.ClampMagnitude(relative,3f)*(body.mass*.25f*(definition.UsesPods?.10f:.22f)*wet*coupling);
                Vector3 forcePoint=new(point.x,body.worldCenterOfMass.y,point.z);
                body.AddForceAtPosition(force,forcePoint,ForceMode.Force);
                LastWaveForce+=force;LastWaveTorque+=Vector3.Cross(forcePoint-body.worldCenterOfMass,force);
            }
            float heaveFrequency=definition.UsesPods?.85f:1.25f;
            float heave=(.1f+meanHeight-centre.y)*heaveFrequency*heaveFrequency-body.linearVelocity.y*1.8f*heaveFrequency;
            body.AddForce(Vector3.up*Mathf.Clamp(heave,-5,5),ForceMode.Acceleration);
            for(int i=-1;i<=1;i++)
            {Vector3 p=centre+forward*(i*halfLength);meanNormal+=WaterWaves.Normal(p.x,p.z,time);}
            meanNormal.Normalize();
            Vector2 direction=WaterWaves.DirectionAt(time);Vector3 travel=new(direction.x,0,direction.y);
            float beam=Mathf.Abs(Vector3.Dot(travel,right));
            Vector3 rollNormal=Vector3.ProjectOnPlane(meanNormal,forward).normalized;
            float waveRoll=Vector3.SignedAngle(Vector3.up,rollNormal,forward)*definition.WaveRollResponse*(.08f+.92f*beam*beam);
            float targetRoll=Mathf.Clamp(waveRoll+ship.TurnHeelDegrees,-definition.MaximumGameRoll,definition.MaximumGameRoll);
            float angle=RollDegrees,rate=Vector3.Dot(body.angularVelocity,forward),frequency=Mathf.PI*2/definition.RollPeriod;
            float damping=definition.RollDamping+Mathf.Clamp01(Mathf.Abs(angle)/definition.MaximumGameRoll)*.09f;
            float rollAcceleration=(targetRoll-angle)*Mathf.Deg2Rad*frequency*frequency-2*damping*frequency*rate;
            // Progressive righting, not a hard Euler clamp or a capsize-stability calculation.
            if(Mathf.Abs(angle)>definition.MaximumGameRoll)rollAcceleration-=Mathf.Sign(angle)*(Mathf.Abs(angle)-definition.MaximumGameRoll)*Mathf.Deg2Rad*frequency*frequency*4;
            Vector3 fore=centre+forward*halfLength,aft=centre-forward*halfLength;
            float targetPitch=Mathf.Atan2(WaterWaves.Height(fore.x,fore.z,time)-WaterWaves.Height(aft.x,aft.z,time),halfLength*2)*Mathf.Rad2Deg;
            float pitchFrequency=Mathf.PI*2/(definition.IsSmallCraft?4:definition.UsesPods?9:6);
            float pitchAcceleration=(Mathf.Clamp(targetPitch,-15,15)-PitchDegrees)*Mathf.Deg2Rad*pitchFrequency*pitchFrequency+Vector3.Dot(body.angularVelocity,right)*1.2f*pitchFrequency;
            body.AddTorque(forward*rollAcceleration-right*pitchAcceleration,ForceMode.Acceleration);
        }
    }
}
