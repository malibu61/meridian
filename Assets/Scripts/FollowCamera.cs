using UnityEngine;

namespace Meridian
{
    public sealed class FollowCamera : MonoBehaviour
    {
        private Transform target;
        private const float MinimumPitch = 12f;
        private const float MaximumPitch = 62f;
        private const float MinimumDistance = 17f;
        private const float MaximumDistance = 48f;

        private float yaw = 24f;
        private float pitch = 23f;
        private float distance = 29f;
        private float targetYaw = 24f;
        private float targetPitch = 23f;
        private float targetDistance = 29f;
        private bool mouseDragging;
        private Vector2 previousMousePosition;

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

            ReadOrbitInput();

            float smoothing = 1f - Mathf.Exp(-8f * Time.deltaTime);
            yaw = Mathf.LerpAngle(yaw, targetYaw, smoothing);
            pitch = Mathf.Lerp(pitch, targetPitch, smoothing);
            distance = Mathf.Lerp(distance, targetDistance, smoothing);

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * 1.6f;
            Vector3 desiredPosition = focusPoint + target.rotation * (orbit * new Vector3(0f, 0f, -distance));
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-5f * Time.deltaTime));
            Quaternion desiredRotation = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-9f * Time.deltaTime));
        }

        private void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * 1.6f;
            transform.position = focusPoint + target.rotation * (orbit * new Vector3(0f, 0f, -distance));
            transform.LookAt(focusPoint);
        }

        private void ReadOrbitInput()
        {
            if (Input.touchCount > 0)
            {
                ReadTouchInput();
                return;
            }

            Vector2 mousePosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                mouseDragging = !IsHudRegion(mousePosition);
                previousMousePosition = mousePosition;
            }

            if (Input.GetMouseButtonUp(0)) mouseDragging = false;
            if (mouseDragging && Input.GetMouseButton(0))
            {
                Vector2 delta = mousePosition - previousMousePosition;
                ApplyOrbitDelta(delta);
                previousMousePosition = mousePosition;
            }

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
                targetDistance = Mathf.Clamp(targetDistance - wheel * 2.4f, MinimumDistance, MaximumDistance);
        }

        private void ReadTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    mouseDragging = !IsHudRegion(touch.position);
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    mouseDragging = false;
                else if (mouseDragging && touch.phase == TouchPhase.Moved)
                    ApplyOrbitDelta(touch.deltaPosition * 0.82f);
            }
            else if (Input.touchCount >= 2)
            {
                mouseDragging = false;
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                Vector2 firstPrevious = first.position - first.deltaPosition;
                Vector2 secondPrevious = second.position - second.deltaPosition;
                float previousDistance = Vector2.Distance(firstPrevious, secondPrevious);
                float currentDistance = Vector2.Distance(first.position, second.position);
                targetDistance = Mathf.Clamp(targetDistance - (currentDistance - previousDistance) * 0.035f,
                    MinimumDistance, MaximumDistance);
            }
        }

        private void ApplyOrbitDelta(Vector2 delta)
        {
            targetYaw += delta.x * 0.2f;
            targetPitch = Mathf.Clamp(targetPitch - delta.y * 0.16f, MinimumPitch, MaximumPitch);
        }

        private static bool IsHudRegion(Vector2 screenPosition)
        {
            float normalizedX = screenPosition.x / Mathf.Max(1f, Screen.width);
            float normalizedY = screenPosition.y / Mathf.Max(1f, Screen.height);
            bool header = normalizedY > 0.82f;
            bool lowerControls = normalizedY < 0.29f;
            bool engineControls = normalizedY < 0.56f && (normalizedX < 0.17f || normalizedX > 0.83f);
            return header || lowerControls || engineControls;
        }
    }
}
