using UnityEngine;

namespace Meridian
{
    [RequireComponent(typeof(Light))]
    public sealed class BeaconPulse : MonoBehaviour
    {
        private Light beacon;

        private void Awake()
        {
            beacon = GetComponent<Light>();
        }

        private void Update()
        {
            float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Time.time * 2.2f)), 10f);
            beacon.intensity = pulse * 5f;
        }
    }
}
