using UnityEngine;

namespace Meridian
{
    public sealed class RotatingRadar : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 42f;

        private void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
