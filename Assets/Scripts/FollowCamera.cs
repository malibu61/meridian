using UnityEngine;

namespace Meridian
{
    public sealed class FollowCamera : MonoBehaviour
    {
        private Transform target;
        private const float MinimumPitch = 8f;
        private const float MaximumPitch = 62f;
        private readonly OrbitZoomInput zoom = new();
        private float distanceScale = 1f, focusHeight = 2.45f;
        public float RequestedDistance => zoom.Distance * distanceScale;
        public float CurrentDistance => distance;
        public float FocusHeight => focusHeight;

        public void ConfigureVessel(VesselDefinition definition)
        {
            distanceScale = definition.CameraScale; focusHeight = definition.CameraFocusHeight;
            ResetView();
        }

        private float yaw = 24f;
        private float pitch = 23f;
        private float distance = 34f;
        private float targetYaw = 24f;
        private float targetPitch = 23f;
        private bool mouseDragging;
        private Vector2 previousMousePosition;
        private int orbitFingerId = -1;
        private bool inputBlocked;

        public void SetInputBlocked(bool blocked)
        {
            if(inputBlocked==blocked)return;
            inputBlocked = blocked;
            zoom.EndPinch(); mouseDragging = false; orbitFingerId = -1;
        }

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
            distance = Mathf.Lerp(distance, RequestedDistance, smoothing);

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * focusHeight;
            Vector3 desiredPosition = focusPoint + Quaternion.Euler(0,target.eulerAngles.y,0) * (orbit * new Vector3(0f, 0f, -distance));
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
            Vector3 focusPoint = target.position + Vector3.up * focusHeight;
            transform.position = focusPoint + Quaternion.Euler(0,target.eulerAngles.y,0) * (orbit * new Vector3(0f, 0f, -distance));
            transform.LookAt(focusPoint);
        }

        private void ReadOrbitInput()
        {
            if (inputBlocked) return;
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
            zoom.Reset(); yaw = targetYaw = 24f; pitch = targetPitch = 23f;
            distance = RequestedDistance;
            mouseDragging = false; orbitFingerId = -1;
            SnapToTarget();
        }

        public void SetTransitView()
        {
            yaw = targetYaw = 0f; pitch = targetPitch = 8f;
            SnapToTarget();
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
            HelmLayout layout=HelmLayout.Current;
            return layout.BlocksCamera(layout.FromScreen(screenPosition))||TugAssistController.BlocksCamera(screenPosition);
        }
    }
}
