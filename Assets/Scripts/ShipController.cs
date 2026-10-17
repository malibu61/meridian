using UnityEngine;

namespace Meridian
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipController : MonoBehaviour
    {
        [Header("Engines")]
        [SerializeField] private float maximumThrust = 6000f;
        [SerializeField] private float engineSpacing = 3.2f;
        [SerializeField] private float engineResponsePerSecond = 0.45f;

        [Header("Water resistance")]
        [SerializeField] private float longitudinalResistance = 1100f;
        [SerializeField] private float lateralResistance = 5200f;
        [SerializeField] private float angularResistance = 5200f;

        [Header("Rudder")]
        [SerializeField] private float rudderTorque = 950f;

        private Rigidbody body;
        private float portTarget;
        private float starboardTarget;
        private float rudderTarget;
        private float portActual;
        private float starboardActual;

        public float PortThrottle => portTarget;
        public float StarboardThrottle => starboardTarget;
        public float Rudder => rudderTarget;
        public float SpeedKnots
        {
            get
            {
                Vector3 velocity = body.linearVelocity;
                velocity.y = 0f;
                return velocity.magnitude * 1.94384f;
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
            body.constraints = RigidbodyConstraints.FreezePositionY |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void SetPortThrottle(float value) => portTarget = Mathf.Clamp(value, -1f, 1f);
        public void SetStarboardThrottle(float value) => starboardTarget = Mathf.Clamp(value, -1f, 1f);
        public void SetRudder(float value) => rudderTarget = Mathf.Clamp(value, -1f, 1f);

        public void EmergencyStop()
        {
            portTarget = 0f;
            starboardTarget = 0f;
        }

        private void FixedUpdate()
        {
            portActual = Mathf.MoveTowards(portActual, portTarget, engineResponsePerSecond * Time.fixedDeltaTime);
            starboardActual = Mathf.MoveTowards(starboardActual, starboardTarget, engineResponsePerSecond * Time.fixedDeltaTime);

            Vector3 stern = transform.position - transform.forward * 5.2f;
            Vector3 portEngine = stern - transform.right * (engineSpacing * 0.5f);
            Vector3 starboardEngine = stern + transform.right * (engineSpacing * 0.5f);

            body.AddForceAtPosition(transform.forward * (portActual * maximumThrust), portEngine, ForceMode.Force);
            body.AddForceAtPosition(transform.forward * (starboardActual * maximumThrust), starboardEngine, ForceMode.Force);

            Vector3 localVelocity = transform.InverseTransformDirection(body.linearVelocity);
            Vector3 resistance = transform.forward * (-localVelocity.z * longitudinalResistance) +
                                 transform.right * (-localVelocity.x * lateralResistance);
            body.AddForce(resistance, ForceMode.Force);

            float waterFlow = Mathf.Clamp(localVelocity.z, -6f, 6f);
            body.AddTorque(Vector3.up * (rudderTarget * waterFlow * Mathf.Abs(waterFlow) * rudderTorque), ForceMode.Force);
            body.AddTorque(Vector3.up * (-body.angularVelocity.y * angularResistance), ForceMode.Force);
        }
    }
}
