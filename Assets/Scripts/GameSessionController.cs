using UnityEngine;

namespace Meridian
{
    public sealed class GameSessionController : MonoBehaviour
    {
        private PrototypeBootstrap bootstrap;
        private ShipController ship;
        private FollowCamera follow;
        private Vector3 previousPosition;
        private float resumeTimeScale = 1f;
        public SimulationMode ActiveMode { get; private set; } = SimulationMode.Harbor;
        public VesselId ActiveVessel { get; private set; } = VesselId.Barbaros;
        public SeaStateSettings ActiveSea { get; private set; } = SeaStateSettings.Default;
        public bool MenuOpen { get; private set; }
        public bool HasStarted { get; private set; }
        public bool CanControl => HasStarted && !MenuOpen && (Tugs==null||!Tugs.IntentOpen) && (Missions==null||!Missions.BlocksControl);
        public TugAssistController Tugs { get; private set; }
        public MissionController Missions { get; private set; }
        public MaritimeAudio Audio { get; private set; }
        public MooringController Mooring { get; private set; }
        public VoyageProgress Voyage { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public bool Completed => Voyage != null && Voyage.Complete || Missions!=null&&Missions.Terminal;
        public ShipController PlayerShip => ship;

        public static bool IsAvailable(SimulationMode mode) => mode == SimulationMode.Harbor || mode == SimulationMode.StraitPassage;

        public void Initialize(PrototypeBootstrap owner, ShipController controller, FollowCamera camera)
        {
            bootstrap = owner; ship = controller; follow = camera;
            Tugs=gameObject.AddComponent<TugAssistController>();
            Tugs.Initialize(this,ship,camera.GetComponent<Camera>());
            Audio=gameObject.AddComponent<MaritimeAudio>();Audio.Initialize(this);
            Missions=gameObject.AddComponent<MissionController>();Missions.Initialize(this,owner,ship,camera,Audio);
            Mooring=gameObject.AddComponent<MooringController>();Mooring.Initialize(this,ship);
            follow.BindSession(this);
            HelmLayout.Configure(ship.Definition);
            OpenMenu();
        }

        public bool StartMode(SimulationMode mode) => StartMode(mode, ActiveVessel);

        public bool StartMode(SimulationMode mode, VesselId vessel)=>StartMode(mode,vessel,ActiveSea);

        public bool StartMode(SimulationMode mode, VesselId vessel,SeaStateSettings sea)
        {
            if (!IsAvailable(mode) || !VesselCatalog.IsAvailable(vessel) || bootstrap == null || !sea.IsValid) return false;
            Missions?.ClearForSailing();Audio?.ResetForVoyage();
            ActiveSea=sea;WaterWaves.Reset(sea,Time.time);
            bootstrap.LoadMode(mode, vessel);
            HelmLayout.Configure(ship.Definition);
            ActiveMode = mode;
            ActiveVessel = vessel;
            Voyage = mode == SimulationMode.StraitPassage ? new VoyageProgress(StraitRoute.Positions()) : null;
            previousPosition = ship.transform.position;
            ElapsedSeconds = 0f;
            HasStarted = true;
            MenuOpen = false;
            Tugs?.BeginMode(mode,vessel);
            Mooring?.BeginMode(mode);
            Time.timeScale = resumeTimeScale > 0f ? resumeTimeScale : 1f;
            follow.SetInputBlocked(Tugs!=null&&Tugs.IntentOpen);
            Audio?.Say("free_sail");
            return true;
        }

        public void OpenMenu()
        {
            if (MenuOpen) return;
            resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            MenuOpen = true;
            Time.timeScale = 0f;
            if (follow != null) follow.SetInputBlocked(true);
        }

        public void Resume()
        {
            if (!HasStarted || Completed) return;
            MenuOpen = false;
            Time.timeScale = Missions!=null&&Missions.BlocksControl?0:resumeTimeScale;
            follow.SetInputBlocked(Tugs!=null&&Tugs.IntentOpen||Missions!=null&&Missions.BlocksControl);
        }
        public bool ApplySeaState(SeaStateSettings settings)
        {
            if(!settings.IsValid||Missions!=null&&Missions.Assigned)return false;
            ActiveSea=settings;WaterWaves.SetState(settings,Time.time);return true;
        }

        private void Update()
        {
            WaterWaves.ApplyShader(Time.time);
            if (ship == null) return;
            if(!MenuOpen&&follow!=null)follow.SetInputBlocked(Tugs!=null&&Tugs.IntentOpen||Missions!=null&&Missions.BlocksControl);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (MenuOpen) Resume(); else OpenMenu();
            }
            if (!CanControl) return;
            ElapsedSeconds += Time.deltaTime;
            if (Voyage != null && Voyage.Advance(previousPosition, ship.transform.position, StraitRoute.GateRadius) && Completed)
                OpenMenu();
            previousPosition = ship.transform.position;
        }

        private void OnApplicationPause(bool paused) { if (paused && HasStarted) OpenMenu(); }
        private void OnDestroy() { if (MenuOpen) Time.timeScale = resumeTimeScale; }
    }
}
