using UnityEngine;

namespace Meridian
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipController : MonoBehaviour
    {
        [Header("Engines")]
        [SerializeField] private float maximumThrust = 6000f;
        [SerializeField] private float engineLongitudinalOffset = 12.2f;
        [SerializeField] private float engineResponsePerSecond = 0.45f;

        private Rigidbody body;
        // 0 port outer, 1 starboard outer, 2 port inner, 3 starboard inner.
        private readonly float[] engineTargets=new float[4],engineActuals=new float[4];
        private float rudderTarget,rudderActual;
        public VesselDefinition Definition { get; private set; } = VesselCatalog.Get(VesselId.Barbaros);
        public Vector3 ForwardOnWater => Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
        public Vector3 RightOnWater => Vector3.Cross(Vector3.up,ForwardOnWater);
        public Vector3 PropulsionDirection => Definition.UsesPods
            ? Quaternion.AngleAxis(-rudderActual * 35f, Vector3.up) * ForwardOnWater : ForwardOnWater;
        public float ActualRudder=>rudderActual;
        public float TurnHeelDegrees{get;private set;}
        public float DesiredYawRate{get;private set;}

        public int EngineCount=>Definition.EngineCount;
        public float PortThrottle=>engineTargets[0];
        public float StarboardThrottle=>engineTargets[1];
        public float PortEngineLoad=>SideAbsolute(engineActuals,0);
        public float StarboardEngineLoad=>SideAbsolute(engineActuals,1);
        public float PortCommandLoad=>SideAbsolute(engineTargets,0);
        public float StarboardCommandLoad=>SideAbsolute(engineTargets,1);
        public float PortSignedLoad=>Definition.HasFourEngines?(engineActuals[0]+engineActuals[2])*.5f:engineActuals[0];
        public float StarboardSignedLoad=>Definition.HasFourEngines?(engineActuals[1]+engineActuals[3])*.5f:engineActuals[1];
        private float SideAbsolute(float[] values,int side)=>Definition.HasFourEngines?(Mathf.Abs(values[side])+Mathf.Abs(values[side+2]))*.5f:Mathf.Abs(values[side]);
        public float CombinedThrottle{get{float sum=0;for(int i=0;i<EngineCount;i++)sum+=engineTargets[i];return sum/EngineCount;}}
        public bool AllEnginesSynchronized{get{for(int i=1;i<EngineCount;i++)if(!ThrottleDetents.Synchronized(engineTargets[0],engineTargets[i]))return false;return true;}}
        public float GetEngineThrottle(int index){ValidateEngine(index);return engineTargets[index];}
        public float GetEngineSignedLoad(int index){ValidateEngine(index);return engineActuals[index];}
        public void SetEngineThrottle(int index,float value){ValidateEngine(index);engineTargets[index]=ThrottleDetents.Snap(value);}
        private void ValidateEngine(int index){if(index<0||index>=EngineCount)throw new System.ArgumentOutOfRangeException(nameof(index));}
        public float Rudder => rudderTarget;
        // Actual fore/aft motion, independent of engine command and transverse drift.
        public float SignedSpeedKnots => Vector3.Dot(body.linearVelocity,ForwardOnWater)*VesselCatalog.ModelMetresPerUnit*1.9438445f;
        public float SpeedKnots
        {
            get
            {
                Vector3 velocity = body.linearVelocity;
                velocity.y = 0f;
                return velocity.magnitude * VesselCatalog.ModelMetresPerUnit * 1.9438445f;
            }
        }

        public float Heading => Mathf.Repeat(transform.eulerAngles.y, 360f);

        public float CourseOverGround
        {
            get
            {
                Vector3 velocity = body.linearVelocity;
                velocity.y = 0f;
                if (velocity.sqrMagnitude < 0.01f)
                {
                    return Heading;
                }

                return Mathf.Repeat(Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 360f);
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 40000f;
            body.useGravity = false;
            // Hydrodynamic damping is applied explicitly; Unity's isotropic angular drag would count it twice.
            body.linearDamping=0;body.angularDamping=0;
            body.constraints = RigidbodyConstraints.None;
            body.maxAngularVelocity=1f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void SetPortThrottle(float value){SetEngineThrottle(0,value);if(Definition.HasFourEngines)SetEngineThrottle(2,value);}
        public void SetStarboardThrottle(float value){SetEngineThrottle(1,value);if(Definition.HasFourEngines)SetEngineThrottle(3,value);}
        public void SetCombinedThrottle(float value){for(int i=0;i<EngineCount;i++)SetEngineThrottle(i,value);}
        public void SetRudder(float value) => rudderTarget = Mathf.Clamp(value, -1f, 1f);

        public void ConfigureVessel(VesselDefinition definition)
        {
            Definition = definition;
            maximumThrust = definition.GameThrust;
            engineLongitudinalOffset = definition.EngineOffset; engineResponsePerSecond = definition.EngineResponse;
            System.Array.Clear(engineTargets,0,4);System.Array.Clear(engineActuals,0,4);rudderTarget=rudderActual=TurnHeelDegrees=DesiredYawRate=0;
            body.mass = definition.GameMass;
            body.ResetCenterOfMass(); body.ResetInertiaTensor();
        }

        public void EmergencyStop()
        {
            System.Array.Clear(engineTargets,0,4);
        }

        public void ResetSimulation(Vector3 position, Quaternion rotation)
        {
            System.Array.Clear(engineTargets,0,4);System.Array.Clear(engineActuals,0,4);rudderTarget=rudderActual=TurnHeelDegrees=DesiredYawRate=0;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = position; body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            foreach (ParticleSystem effect in GetComponentsInChildren<ParticleSystem>()) effect.Clear(true);
            foreach (EngineExhaustSmoke smoke in GetComponentsInChildren<EngineExhaustSmoke>()) smoke.ResetForMode();
            GetComponent<WakeTrail>()?.ResetForMode();
            body.WakeUp();
        }

        public void StartUnderway(float aheadKnots)
        {
            if(!float.IsFinite(aheadKnots)||aheadKnots<0||aheadKnots>Definition.MaximumAheadKnots)
                throw new System.ArgumentOutOfRangeException(nameof(aheadKnots));
            float q=aheadKnots/Definition.MaximumAheadKnots;
            float load=Mathf.Sqrt(.15f*q+.85f*q*q);
            for(int i=0;i<EngineCount;i++){engineTargets[i]=ThrottleDetents.Snap(load);engineActuals[i]=load;}
            body.linearVelocity=ForwardOnWater*(aheadKnots/(VesselCatalog.ModelMetresPerUnit*1.9438445f));
            body.WakeUp();
        }

        private void FixedUpdate()=>SimulateForces(Time.fixedDeltaTime);
        // Also used by isolated PhysicsScene integration tests with the same production force path.
        public void SimulateForces(float deltaTime)
        {
            if(!float.IsFinite(deltaTime)||deltaTime<=0)return;
            Vector3 forward=ForwardOnWater,right=RightOnWater;
            rudderActual=Mathf.MoveTowards(rudderActual,rudderTarget,.20f*deltaTime);
            Vector3 stern = transform.position - forward * engineLongitudinalOffset;
            float podSteeringMoment=0;
            for(int i=0;i<EngineCount;i++)
            {
                engineActuals[i]=Mathf.MoveTowards(engineActuals[i],engineTargets[i],engineResponsePerSecond*deltaTime);
                float x=Definition.EngineLocalX(i),load=engineActuals[i];
                // RPM-like command -> thrust squared, reduced reverse thrust; never clamp or teleport velocity.
                float thrust=load*Mathf.Abs(load)*maximumThrust*(load<0?Definition.ReverseThrustFactor:1);
                Vector3 point=stern+right*x,direction=PropulsionDirection;
                // Planar manoeuvring and roll response are separate reduced models; do not add a second CG-height roll moment.
                point.y=body.worldCenterOfMass.y;
                body.AddForceAtPosition(direction*thrust,point,ForceMode.Force);
                if(Definition.UsesPods)podSteeringMoment+=Vector3.Cross(point-body.worldCenterOfMass,(direction-forward)*thrust).y;
            }
            float surge=Vector3.Dot(body.linearVelocity,forward),sway=Vector3.Dot(body.linearVelocity,right);
            float q=surge/Definition.MaximumWorldSpeed,yaw=body.angularVelocity.y;
            float drag=(.15f*q+.85f*q*Mathf.Abs(q))*maximumThrust*EngineCount;
            float turnDrag=1+Mathf.Min(.4f,Mathf.Abs(yaw)*Definition.ColliderSize.z*.08f);
            body.AddForce(-forward*(drag*turnDrag)-right*(sway*body.mass*(.65f+Mathf.Abs(surge)*.12f)),ForceMode.Force);
            // First-order Nomoto-style yaw response: a finite curvature with yaw inertia, not steering-wheel rotation.
            float diameter=Definition.ColliderSize.z*Definition.TurnDiameterLengths;
            DesiredYawRate=rudderActual*2*surge/diameter;
            float response=Mathf.Lerp(Definition.YawResponseSeconds*1.7f,Definition.YawResponseSeconds,Mathf.Clamp01(Mathf.Abs(q)));
            // At transit speed the reduced yaw model already includes steering: don't count pod steering moment twice.
            // Low-speed azimuth/differential manoeuvres retain direct thrust moments.
            if(Definition.UsesPods)body.AddTorque(Vector3.up*(-podSteeringMoment*Mathf.Clamp01(Mathf.Abs(q)*3)),ForceMode.Force);
            body.AddTorque(Vector3.up*((DesiredYawRate-yaw)/response),ForceMode.Acceleration);
            // A positive starboard turn heels outwards to port; reversed on a port turn.
            float lateralAcceleration=surge*yaw*VesselCatalog.ModelMetresPerUnit;
            TurnHeelDegrees=Mathf.Clamp(Mathf.Atan(lateralAcceleration/9.81f)*Mathf.Rad2Deg*.95f,-Definition.MaximumTurnHeel,Definition.MaximumTurnHeel);
        }
    }
}
