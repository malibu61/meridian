using System.IO;
using UnityEditor;
using UnityEngine;

namespace Meridian.Editor
{
    public static class BarbarosVisualReview
    {
        private static double nextCheck;
        private static NavalDeckLife pendingLiveCheck;
        private static double liveCheckDeadline;

        [InitializeOnLoadMethod]
        private static void EnableRequestedReview()
        {
            EditorApplication.update -= RunRequestedReview;
            EditorApplication.update += RunRequestedReview;
        }

        private static void RunRequestedReview()
        {
            if (EditorApplication.isCompiling || !EditorApplication.isPlaying || EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + 2.0;
            if (pendingLiveCheck != null)
            {
                if (pendingLiveCheck.AnimatedFrames >= 20 && pendingLiveCheck.ObservedHandTravel > .04f
                    && pendingLiveCheck.ObservedPatrolTravel > .15f && pendingLiveCheck.ObservedLookoutTurn > 5f)
                {
                    Debug.Log($"DECK_LIFE_RUNTIME_PASS frames={pendingLiveCheck.AnimatedFrames} localHandTravel={pendingLiveCheck.ObservedHandTravel:F3} patrolTravel={pendingLiveCheck.ObservedPatrolTravel:F3} lookoutTurn={pendingLiveCheck.ObservedLookoutTurn:F1}");
                    pendingLiveCheck = null;
                }
                else if (EditorApplication.timeSinceStartup > liveCheckDeadline)
                {
                    Debug.LogError("Deck crew did not animate during the live Play-mode check."); pendingLiveCheck = null;
                }
            }
            if (SessionState.GetBool("Meridian.VisualReview.Startup", false))
            {
                if (Object.FindAnyObjectByType<ShipController>() == null) return;
                SessionState.SetBool("Meridian.VisualReview.Startup", false);
                Capture();
                return;
            }
            const string request = "Temp/BarbarosVisualReview/request.txt";
            if (!File.Exists(request)) return;
            string token = File.ReadAllText(request).Trim();
            if (SessionState.GetString("Meridian.VisualReview.Completed", "") == token) return;
            if (Object.FindAnyObjectByType<ShipController>() == null) return;
            SessionState.SetString("Meridian.VisualReview.Completed", token);
            Capture();
        }

        // Also supports Unity -executeMethod Meridian.Editor.BarbarosVisualReview.StartReview.
        [MenuItem("Meridian/Visual Review - Run")]
        public static void StartReview()
        {
            SessionState.SetBool("Meridian.VisualReview.Startup", true);
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
        }

        [MenuItem("Meridian/Visual Review - 8 Views")]
        public static void Capture()
        {
            ShipController ship = Object.FindAnyObjectByType<ShipController>();
            if (ship == null) { Debug.LogWarning("Enter Play mode before capturing the exterior review."); return; }
            Transform visuals = ship.transform.Find("Visuals");
            if (visuals != null)
            {
                // Runtime-only rebuild makes the review reflect the latest compiled builder after a hot reload.
                for (int child = visuals.childCount - 1; child >= 0; child--)
                {
                    Transform part = visuals.GetChild(child);
                    if (part.name != "Turkish naval ensign") Object.DestroyImmediate(part.gameObject);
                }
                new BarbarosModelBuilder(visuals).Build();
            }
            string folder = Path.GetFullPath("Temp/BarbarosVisualReview");
            Directory.CreateDirectory(folder);
            string[] names = { "01-port", "02-starboard", "03-bow", "04-stern", "05-port-bow", "06-starboard-stern", "07-top", "08-low-port" };
            float[] yaw = { -90f, 90f, 180f, 0f, -135f, 45f, 0f, -90f };
            float[] pitch = { 7f, 7f, 9f, 9f, 23f, 23f, 87f, 1f };
            float[] distance = { 35f, 35f, 36f, 36f, 38f, 38f, 54f, 35f };
            const int width = 1280, height = 720, cellWidth = 640, cellHeight = 360;
            Texture2D contact = new(cellWidth * 4, cellHeight * 2, TextureFormat.RGB24, false);
            GameObject cameraObject = new("Exterior review camera");
            Camera camera = cameraObject.AddComponent<Camera>(); camera.enabled = false;
            camera.fieldOfView = 34f; camera.nearClipPlane = 0.15f; camera.farClipPlane = 420f;
            camera.clearFlags = CameraClearFlags.Skybox; camera.allowHDR = false;
            RenderTexture rt = new(width, height, 24);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = rt;
                for (int view = 0; view < names.Length; view++)
                {
                    Vector3 focus = ship.transform.position + Vector3.up * 2.0f;
                    Quaternion orbit = ship.transform.rotation * Quaternion.Euler(pitch[view], yaw[view], 0f);
                    camera.transform.position = focus + orbit * Vector3.back * distance[view];
                    camera.transform.LookAt(focus);
                    camera.Render(); RenderTexture.active = rt;
                    Texture2D capture = new(width, height, TextureFormat.RGB24, false);
                    capture.ReadPixels(new Rect(0, 0, width, height), 0, 0); capture.Apply();
                    File.WriteAllBytes(Path.Combine(folder, names[view] + ".png"), capture.EncodeToPNG());
                    Color[] thumbnail = new Color[cellWidth * cellHeight];
                    for (int y = 0; y < cellHeight; y++)
                    for (int x = 0; x < cellWidth; x++)
                        thumbnail[y * cellWidth + x] = capture.GetPixel(x * 2, y * 2);
                    contact.SetPixels((view % 4) * cellWidth, (1 - view / 4) * cellHeight, cellWidth, cellHeight, thumbnail);
                    Object.DestroyImmediate(capture);
                }
                contact.Apply(); File.WriteAllBytes(Path.Combine(folder, "contact-sheet.png"), contact.EncodeToPNG());
                CaptureDeckLife(ship, camera, rt, folder);
                Debug.Log("BARBAROS_VISUAL_REVIEW_COMPLETE " + folder);
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null;
                Object.DestroyImmediate(cameraObject); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(contact);
            }
        }

