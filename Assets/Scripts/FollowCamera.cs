using UnityEngine;

namespace Meridian
{
    public sealed class FollowCamera : MonoBehaviour
    {
        private Transform target;
        private const float MinimumPitch = 12f;
        private const float MaximumPitch = 62f;
        private readonly OrbitZoomInput zoom = new();
        public float RequestedDistance => zoom.Distance;
        public float CurrentDistance => distance;

        private float yaw = 24f;
        private float pitch = 23f;
        private float distance = 34f;
        private float targetYaw = 24f;
        private float targetPitch = 23f;
        private bool mouseDragging;
        private Vector2 previousMousePosition;
        private int orbitFingerId = -1;

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

            float smoothing = 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime);
            yaw = Mathf.LerpAngle(yaw, targetYaw, smoothing);
            pitch = Mathf.Lerp(pitch, targetPitch, smoothing);
            distance = Mathf.Lerp(distance, zoom.Distance, smoothing);

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * 2.45f;
            Vector3 desiredPosition = focusPoint + target.rotation * (orbit * new Vector3(0f, 0f, -distance));
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-5f * Time.unscaledDeltaTime));
            Quaternion desiredRotation = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-9f * Time.unscaledDeltaTime));
        }

        private void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * 2.45f;
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
            zoom.EndPinch();
            orbitFingerId = -1;

            Vector2 mousePosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                mouseDragging = IsCameraArea(mousePosition);
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
            if (Mathf.Abs(wheel) > 0.01f && IsCameraArea(mousePosition)) zoom.ApplyWheel(wheel);
            float keyboardZoom = (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus) ? 1f : 0f)
                - (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) ? 1f : 0f);
            zoom.ApplyWheel(keyboardZoom * 7f * Time.unscaledDeltaTime);
            if (Input.GetKeyDown(KeyCode.R)) ResetView();
        }

        public void ResetView()
        {
            zoom.Reset(); targetYaw = 24f; targetPitch = 23f;
            mouseDragging = false; orbitFingerId = -1;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) { zoom.EndPinch(); mouseDragging = false; orbitFingerId = -1; }
        }

        private void OnDisable() { zoom.EndPinch(); mouseDragging = false; orbitFingerId = -1; }

        private void ReadTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                zoom.EndPinch();
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                { mouseDragging = false; orbitFingerId = -1; }
                else if (touch.phase == TouchPhase.Began || orbitFingerId != touch.fingerId)
                {
                    orbitFingerId = touch.fingerId;
                    mouseDragging = IsCameraArea(touch.position);
                    // Rebase after a pinch before the remaining finger can orbit.
                }
                else if (mouseDragging && touch.phase == TouchPhase.Moved)
                    ApplyOrbitDelta(touch.deltaPosition * 0.82f);
            }
            else if (Input.touchCount >= 2)
            {
                mouseDragging = false; orbitFingerId = -1;
                if (Input.touchCount != 2) { zoom.EndPinch(); return; }
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                if (first.phase == TouchPhase.Ended || first.phase == TouchPhase.Canceled || second.phase == TouchPhase.Ended || second.phase == TouchPhase.Canceled)
                { zoom.EndPinch(); return; }
                if (first.phase == TouchPhase.Began || second.phase == TouchPhase.Began) zoom.EndPinch();
                zoom.UpdatePinch(first.fingerId,first.position,second.fingerId,second.position,
                    IsCameraArea(first.position) && IsCameraArea(second.position));
            }
        }

        private static bool IsCameraArea(Vector2 point) => point.x >= 0f && point.y >= 0f
            && point.x <= Screen.width && point.y <= Screen.height && !IsHudRegion(point);

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
