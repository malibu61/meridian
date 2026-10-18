using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meridian
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        private Material seaMaterial;
        private Material foamMaterial;
        private Material hullMaterial;
        private Material hullDarkMaterial;
        private Material deckMaterial;
        private Material superstructureMaterial;
        private Material windowMaterial;
        private Material concreteMaterial;
        private Material sandMaterial;
        private Material grassMaterial;
        private Material roofMaterial;
        private Material buoyRedMaterial;
        private Material buoyGreenMaterial;
        private Material whiteMaterial;
        private Material orangeMaterial;
        private Material darkMaterial;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            BuildMaterials();
            BuildLighting();
            BuildEnvironment();
            ShipController ship = BuildShip();
            BuildCamera(ship.transform);
            BuildHud(ship);
        }

        private void BuildMaterials()
        {
            seaMaterial = CreateMaterial(new Color(0.025f, 0.28f, 0.39f), 0.12f, 0.82f);
            seaMaterial.mainTexture = CreateWaveTexture();
            seaMaterial.mainTextureScale = Vector2.one;

            Shader particleShader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            foamMaterial = new Material(particleShader) { color = new Color(0.78f, 0.95f, 1f, 0.62f) };

            hullMaterial = CreateMaterial(new Color(0.31f, 0.38f, 0.39f), 0.18f, 0.42f);
            hullDarkMaterial = CreateMaterial(new Color(0.15f, 0.20f, 0.21f), 0.14f, 0.3f);
            deckMaterial = CreateMaterial(new Color(0.19f, 0.25f, 0.25f), 0.05f, 0.25f);
            superstructureMaterial = CreateMaterial(new Color(0.62f, 0.67f, 0.66f), 0.12f, 0.38f);
            windowMaterial = CreateMaterial(new Color(0.025f, 0.10f, 0.14f), 0.35f, 0.88f);
            concreteMaterial = CreateMaterial(new Color(0.34f, 0.36f, 0.34f), 0.02f, 0.2f);
            sandMaterial = CreateMaterial(new Color(0.58f, 0.54f, 0.42f), 0f, 0.16f);
            grassMaterial = CreateMaterial(new Color(0.20f, 0.34f, 0.23f), 0f, 0.2f);
            roofMaterial = CreateMaterial(new Color(0.32f, 0.18f, 0.14f), 0.05f, 0.22f);
            buoyRedMaterial = CreateMaterial(new Color(0.82f, 0.12f, 0.09f), 0.12f, 0.45f);
            buoyGreenMaterial = CreateMaterial(new Color(0.08f, 0.58f, 0.34f), 0.12f, 0.45f);
            whiteMaterial = CreateMaterial(new Color(0.9f, 0.93f, 0.91f), 0.05f, 0.45f);
            orangeMaterial = CreateMaterial(new Color(0.92f, 0.35f, 0.08f), 0.04f, 0.35f);
            darkMaterial = CreateMaterial(new Color(0.08f, 0.10f, 0.10f), 0.15f, 0.3f);
        }

        private void BuildLighting()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.56f, 0.70f, 0.73f);
            RenderSettings.fogStartDistance = 85f;
            RenderSettings.fogEndDistance = 275f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.54f, 0.68f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.31f, 0.42f, 0.43f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.20f, 0.19f);

            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                Material sky = new(skyShader);
                sky.SetColor("_SkyTint", new Color(0.32f, 0.55f, 0.63f));
                sky.SetColor("_GroundColor", new Color(0.24f, 0.30f, 0.29f));
                sky.SetFloat("_AtmosphereThickness", 0.72f);
                sky.SetFloat("_SunSize", 0.035f);
                RenderSettings.skybox = sky;
            }

            GameObject sun = new("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.91f, 0.74f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.72f;
            light.shadowBias = 0.06f;
            sun.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            QualitySettings.shadowDistance = 120f;
        }

        private void BuildEnvironment()
        {
            GameObject sea = new("Animated Sea");
            sea.transform.position = new Vector3(0f, -0.72f, 52f);
            sea.AddComponent<MeshFilter>();
            sea.AddComponent<MeshRenderer>();
            sea.AddComponent<OceanSurface>().Configure(seaMaterial, 300f, 360f, 44);

            BuildCoastline();
            BuildHarborBuildings();
            BuildNavigationMarkers();
            BuildDistantTown();
        }

        private void BuildCoastline()
        {
            CreateCube("West coast", new Vector3(-65f, 0.2f, 70f), new Vector3(42f, 2.4f, 190f), sandMaterial, true);
            CreateCube("West vegetation", new Vector3(-72f, 1.35f, 70f), new Vector3(29f, 0.7f, 185f), grassMaterial, true);
            CreateCube("East coast", new Vector3(68f, 0.15f, 86f), new Vector3(44f, 2.3f, 170f), sandMaterial, true);
            CreateCube("East vegetation", new Vector3(76f, 1.3f, 86f), new Vector3(29f, 0.65f, 166f), grassMaterial, true);

            CreateCube("West breakwater", new Vector3(-38f, 0.55f, 54f), new Vector3(14f, 2.7f, 96f), concreteMaterial, true);
            CreateCube("West breakwater cap", new Vector3(-38f, 2f, 54f), new Vector3(15f, 0.25f, 97f), whiteMaterial, false);
            CreateCube("East pier", new Vector3(42f, 0.5f, 70f), new Vector3(18f, 2.5f, 82f), concreteMaterial, true);
            CreateCube("East pier cap", new Vector3(42f, 1.85f, 70f), new Vector3(19f, 0.24f, 83f), whiteMaterial, false);
            CreateCube("Harbor wall", new Vector3(0f, 0.7f, 128f), new Vector3(120f, 2.8f, 9f), concreteMaterial, true);

            Material shoreFoam = CreateMaterial(new Color(0.72f, 0.88f, 0.86f, 0.7f), 0f, 0.35f);
            CreateCube("West shoreline foam", new Vector3(-44.3f, -0.43f, 70f), new Vector3(1.1f, 0.05f, 185f), shoreFoam, false);
            CreateCube("East shoreline foam", new Vector3(45.7f, -0.43f, 86f), new Vector3(1.1f, 0.05f, 165f), shoreFoam, false);
            CreateCube("Harbor wall foam", new Vector3(0f, -0.42f, 122.9f), new Vector3(118f, 0.05f, 0.8f), shoreFoam, false);

            for (int i = 0; i < 12; i++)
            {
                float z = 12f + i * 8f;
                CreatePrimitiveChild(PrimitiveType.Sphere, $"West rock {i + 1}", null,
                    new Vector3(-47f + Mathf.Sin(i * 1.8f) * 2f, 0f, z),
                    new Vector3(2.4f + (i % 3), 1.1f, 2f + (i % 2)), concreteMaterial, Vector3.zero, true);
            }

            Random.State coastState = Random.state;
            Random.InitState(244);
            for (int i = 0; i < 30; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float x = side * Random.Range(56f, 84f);
                float z = Random.Range(-5f, 165f);
                float size = Random.Range(2.2f, 5.5f);
                CreatePrimitiveChild(PrimitiveType.Sphere, $"Coastal scrub {i + 1}", null,
                    new Vector3(x, 1.4f, z), new Vector3(size, Random.Range(1.4f, 3f), size), grassMaterial, Vector3.zero, false);
            }
            Random.state = coastState;
        }

        private void BuildHarborBuildings()
        {
            CreateWarehouse("Operations building", new Vector3(-60f, 4.2f, 73f), new Vector3(20f, 7f, 24f));
            CreateWarehouse("Maintenance hangar", new Vector3(61f, 4.6f, 100f), new Vector3(25f, 8f, 30f));
            CreateWarehouse("Harbor office", new Vector3(57f, 3.2f, 49f), new Vector3(14f, 5.2f, 16f));

            for (int i = 0; i < 5; i++)
            {
                CreateCube($"Container red {i}", new Vector3(50f + (i % 2) * 4.2f, 2.4f, 74f + i * 5f), new Vector3(3.8f, 2.2f, 4.5f), buoyRedMaterial, true);
                CreateCube($"Container blue {i}", new Vector3(55f + (i % 2) * 4.2f, 2.4f, 76f + i * 5f), new Vector3(3.8f, 2.2f, 4.5f), seaMaterial, true);
            }

            BuildCrane(new Vector3(45f, 2f, 112f));
            BuildCrane(new Vector3(-48f, 2f, 101f));
            BuildLighthouse(new Vector3(-38f, 2.2f, 5.5f));

            for (int i = 0; i < 9; i++)
            {
                float z = 24f + i * 10f;
                CreatePrimitiveChild(PrimitiveType.Cylinder, $"Bollard {i + 1}", null,
                    new Vector3(33.5f, 2.35f, z), new Vector3(0.45f, 0.5f, 0.45f), darkMaterial, Vector3.zero, false);
            }
        }

        private void BuildNavigationMarkers()
        {
            for (int i = 0; i < 8; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                Material buoyMaterial = side < 0f ? buoyRedMaterial : buoyGreenMaterial;
                Vector3 position = new(side * (23f + (i % 3) * 1.5f), -0.15f, 14f + i * 14f);
                GameObject buoy = CreatePrimitiveChild(PrimitiveType.Cylinder, $"Buoy {i + 1}", null, position,
                    new Vector3(0.55f, 1.15f, 0.55f), buoyMaterial, Vector3.zero, false);
                CreatePrimitiveChild(PrimitiveType.Sphere, "Top mark", buoy.transform, new Vector3(0f, 1.05f, 0f),
                    new Vector3(0.34f, 0.34f, 0.34f), buoyMaterial, Vector3.zero, false);
            }
        }

        private void BuildDistantTown()
        {
            Random.State previousState = Random.state;
            Random.InitState(2026);
            for (int i = 0; i < 24; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float width = Random.Range(5f, 11f);
                float height = Random.Range(5f, 17f);
                float depth = Random.Range(6f, 13f);
                float x = side * Random.Range(66f, 88f);
                float z = Random.Range(28f, 155f);
                Material material = i % 3 == 0 ? superstructureMaterial : concreteMaterial;
                CreateCube($"Distant building {i + 1}", new Vector3(x, 1.25f + height * 0.5f, z), new Vector3(width, height, depth), material, true);
            }
            Random.state = previousState;
        }

        private ShipController BuildShip()
        {
            GameObject root = new("TCG BARBAROS F-244");
            root.transform.position = new Vector3(0f, 0.1f, -38f);
            root.AddComponent<Rigidbody>();
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.12f, 0.4f);
            collider.size = new Vector3(3.7f, 1.85f, 29.3f);

            GameObject visuals = new("Visuals");
            visuals.transform.SetParent(root.transform, false);
            visuals.AddComponent<ShipVisualMotion>();
            new BarbarosModelBuilder(visuals.transform).Build();
            BuildTurkishFlag(visuals.transform);

            ShipController controller = root.AddComponent<ShipController>();
            root.AddComponent<WakeTrail>().Configure(controller, foamMaterial);
            return controller;
        }


        private void BuildTurkishFlag(Transform parent)
        {
            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flag.name = "Turkish naval ensign";
            flag.transform.SetParent(parent, false);
            flag.transform.localPosition = new Vector3(0f, 2.25f, -14f);
            flag.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            flag.transform.localScale = new Vector3(0.78f, 0.51f, 1f);
            Collider flagCollider = flag.GetComponent<Collider>();
            flagCollider.enabled = false;
            Destroy(flagCollider);
            Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default");
            Material flagMaterial = new(shader) { mainTexture = CreateTurkishFlagTexture() };
            flag.GetComponent<Renderer>().sharedMaterial = flagMaterial;
        }

        private Texture2D CreateTurkishFlagTexture()
        {
            const int width = 160;
            const int height = 104;
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false) { name = "Turkish flag" };
            Color red = new(0.78f, 0.035f, 0.055f);
            Vector2 crescentCenter = new(65f, 52f);
            Vector2 cutoutCenter = new(74f, 52f);
            Vector2 starCenter = new(98f, 52f);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new(x, y);
                bool crescent = Vector2.Distance(point, crescentCenter) < 27f && Vector2.Distance(point, cutoutCenter) > 21f;
                Vector2 delta = point - starCenter;
                float angle = Mathf.Atan2(delta.y, delta.x) + Mathf.PI * 0.5f;
                float starRadius = Mathf.Cos(5f * angle) >= 0f ? 12f : 5f;
                bool star = delta.magnitude < starRadius;
                texture.SetPixel(x, y, crescent || star ? Color.white : red);
            }
            texture.Apply();
            return texture;
        }

        private void BuildCamera(Transform target)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 420f;
            camera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
            FollowCamera follow = cameraObject.AddComponent<FollowCamera>();
            follow.SetTarget(target);
        }

        private void BuildHud(ShipController ship)
        {
            PrototypeHud hud = gameObject.AddComponent<PrototypeHud>();
            hud.SetShip(ship);
        }

        private void CreateWarehouse(string name, Vector3 position, Vector3 scale)
        {
            CreateCube(name, position, scale, superstructureMaterial, true);
            CreateCube(name + " roof", position + Vector3.up * (scale.y * 0.52f), new Vector3(scale.x * 1.04f, 0.45f, scale.z * 1.05f), roofMaterial, false);
            for (int i = -1; i <= 1; i++)
            {
                CreateCube(name + " door", position + new Vector3(i * scale.x * 0.27f, -scale.y * 0.16f, -scale.z * 0.505f), new Vector3(scale.x * 0.18f, scale.y * 0.45f, 0.08f), darkMaterial, false);
            }
        }

        private void BuildCrane(Vector3 position)
        {
            CreateCube("Crane tower", position + new Vector3(0f, 7f, 0f), new Vector3(1.1f, 14f, 1.1f), orangeMaterial, true);
            CreateCube("Crane boom", position + new Vector3(-4f, 13.5f, 0f), new Vector3(9f, 0.7f, 0.8f), orangeMaterial, false);
            CreateCube("Crane cable", position + new Vector3(-7.8f, 9f, 0f), new Vector3(0.12f, 8f, 0.12f), darkMaterial, false);
        }

        private void BuildLighthouse(Vector3 position)
        {
            CreatePrimitiveChild(PrimitiveType.Cylinder, "Lighthouse tower", null, position + Vector3.up * 3f,
                new Vector3(1.15f, 3f, 1.15f), whiteMaterial, Vector3.zero, true);
            CreatePrimitiveChild(PrimitiveType.Cylinder, "Lighthouse stripe", null, position + Vector3.up * 3.2f,
                new Vector3(1.18f, 0.55f, 1.18f), buoyRedMaterial, Vector3.zero, false);
            CreatePrimitiveChild(PrimitiveType.Cylinder, "Lighthouse gallery", null, position + Vector3.up * 6.25f,
                new Vector3(1.55f, 0.18f, 1.55f), darkMaterial, Vector3.zero, false);

            GameObject beaconObject = new("Lighthouse beacon");
            beaconObject.transform.position = position + Vector3.up * 6.8f;
            Light beacon = beaconObject.AddComponent<Light>();
            beacon.type = LightType.Point;
            beacon.color = new Color(1f, 0.86f, 0.46f);
            beacon.range = 32f;
            beacon.shadows = LightShadows.None;
            beaconObject.AddComponent<BeaconPulse>();
        }


        private Texture2D CreateWaveTexture()
        {
            const int size = 128;
            Texture2D texture = new(size, size, TextureFormat.RGB24, false)
            {
                name = "Procedural harbor water",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            Color deep = new(0.025f, 0.23f, 0.34f);
            Color light = new(0.08f, 0.43f, 0.52f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float waveA = Mathf.Sin((x + y * 1.7f) * 0.22f);
                    float waveB = Mathf.Sin((x * 0.47f - y) * 0.13f);
                    float waveC = Mathf.Sin((x - y * 2.1f) * 0.07f);
                    float value = Mathf.InverseLerp(-2.5f, 2.5f, waveA + waveB + waveC * 0.5f);
                    texture.SetPixel(x, y, Color.Lerp(deep, light, value));
                }
            }
            texture.Apply();
            return texture;
        }

        private Material CreateMaterial(Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
            Material material = new(shader) { color = color };
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider) Destroy(cube.GetComponent<Collider>());
            return cube;
        }

        private GameObject CreateShipPart(PrimitiveType type, string name, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material, Vector3? localEuler = null)
        {
            return CreatePrimitiveChild(type, name, parent, localPosition, localScale, material, localEuler ?? Vector3.zero, false);
        }

        private GameObject CreatePrimitiveChild(PrimitiveType type, string name, Transform parent, Vector3 position,
            Vector3 scale, Material material, Vector3 euler, bool keepCollider)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            if (parent != null)
            {
                part.transform.SetParent(parent, false);
                part.transform.localPosition = position;
                part.transform.localEulerAngles = euler;
            }
            else
            {
                part.transform.position = position;
                part.transform.eulerAngles = euler;
            }
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider)
            {
                Collider partCollider = part.GetComponent<Collider>();
                partCollider.enabled = false;
                Destroy(partCollider);
            }
            return part;
        }
    }
}
