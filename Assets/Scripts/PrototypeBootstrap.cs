using UnityEngine;

namespace Meridian
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        private Material seaMaterial;
        private Material shipMaterial;
        private Material deckMaterial;
        private Material harborMaterial;
        private Material buoyMaterial;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            BuildMaterials();
            BuildEnvironment();
            ShipController ship = BuildShip();
            BuildCamera(ship.transform);
            BuildHud(ship);
        }

        private void BuildMaterials()
        {
            seaMaterial = CreateMaterial(new Color(0.035f, 0.30f, 0.39f), 0.25f, 0.65f);
            shipMaterial = CreateMaterial(new Color(0.34f, 0.41f, 0.42f), 0.2f, 0.35f);
            deckMaterial = CreateMaterial(new Color(0.66f, 0.70f, 0.69f), 0.15f, 0.25f);
            harborMaterial = CreateMaterial(new Color(0.31f, 0.34f, 0.33f), 0.05f, 0.25f);
            buoyMaterial = CreateMaterial(new Color(0.85f, 0.18f, 0.12f), 0.1f, 0.35f);
        }

        private void BuildEnvironment()
        {
            RenderSettings.ambientLight = new Color(0.48f, 0.57f, 0.60f);

            GameObject sun = new("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.93f, 0.78f);
            sun.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            CreateCube("Sea", new Vector3(0f, -0.65f, 35f), new Vector3(220f, 1f, 260f), seaMaterial, true);
            CreateCube("Left breakwater", new Vector3(-42f, 1.4f, 38f), new Vector3(18f, 3f, 90f), harborMaterial, true);
            CreateCube("Right pier", new Vector3(45f, 1.1f, 66f), new Vector3(24f, 2.4f, 72f), harborMaterial, true);
            CreateCube("Harbor wall", new Vector3(0f, 1.6f, 116f), new Vector3(110f, 3.2f, 10f), harborMaterial, true);

            for (int i = 0; i < 6; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject buoy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                buoy.name = $"Buoy {i + 1}";
                buoy.transform.position = new Vector3(side * 25f, 0.35f, 20f + i * 16f);
                buoy.transform.localScale = new Vector3(0.65f, 1f, 0.65f);
                buoy.GetComponent<Renderer>().sharedMaterial = buoyMaterial;
            }
        }

        private ShipController BuildShip()
        {
            GameObject root = new("Prototype Frigate");
            root.transform.position = new Vector3(0f, 0.55f, -36f);

            Rigidbody body = root.AddComponent<Rigidbody>();
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(5f, 1.8f, 15f);

            GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hull.name = "Hull";
            hull.transform.SetParent(root.transform, false);
            hull.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hull.transform.localScale = new Vector3(2.5f, 7.5f, 0.8f);
            hull.GetComponent<Renderer>().sharedMaterial = shipMaterial;
            Destroy(hull.GetComponent<Collider>());

            GameObject deck = CreateCube("Superstructure", new Vector3(0f, 1.15f, -0.6f), new Vector3(3.3f, 1.4f, 5.5f), deckMaterial, false);
            deck.transform.SetParent(root.transform, false);

            GameObject bridge = CreateCube("Bridge", new Vector3(0f, 2.15f, 0.4f), new Vector3(2.7f, 1f, 2.4f), deckMaterial, false);
            bridge.transform.SetParent(root.transform, false);

            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Mast";
            mast.transform.SetParent(root.transform, false);
            mast.transform.localPosition = new Vector3(0f, 3.4f, -0.4f);
            mast.transform.localScale = new Vector3(0.16f, 1.5f, 0.16f);
            mast.GetComponent<Renderer>().sharedMaterial = shipMaterial;
            Destroy(mast.GetComponent<Collider>());

            return root.AddComponent<ShipController>();
        }

        private void BuildCamera(Transform target)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.36f, 0.61f, 0.68f);
            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.2f;
            FollowCamera follow = cameraObject.AddComponent<FollowCamera>();
            follow.SetTarget(target);
        }

        private void BuildHud(ShipController ship)
        {
            PrototypeHud hud = gameObject.AddComponent<PrototypeHud>();
            hud.SetShip(ship);
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
            if (!keepCollider)
            {
                Destroy(cube.GetComponent<Collider>());
            }
            return cube;
        }
    }
}
