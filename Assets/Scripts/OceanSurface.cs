using UnityEngine;

namespace Meridian
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class OceanSurface : MonoBehaviour
    {
        [SerializeField] private Vector2 scrollSpeed = new(0.012f, 0.018f);
        [SerializeField] private float waveHeight = 0.22f;
        [SerializeField] private float waveSpeed = 0.72f;
        private Material oceanMaterial;
        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] animatedVertices;

        public void Configure(Material material, float width, float length, int resolution)
        {
            oceanMaterial = new Material(material);
            GetComponent<MeshRenderer>().sharedMaterial = oceanMaterial;
            BuildGrid(width, length, Mathf.Clamp(resolution, 12, 64));
        }

        private void Awake()
        {
            if (oceanMaterial == null && GetComponent<Renderer>().sharedMaterial != null)
                oceanMaterial = GetComponent<Renderer>().material;
        }

        private void Update()
        {
            if (oceanMaterial != null) oceanMaterial.mainTextureOffset = scrollSpeed * Time.time;
            if (mesh == null || baseVertices == null) return;

            float time = Time.time * waveSpeed;
            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 point = baseVertices[i];
                float broad = Mathf.Sin(point.x * 0.105f + point.z * 0.073f + time) * waveHeight;
                float cross = Mathf.Sin(point.x * -0.16f + point.z * 0.12f + time * 1.43f) * waveHeight * 0.48f;
                float ripple = Mathf.Sin(point.x * 0.34f + point.z * -0.25f + time * 2.1f) * waveHeight * 0.16f;
                animatedVertices[i] = point + Vector3.up * (broad + cross + ripple);
            }
            mesh.vertices = animatedVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void BuildGrid(float width, float length, int resolution)
        {
            int row = resolution + 1;
            baseVertices = new Vector3[row * row];
            animatedVertices = new Vector3[baseVertices.Length];
            Vector2[] uvs = new Vector2[baseVertices.Length];
            int[] triangles = new int[resolution * resolution * 6];
            for (int z = 0; z <= resolution; z++)
            for (int x = 0; x <= resolution; x++)
            {
                int index = z * row + x;
                baseVertices[index] = new Vector3((x / (float)resolution - 0.5f) * width, 0f,
                    (z / (float)resolution - 0.5f) * length);
                animatedVertices[index] = baseVertices[index];
                uvs[index] = new Vector2(x / (float)resolution * 12f, z / (float)resolution * 16f);
            }
            int t = 0;
            for (int z = 0; z < resolution; z++)
            for (int x = 0; x < resolution; x++)
            {
                int i = z * row + x;
                triangles[t++] = i; triangles[t++] = i + row; triangles[t++] = i + 1;
                triangles[t++] = i + 1; triangles[t++] = i + row; triangles[t++] = i + row + 1;
            }
            mesh = new Mesh { name = "Animated Aegean water" };
            mesh.MarkDynamic();
            mesh.vertices = baseVertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
}
