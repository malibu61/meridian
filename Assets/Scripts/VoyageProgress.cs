using System;
using UnityEngine;

namespace Meridian
{
    public enum SimulationMode { Harbor, StraitPassage, BuoyMooring }

    /// <summary>Ordered game checkpoints, not a real-world navigation route.</summary>
    public sealed class VoyageProgress
    {
        private readonly Vector3[] points;
        public int NextIndex { get; private set; } = 1;
        public int GateCount => points.Length - 1;
        public int PassedCount => NextIndex - 1;
        public bool Complete => NextIndex >= points.Length;
        public float Fraction => PassedCount / (float)GateCount;
        public Vector3 NextPosition => points[Mathf.Min(NextIndex, points.Length - 1)];

        public VoyageProgress(Vector3[] route)
        {
            if (route == null || route.Length < 2) throw new ArgumentException("At least two route points are required.");
            points = (Vector3[])route.Clone();
        }

        public bool Advance(Vector3 previous, Vector3 current, float radius)
        {
            if (Complete || radius <= 0f || !Finite(previous) || !Finite(current)) return false;
            previous.y = current.y = 0f;
            Vector3 target = NextPosition; target.y = 0f;
            Vector3 travel = current - previous;
            // Do not count a checkpoint crossed while travelling backwards along the route.
            Vector3 forward = target - points[NextIndex - 1]; forward.y = 0f;
            if (Vector3.Dot(travel, forward) <= 0f) return false;
            float t = travel.sqrMagnitude < .000001f ? 0f : Mathf.Clamp01(Vector3.Dot(target - previous, travel) / travel.sqrMagnitude);
            if ((previous + travel * t - target).sqrMagnitude > radius * radius) return false;
            NextIndex++;
            return true;
        }

        private static bool Finite(Vector3 v) => !float.IsNaN(v.x) && !float.IsInfinity(v.x)
            && !float.IsNaN(v.y) && !float.IsInfinity(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.z);
    }
}