        private static void CaptureDeckLife(ShipController ship, Camera camera, RenderTexture rt, string folder)
        {
            NavalDeckLife life = ship.GetComponentInChildren<NavalDeckLife>();
            if (life == null) throw new System.InvalidOperationException("Deck life was not built.");
            life.SampleMotion(0f); Vector3 before = life.FirstHandPosition;
            string[] names = { "09-seahawk", "10-bow-crew", "11-stern-crew", "12-crew-haul", "13-seahawk-bow-quarter", "14-port-wing-out", "15-starboard-wing", "16-port-wing-in", "17-bow-complement", "18-stern-complement" };
            Vector3[] targets = { new(-.1f,1.62f,-11.40f), new(-1.02f,1.60f,9.72f), new(1.1f,1.29f,-12.78f), new(-1.02f,1.60f,9.72f), new(-.1f,1.62f,-11.40f),new(-1.6f,2.72f,3.22f),new(1.6f,2.72f,3.22f),new(-1.6f,2.72f,3.22f),new(0f,1.6f,9.68f),new(0f,1.3f,-12.8f) };
            Vector3[] offsets = { new(-3.8f,2.4f,-4.2f), new(-1.35f,.82f,1.4f), new(-1.4f,1.8f,-1.2f), new(-1.35f,.82f,1.4f), new(-2.9f,1.3f,1.8f),new(-1.8f,.82f,.60f),new(1.8f,.82f,.60f),new(-1.8f,.82f,.60f),new(0f,4.8f,-2.4f),new(.3f,2.9f,-2.4f) };
            float[] times={0f,0f,0f,1.8f,0f,3f,3f,6.2f,5f,5f};
            for (int i = 0; i < names.Length; i++)
            {
                life.SampleMotion(times[i]);
                camera.transform.position = ship.transform.TransformPoint(targets[i] + offsets[i]);
                camera.transform.LookAt(ship.transform.TransformPoint(targets[i]));
                camera.nearClipPlane = .025f;
                camera.Render(); RenderTexture.active = rt;
                Texture2D capture = new(rt.width,rt.height,TextureFormat.RGB24,false);
                capture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);capture.Apply();
                File.WriteAllBytes(Path.Combine(folder,names[i]+".png"),capture.EncodeToPNG());Object.DestroyImmediate(capture);
            }
            life.SampleMotion(1.8f);
            float movement = Vector3.Distance(before,life.FirstHandPosition);
            if (life.CrewCount != 26 || movement < .06f || life.MaximumGripError > .006f)
                throw new System.InvalidOperationException($"Deck animation check failed: crew={life.CrewCount}, movement={movement}, grip={life.MaximumGripError}");
            if(!life.ValidateBridgeRoutes(out string routeReport))throw new System.InvalidOperationException(routeReport);
            life.SampleMotion(0f);Vector3 patrolStart=life.FirstPatrolPosition;Quaternion headStart=life.FirstLookoutHead;
            life.SampleMotion(3f);
            float patrolMove=Vector3.Distance(patrolStart,life.FirstPatrolPosition),headTurn=Quaternion.Angle(headStart,life.FirstLookoutHead);
            if(patrolMove<.4f || headTurn<15f)throw new System.InvalidOperationException("Patrol or lookout pose check failed.");
            Debug.Log($"BRIDGE_WATCH_CHECK_PASS {routeReport}, patrolTravel={patrolMove:F3}, headTurn={headTurn:F1}");
            Debug.Log($"DECK_LIFE_CHECK_PASS crew={life.CrewCount} handTravel={movement:F3} gripError={life.MaximumGripError:F4}");
            life.SampleMotion(Time.time);
            pendingLiveCheck = life; liveCheckDeadline = EditorApplication.timeSinceStartup + 20.0;
        }
    }
}
