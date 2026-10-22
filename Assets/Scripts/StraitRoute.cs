using UnityEngine;

namespace Meridian
{
    /// <summary>Hand-authored geographic study. NOT surveyed coastlines, bathymetry or a sailing plan.</summary>
    public static class StraitRoute
    {
        public const float MetersPerUnit = 4f;
        public const float GateRadius = 100f;
        public static readonly string[] Names = {
            "Marmara yaklaşımı", "Gelibolu · Çardak", "1915 Köprüsü", "Lapseki güneyi",
            "Nara yaklaşımı", "Nara kuzeyi", "Nara dönüşü", "Eceabat",
            "Çanakkale · Kilitbahir", "Kepez", "Erenköy", "Güney yaklaşımı", "Ege çıkışı"
        };
        // Approximate centreline anchors. Widths and terrain are deliberately illustrative.
        private static readonly Vector2[] LonLat = {
            new(26.715f,40.425f), new(26.680f,40.390f), new(26.634f,40.348f),
            new(26.565f,40.305f), new(26.490f,40.250f), new(26.443f,40.218f),
            new(26.405f,40.208f), new(26.380f,40.188f), new(26.397f,40.158f),
            new(26.370f,40.130f), new(26.320f,40.095f), new(26.250f,40.065f), new(26.185f,40.040f)
        };
        public static readonly float[] HalfWidths = { 630f,550f,500f,620f,620f,430f,260f,200f,165f,350f,550f,600f,800f };
        public static int Count => LonLat.Length;

        public static Vector3 Point(int index)
        {
            Vector2 geo = LonLat[index];
            return new Vector3((geo.x - 26.715f) * 85000f / MetersPerUnit, .1f,
                (geo.y - 40.425f) * 111000f / MetersPerUnit);
        }

        public static Vector3[] Positions()
        {
            Vector3[] positions = new Vector3[Count];
            for (int i = 0; i < Count; i++) positions[i] = Point(i);
            return positions;
        }

        public static Vector3 Direction(int index)
        {
            Vector3 direction = Point(Mathf.Min(index + 1, Count - 1)) - Point(Mathf.Max(0, index - 1));
            direction.y = 0f;
            return direction.normalized;
        }
    }
}
