using UnityEngine;

namespace Meridian
{
    public sealed class FollowCamera : MonoBehaviour
    {
        private Transform target;
        private readonly Vector3 localOffset = new(0f, 16f, -25f);

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.TransformPoint(localOffset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 3f * Time.deltaTime);
            Quaternion desiredRotation = Quaternion.LookRotation(target.position + target.forward * 5f - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 4f * Time.deltaTime);
        }

        private void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.TransformPoint(localOffset);
            transform.LookAt(target.position + target.forward * 5f);
        }
    }
}
