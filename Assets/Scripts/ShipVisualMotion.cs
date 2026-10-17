using UnityEngine;

namespace Meridian
{
    public sealed class ShipVisualMotion : MonoBehaviour
    {
        private Vector3 basePosition;
        private Quaternion baseRotation;

        private void Awake()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
        }

        private void Update()
        {
            float bob = Mathf.Sin(Time.time * 0.82f) * 0.045f;
            float roll = Mathf.Sin(Time.time * 0.63f + 1.2f) * 0.42f;
            float pitch = Mathf.Sin(Time.time * 0.48f) * 0.18f;
            transform.localPosition = basePosition + Vector3.up * bob;
            transform.localRotation = baseRotation * Quaternion.Euler(pitch, 0f, roll);
        }
    }
}
