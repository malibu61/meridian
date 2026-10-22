using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Meridian.Editor
{
    public static class ModeSelectionReview
    {
        private static double nextCheck;
        private static int motionStage;
        private static double motionDeadline;
        private static Vector3 motionStart, pausedPosition;
        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.delayCall += RunChecks;
            EditorApplication.update -= RunRequested;
            EditorApplication.update += RunRequested;
        }

        [MenuItem("Meridian/Modes - Check Route Logic")]
        public static void RunChecks()
        {
            int checks = 0;
            void Check(bool result, string name) { checks++; if (!result) throw new InvalidOperationException("Mode check: " + name); }
            Check(GameSessionController.IsAvailable(SimulationMode.Harbor),"harbor available");
            Check(GameSessionController.IsAvailable(SimulationMode.StraitPassage),"strait available");
            Check(!GameSessionController.IsAvailable(SimulationMode.BuoyMooring),"future mode cannot launch");
            Check(!GameSessionController.IsAvailable((SimulationMode)99),"unknown mode cannot launch");
            Vector3[] points = { Vector3.zero,Vector3.forward * 10f,Vector3.forward * 20f };
            VoyageProgress progress = new(points);
            Check(progress.NextIndex == 1 && progress.Fraction == 0f,"first objective after spawn");
            Check(!progress.Advance(Vector3.forward * 18f,Vector3.forward * 22f,1f),"cannot skip first objective");
            Check(!progress.Advance(Vector3.forward * 11f,Vector3.forward * 9f,1f),"wrong direction rejected");
            Check(!progress.Advance(Vector3.forward * 8f + Vector3.right * 5f,Vector3.forward * 12f + Vector3.right * 5f,1f),"off-route miss rejected");
            Check(progress.Advance(Vector3.forward * 8f,Vector3.forward * 12f,1f),"swept checkpoint");
            Check(progress.PassedCount == 1 && Mathf.Abs(progress.Fraction - .5f) < .001f,"progress halfway");
            Check(!progress.Advance(Vector3.forward * 12f,Vector3.forward * 12f,1f),"stationary frame cannot advance");
            Check(!progress.Advance(Vector3.zero,new Vector3(float.NaN,0f,20f),1f),"invalid position ignored");
            Check(!progress.Advance(Vector3.zero,Vector3.forward * 20f,0f),"invalid radius ignored");
            Check(progress.Advance(Vector3.forward * 18f,Vector3.forward * 22f,1f),"last gate accepted");
            Check(progress.Complete && progress.Fraction == 1f && progress.PassedCount == 2,"completion");
            Check(!progress.Advance(Vector3.forward * 18f,Vector3.forward * 22f,1f),"cannot repeat completion");
            bool invalidRejected = false;
            try { _ = new VoyageProgress(new[] { Vector3.zero }); } catch (ArgumentException) { invalidRejected = true; }
            Check(invalidRejected,"empty route rejected");
            Check(StraitRoute.Count == StraitRoute.Names.Length && StraitRoute.Count == StraitRoute.HalfWidths.Length,"route metadata matches");
            Check(StraitRoute.Point(StraitRoute.Count - 1).z < StraitRoute.Point(0).z,"north to south");
            for (int i = 0; i < StraitRoute.Count; i++)
            {
                Check(StraitRoute.Direction(i).sqrMagnitude > .99f,"valid direction " + i);
                Check(StraitRoute.HalfWidths[i] > StraitRoute.GateRadius,"gate inside corridor " + i);
            }
            Debug.Log($"MODE_ROUTE_CHECK_PASS checks={checks}");
        }

        private static void RunRequested()
        {
            CheckMotion();
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + .5;
            const string folder = "Temp/ModeSelectionReview", request = folder + "/request.txt";
            if (!File.Exists(request)) return;
            string command = File.ReadAllText(request).Trim();
            if (command.Length == 0 || SessionState.GetString("Meridian.ModeReview.Token","") == command) return;
            GameSessionController session = Object.FindAnyObjectByType<GameSessionController>();
            if (session == null) return;
            SessionState.SetString("Meridian.ModeReview.Token",command);
            string action = command.Split(':')[0];
            if (action == "checks") RuntimeChecks(session);
            else if (action == "strait") session.StartMode(SimulationMode.StraitPassage);
            else if (action == "harbor") session.StartMode(SimulationMode.Harbor);
            else if (action == "menu") session.OpenMenu();
            else if (action == "resume") session.Resume();
            else if (action == "bridge")
            {
                session.StartMode(SimulationMode.StraitPassage);
                session.PlayerShip.ResetSimulation(StraitRoute.Point(2) - StraitRoute.Direction(2) * 800f,
                    Quaternion.LookRotation(StraitRoute.Direction(2),Vector3.up));
            }
            else if (action == "cruise") { session.PlayerShip.SetPortThrottle(.7f); session.PlayerShip.SetStarboardThrottle(.7f); }
            else if (action == "motion")
            {
                session.StartMode(SimulationMode.StraitPassage);
                motionStart = session.PlayerShip.transform.position;
                session.PlayerShip.SetPortThrottle(.7f); session.PlayerShip.SetStarboardThrottle(.7f);
                motionStage = 1; motionDeadline = EditorApplication.timeSinceStartup + 3.0;
            }
            else if (action == "capture")
            {
                string name = command.Substring(command.IndexOf(':') + 1);
                if (name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath(folder + "/" + name + ".png"));
            }
            Debug.Log($"MODE_RUNTIME_STATE action={action} mode={session.ActiveMode} menu={session.MenuOpen} started={session.HasStarted} paused={Time.timeScale == 0f} position={session.PlayerShip.transform.position} speed={session.PlayerShip.SpeedKnots:F2}");
        }

        private static void CheckMotion()
        {
            if (motionStage == 0 || !Application.isPlaying || EditorApplication.timeSinceStartup < motionDeadline) return;
            GameSessionController session = Object.FindAnyObjectByType<GameSessionController>();
            if (session == null) { motionStage = 0; return; }
            if (motionStage == 1)
            {
                float distance = Vector3.Distance(motionStart,session.PlayerShip.transform.position);
                if (distance < .2f) { motionStage = 0; throw new InvalidOperationException("Mode motion: ship did not move in real FixedUpdate."); }
                pausedPosition = session.PlayerShip.transform.position;
                session.OpenMenu(); motionStage = 2; motionDeadline = EditorApplication.timeSinceStartup + .7;
                Debug.Log($"MODE_MOTION_PASS distance={distance:F3} speed={session.PlayerShip.SpeedKnots:F2}");
            }
            else
            {
                motionStage = 0;
                if (Vector3.Distance(pausedPosition,session.PlayerShip.transform.position) > .001f)
                    throw new InvalidOperationException("Mode pause: ship moved while menu was open.");
                Debug.Log("MODE_PAUSE_PASS ship position unchanged while menu open");
            }
        }

        [MenuItem("Meridian/Modes - Check Runtime Switching")]
        public static void RunLive()
        {
            GameSessionController session = Object.FindAnyObjectByType<GameSessionController>();
            if (session == null || !Application.isPlaying) { Debug.LogWarning("Enter Play mode first."); return; }
            RuntimeChecks(session);
        }

        private static void RuntimeChecks(GameSessionController session)
        {
            int checks = 0;
            void Check(bool result,string name) { checks++; if (!result) throw new InvalidOperationException("Mode runtime: " + name); }
            Check(session.StartMode(SimulationMode.StraitPassage),"start strait");
            Check(session.ActiveMode == SimulationMode.StraitPassage && session.CanControl && session.Voyage != null,"strait state");
            Check(Vector3.Distance(session.PlayerShip.transform.position,StraitRoute.Point(0)) < .01f,"northern spawn");
            Check(Vector3.Dot(session.PlayerShip.transform.forward,StraitRoute.Direction(0)) > .99f,"southern heading");
            session.PlayerShip.SetPortThrottle(1f); session.PlayerShip.SetStarboardThrottle(-1f); session.PlayerShip.SetRudder(.8f);
            session.OpenMenu();
            Check(session.MenuOpen && !session.CanControl && Time.timeScale == 0f,"menu pauses");
            Check(!session.StartMode(SimulationMode.BuoyMooring) && session.MenuOpen,"future mode blocked without closing menu");
            session.Resume(); Check(session.CanControl && Time.timeScale > 0f,"resume");
            Check(session.StartMode(SimulationMode.Harbor),"return to harbor");
            Check(session.Voyage == null && session.ActiveMode == SimulationMode.Harbor,"no leaked route");
            Check(session.PlayerShip.PortThrottle == 0f && session.PlayerShip.StarboardThrottle == 0f && session.PlayerShip.Rudder == 0f,"controls reset");
            Check(session.PlayerShip.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude < .0001f,"speed reset");
            Check(Vector3.Distance(session.PlayerShip.transform.position,new Vector3(0f,.1f,-38f)) < .01f,"harbor spawn restored");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                session.StartMode(SimulationMode.StraitPassage);
                Check(session.Voyage.PassedCount == 0,"new voyage " + cycle);
                Check(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length == 1,"single camera " + cycle);
                Check(Object.FindObjectsByType<ShipController>(FindObjectsSortMode.None).Length == 1,"single ship " + cycle);
                session.StartMode(SimulationMode.Harbor);
            }
            // Exercise the actual geographic route progression without simulating a full real-time passage.
            session.StartMode(SimulationMode.StraitPassage);
            for (int i = 1; i < StraitRoute.Count; i++)
            {
                Vector3 direction = (StraitRoute.Point(i) - StraitRoute.Point(i - 1)).normalized;
                Check(session.Voyage.Advance(StraitRoute.Point(i) - direction * 150f,StraitRoute.Point(i) + direction * 150f,StraitRoute.GateRadius),"geographic gate " + i);
            }
            Check(session.Completed,"all geographic gates complete");
            session.OpenMenu(); session.Resume(); Check(session.MenuOpen,"completed voyage cannot resume");
            session.StartMode(SimulationMode.Harbor); session.OpenMenu();
            Debug.Log($"MODE_RUNTIME_CHECK_PASS checks={checks}");
        }
    }
}
