using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meridian
{
    /// <summary>Playable geographic blockout; real OSM/DEM import is a separate production step.</summary>
    public sealed class StraitEnvironment : MonoBehaviour
    {
        private EnvironmentResources resources;
        private Material grass, sand, stone, white, roof, red, gateMaterial;
        private LineRenderer gate;
        private GameSessionController session;
        private int displayedGate = -1;

        public void Build(Transform ship, Material water, Material beach, Material vegetation, Material concrete, Material pale, Material tile)
        {
            resources = GetComponent<EnvironmentResources>();
            grass = vegetation; sand = beach; stone = concrete; white = pale; roof = tile;
            red = resources.Own(new Material(concrete) { color = new Color(.72f,.12f,.09f) });
            gateMaterial = resources.Own(new Material(Shader.Find("Sprites/Default")) { color = new Color(.25f,.91f,.86f,.8f) });
            session = Object.FindAnyObjectByType<GameSessionController>();
            BuildWater(ship, water);
            for (int segment = 0; segment < StraitRoute.Count - 1; segment++)
            {
                BuildShore(segment, -1f);
                BuildShore(segment, 1f);
            }
            BuildBridge();
            BuildTown(1, -1f, "Northern settlement study");
            BuildTown(7, 1f, "Eceabat silhouette study");
            BuildTown(8, -1f, "Canakkale silhouette study");
            BuildFort(8, 1f);
            BuildMonument();
            GameObject marker = new("Next game checkpoint — not a navigation aid"); marker.transform.SetParent(transform, false);
            gate = marker.AddComponent<LineRenderer>();
            gate.sharedMaterial = gateMaterial; gate.positionCount = 65;
            gate.startWidth = gate.endWidth = 1.8f; gate.useWorldSpace = true;
            gate.shadowCastingMode = ShadowCastingMode.Off; gate.receiveShadows = false;
        }

        private void Update()
        {
            if (session == null || session.Voyage == null || gate == null) return;
            int next = session.Voyage.NextIndex;
            gate.enabled = !session.Voyage.Complete;
            if (next == displayedGate || !gate.enabled) return;
            displayedGate = next;
            Vector3 center = session.Voyage.NextPosition; center.y = -.25f;
            for (int i = 0; i <= 64; i++)
            {
                float angle = i / 64f * Mathf.PI * 2f;
                gate.SetPosition(i, center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * StraitRoute.GateRadius);
            }
        }

        private void BuildWater(Transform ship, Material water)
        {
            GameObject distant = GameObject.CreatePrimitive(PrimitiveType.Plane);
            distant.name = "Distant strait water"; distant.transform.SetParent(transform, false);
            distant.transform.position = new Vector3(-5600f,WaterWaves.SeaLevel,-5600f);
            distant.transform.localScale = new Vector3(3000f,1f,3000f);
            Destroy(distant.GetComponent<Collider>());
            Material distantMaterial = resources.Own(new Material(water));
            distantMaterial.SetFloat("_FarWater",1);
            distant.GetComponent<Renderer>().sharedMaterial = distantMaterial;
            GameObject near = new("Ship-following wave patch"); near.transform.SetParent(transform, false);
            near.transform.position = new Vector3(ship.position.x,-.72f,ship.position.z);
            near.AddComponent<MeshFilter>(); near.AddComponent<MeshRenderer>();
            OceanSurface ocean = near.AddComponent<OceanSurface>();
            ocean.Configure(water, 320f, 320f, 96); ocean.Follow(ship);
        }

        private void BuildShore(int segment, float side)
        {
            const int rows = 17, columns = 8;
            Vector3[] vertices = new Vector3[rows * columns];
            Vector2[] uv = new Vector2[vertices.Length];
            List<int>[] indices = { new(),new(),new() };
            float[] offsets = { -3f,0f,9f,35f,120f,300f,600f,1000f };
            for (int row = 0; row < rows; row++)
            {
                float t = row / (float)(rows - 1);
                Vector3 center = Vector3.Lerp(StraitRoute.Point(segment), StraitRoute.Point(segment + 1), t);
                Vector3 direction = Vector3.Lerp(StraitRoute.Direction(segment), StraitRoute.Direction(segment + 1), t).normalized;
                Vector3 outward = new Vector3(direction.z,0f,-direction.x) * side;
                float width = Mathf.Lerp(StraitRoute.HalfWidths[segment], StraitRoute.HalfWidths[segment + 1], t);
                for (int column = 0; column < columns; column++)
                {
                    Vector3 point = center + outward * (width + offsets[column]);
                    float noise = Mathf.PerlinNoise(point.x * .0011f + 50f,point.z * .0011f + 50f);
                    point.y = column == 0 ? -2f : column == 1 ? -.85f :
                        Mathf.Lerp(1f,80f * (.3f + noise),Mathf.Clamp01(offsets[column] / 600f));
                    vertices[row * columns + column] = point;
                    uv[row * columns + column] = new Vector2(point.x * .025f,point.z * .025f);
                }
            }
            for (int row = 0; row < rows - 1; row++) for (int column = 0; column < columns - 1; column++)
            {
                int a = row * columns + column, b = a + columns, c = a + 1, d = b + 1;
                List<int> list = indices[column < 2 ? 0 : column < 3 ? 1 : 2];
                if (Vector3.Cross(vertices[b] - vertices[a],vertices[c] - vertices[a]).y >= 0f)
                    list.AddRange(new[] { a,b,c,c,b,d });
                else list.AddRange(new[] { a,c,b,c,d,b });
            }
            Mesh mesh = resources.Own(new Mesh { name = "Illustrative shore terrain" });
            mesh.vertices = vertices; mesh.uv = uv; mesh.subMeshCount = 3;
            for (int i = 0; i < 3; i++) mesh.SetTriangles(indices[i],i);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            GameObject coast = new($"Coast {segment:00} {(side > 0f ? "Europe" : "Anatolia")}");
            coast.transform.SetParent(transform,false);
            coast.AddComponent<MeshFilter>().sharedMesh = mesh;
            coast.AddComponent<MeshRenderer>().sharedMaterials = new[] { sand,stone,grass };
            coast.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private Transform Frame(string title, int index)
        {
            Transform frame = new GameObject(title).transform; frame.SetParent(transform,false);
            frame.position = StraitRoute.Point(index);
            frame.rotation = Quaternion.LookRotation(StraitRoute.Direction(index),Vector3.up);
            return frame;
        }

        private void BuildBridge()
        {
            Transform frame = Frame("1915 bridge — simplified landmark",2);
            const float deckHeight = 18f, halfSpan = 252.875f;
            Box("Bridge deck",frame,new Vector3(0f,deckHeight,0f),new Vector3(1152f,1f,11.25f),stone,true);
            foreach (float x in new[] { -halfSpan,halfSpan })
            {
                for (int side = -1; side <= 1; side += 2)
                    Box("Red tower",frame,new Vector3(x,39f,side * 6.3f),new Vector3(3.2f,80f,3.2f),red,true);
                Box("Tower crown",frame,new Vector3(x,79f,0f),new Vector3(4.3f,2f,16f),white,false);
                Box("Tower crossbeam",frame,new Vector3(x,32f,0f),new Vector3(3f,2f,14f),red,false);
            }
            for (int side = -1; side <= 1; side += 2) for (int span = 0; span < 3; span++)
            {
                float from = span == 0 ? -576f : span == 1 ? -halfSpan : halfSpan;
                float to = span == 0 ? -halfSpan : span == 1 ? halfSpan : 576f;
                Vector3 previous = default;
                for (int i = 0; i <= 24; i++)
                {
                    float t = i / 24f, x = Mathf.Lerp(from,to,t);
                    float height = span == 1 ? 23f + 53f * Mathf.Pow(2f * t - 1f,2f) : Mathf.Lerp(span == 0 ? 20f : 76f,span == 0 ? 76f : 20f,t);
                    Vector3 point = new(x,height,side * 5.8f);
                    if (i > 0) Beam("Suspension cable",frame,previous,point,.38f,stone);
                    if (i % 2 == 0) Beam("Deck hanger",frame,new Vector3(x,deckHeight,side * 5.8f),point,.13f,white);
                    previous = point;
                }
            }
        }

        private void BuildTown(int index, float side, string name)
        {
            Transform frame = Frame(name,index);
            for (int row = 0; row < 3; row++) for (int i = 0; i < 14; i++)
            {
                float height = 2f + Mathf.Abs(Mathf.Sin(i * 2.7f + row)) * 5f;
                Vector3 position = new(side * (StraitRoute.HalfWidths[index] + 32f + row * 19f),height * .5f + 2f,(i - 6.5f) * 15f);
                Box("Coastal building",frame,position,new Vector3(8f,height,10f),white,false);
                Box("Tile roof",frame,position + Vector3.up * (height * .5f),new Vector3(8.4f,.6f,10.4f),roof,false);
            }
        }

        private void BuildFort(int index, float side)
        {
            Transform frame = Frame("Kilitbahir fort silhouette study",index);
            float x = side * (StraitRoute.HalfWidths[index] + 12f);
            Box("Fort wall",frame,new Vector3(x,3f,0f),new Vector3(12f,6f,23f),sand,true);
            for (int i = -1; i <= 1; i++) Box("Fort tower",frame,new Vector3(x,5f,i * 10f),new Vector3(6f,10f,6f),stone,true);
        }

        private void BuildMonument()
        {
            Transform frame = Frame("Southern memorial silhouette study",11);
            float x = StraitRoute.HalfWidths[11] + 100f;
            for (int a = -1; a <= 1; a += 2) for (int b = -1; b <= 1; b += 2)
                Box("Memorial column",frame,new Vector3(x + a * 3.5f,8f,b * 3.5f),new Vector3(2f,12f,2f),white,false);
            Box("Memorial top",frame,new Vector3(x,14.5f,0f),new Vector3(10f,1.7f,10f),white,false);
        }

        private GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube); part.name = name;
            part.transform.SetParent(parent,false); part.transform.localPosition = position; part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) { part.GetComponent<Collider>().enabled = false; Destroy(part.GetComponent<Collider>()); }
            return part;
        }

        private void Beam(string name, Transform parent, Vector3 a, Vector3 b, float width, Material material)
        {
            GameObject beam = Box(name,parent,(a + b) * .5f,new Vector3(width,width,(b - a).magnitude),material,false);
            beam.transform.localRotation = Quaternion.LookRotation(b - a,Vector3.up);
        }
    }
}
