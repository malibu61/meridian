using UnityEngine;

namespace Meridian
{
    /// <summary>Input-independent zoom state shared by the mouse and two-finger gestures.</summary>
    public sealed class OrbitZoomInput
    {
        public const float MinimumDistance = 6f, MaximumDistance = 60f, DefaultDistance = 34f;
        public float Distance { get; private set; } = DefaultDistance;
        public bool IsPinching { get; private set; }
        private bool hasPair;
        private int firstId, secondId;
        private float previousGap;

        public void ApplyWheel(float steps)
        {
            if (!Finite(steps)) return;
            SetDistance(Distance * Mathf.Exp(Mathf.Clamp(-steps * .12f, -20f, 20f)));
        }

        public void UpdatePinch(int fingerA, Vector2 positionA, int fingerB, Vector2 positionB, bool mayStart)
        {
            float gap = Vector2.Distance(positionA, positionB);
            if (fingerA == fingerB || !Finite(gap)) { EndPinch(); return; }
            bool samePair = hasPair && ((firstId == fingerA && secondId == fingerB) || (firstId == fingerB && secondId == fingerA));
            if (!samePair)
            {
                firstId = fingerA; secondId = fingerB; hasPair = true;
                IsPinching = mayStart; previousGap = gap;
                return; // Placing/replacing a finger establishes a baseline, never a zoom jump.
            }
            if (!IsPinching) return; // Gestures started over controls remain blocked until release.
            if (gap >= 8f && previousGap >= 8f) SetDistance(Distance * previousGap / gap);
            previousGap = gap;
        }

        public void EndPinch() { hasPair = false; IsPinching = false; previousGap = 0f; }
        public void Reset() { Distance = DefaultDistance; EndPinch(); }
        private void SetDistance(float value) { if (Finite(value)) Distance = Mathf.Clamp(value, MinimumDistance, MaximumDistance); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
