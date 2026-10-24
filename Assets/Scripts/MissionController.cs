using UnityEngine;
using System.Collections.Generic;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    public sealed class MissionController:MonoBehaviour
    {
        private GameSessionController session;private PrototypeBootstrap bootstrap;private ShipController ship;private FollowCamera follow;
        private MaritimeAudio audioSystem;private MaritimeUiTheme ui;private bool resultRecorded,approachSpoken,entrySpoken;private WhistleSignal lastExpected;
        private IMissionProgressStorage storage;private readonly Dictionary<VesselId,MissionProgressStore> careers=new();private VesselId careerVessel=VesselId.Kumkale;
        public MissionProgressStore Progress=>GetProgress(Run!=null?Run.Definition.Vessel:careerVessel);
        public MissionProgressStore GetProgress(VesselId vessel)
        {if(!careers.TryGetValue(vessel,out var progress)){progress=new MissionProgressStore(storage,vessel,MissionAccessPolicy.AllScenariosUnlockedForTesting);careers[vessel]=progress;}return progress;}
        public DockingMissionRun Run{get;private set;}public int Index{get;private set;}=-1;
        public bool Assigned=>Run!=null;public bool Running=>Run!=null&&Run.Phase==DockingMissionPhase.Running;
        public bool Terminal=>Run!=null&&(Run.Phase==DockingMissionPhase.Passed||Run.Phase==DockingMissionPhase.Failed);
        public bool BlocksControl=>Run!=null&&!Running;
        public ManeuverSignalMonitor Signals=>Run?.Signals;
        public void Initialize(GameSessionController owner,PrototypeBootstrap builder,ShipController controller,FollowCamera camera,MaritimeAudio sounds)
        {
            session=owner;bootstrap=builder;ship=controller;follow=camera;audioSystem=sounds;storage=new PlayerPrefsMissionStorage();
            var reporter=ship.GetComponent<MissionCollisionReporter>()??ship.gameObject.AddComponent<MissionCollisionReporter>();reporter.Owner=this;audioSystem.HornCompleted+=OnWhistle;
        }
        public void SetStorageForReview(IMissionProgressStorage reviewStorage){storage=reviewStorage;careers.Clear();careerVessel=VesselId.Kumkale;}
        public bool StartMission(int index)=>StartMission(Run!=null?Run.Definition.Vessel:careerVessel,index);
        public bool StartMission(VesselId vessel,int index)
        {
            if(!VesselCatalog.IsAvailable(vessel))return false;
            var definition=DockingMissionCatalog.Get(vessel,index);if(definition==null||!GetProgress(vessel).IsUnlocked(index))return false;
            if(!session.StartMode(SimulationMode.Harbor,vessel,definition.Sea))return false;
            Run=new DockingMissionRun(definition);Index=index;careerVessel=vessel;resultRecorded=approachSpoken=entrySpoken=false;lastExpected=WhistleSignal.None;
            Progress.Start(index);ship.ResetSimulation(definition.Spawn,Quaternion.identity);
            session.Tugs.BeginMode(SimulationMode.Harbor,vessel,definition.RequiresTugs);
            if(definition.IsRescue)session.Mooring.BeginMode(SimulationMode.StraitPassage);
            if(definition.RequiresTugs)session.Tugs.SelectBerth(Run.Berth);
            if(definition.IsRescue)ship.StartUnderway(definition.InitialKnots);
            else ship.GetComponent<Rigidbody>().linearVelocity=Vector3.forward*(definition.InitialKnots/(VesselCatalog.ModelMetresPerUnit*1.9438445f));
            bootstrap.BuildMissionEnvironment(Run);session.Mooring?.RefreshBerth();follow.ConfigureVessel(ship.Definition);follow.SetTarget(ship.transform);follow.SetInputBlocked(true);
            if(definition.IsRescue)follow.SetRescueView(definition.RescueSide);
            Time.timeScale=0;audioSystem.Say(definition.RadioId,true);return true;
        }
        public bool BeginMission()
        {if(Run==null||!Run.Begin())return false;Time.timeScale=1;follow.SetInputBlocked(false);return true;}
        public void ClearForSailing(){Run?.Rescue?.Stop();Run=null;Index=-1;resultRecorded=false;}
        public bool RescueAction()
        {
            if(!Running||session.MenuOpen||Run.Rescue==null)return false;
            var r=Run.Rescue;bool changed=r.Stage==RescueStage.Alarm?r.Alarm():r.Stage==RescueStage.Mark?r.Mark():r.Recover();
            if(changed)audioSystem.Say(r.Stage==RescueStage.Mark?"mob_alarm":r.Stage==RescueStage.Approach?"mob_mark":"mob_recover",true);
            return changed;
        }
        private void OnWhistle(WhistleSignal signal){if(Running)Run.SoundCompleted(signal);}
        public void ReportCollision(float speed){if(Running)Run.Collision(speed);}
        public void ReportTugDelivery(float dt,float bow,float stern,HarborBerth berth){if(Running)Run.RecordTugDelivery(dt,bow,stern,berth);}
        private void Update()
        {
            if(Terminal){if(!session.MenuOpen)RecordResult();return;}
            if(!Running||session.MenuOpen)return;var body=ship.GetComponent<Rigidbody>();
            if(Run.Rescue!=null)
            {
                bool stopped=true;for(int i=0;i<ship.EngineCount;i++)if(Mathf.Abs(ship.GetEngineThrottle(i))>.001f||Mathf.Abs(ship.GetEngineSignedLoad(i))>.04f)stopped=false;
                Run.TickRescue(Time.deltaTime,ship.transform.position,ship.Heading,ship.SpeedKnots,stopped,body.angularVelocity.y*Mathf.Rad2Deg,Mathf.DeltaAngle(0,ship.transform.eulerAngles.z));
                if(Terminal)RecordResult();return;
            }
            float forward=Vector3.Dot(body.linearVelocity,ship.ForwardOnWater)*4*1.9438445f,sway=Vector3.Dot(body.linearVelocity,ship.RightOnWater)*4*1.9438445f;
            bool astern=false;for(int i=0;i<ship.EngineCount;i++)if(ship.GetEngineSignedLoad(i)<-.04f)astern=true;
            Run.Tick(Time.deltaTime,ship.transform.position,ship.Heading,forward,sway,astern,ship.ActualRudder,ship.PortSignedLoad-ship.StarboardSignedLoad);
            if(Run.TargetChanged){if(Run.Definition.RequiresTugs)session.Tugs.SelectBerth(Run.Berth);audioSystem.Say(RadioId("reassign"),true);}
            else if(Run.GatesPassed>=1&&!entrySpoken){entrySpoken=true;audioSystem.Say(RadioId("entry"));}
            if(Run.GatesPassed==3&&!approachSpoken&&Vector3.Distance(ship.transform.position,Run.Target)<18){approachSpoken=true;audioSystem.Say(RadioId("approach"));}
            if(ship.Definition.HasFourEngines){float difference=Mathf.Max(Mathf.Abs(ship.GetEngineSignedLoad(0)-ship.GetEngineSignedLoad(2)),Mathf.Abs(ship.GetEngineSignedLoad(1)-ship.GetEngineSignedLoad(3)));Run.RecordEngineHandling(Time.deltaTime,difference,ship.SpeedKnots<5&&TugAssistRules.InArea(ship.transform.position,Run.Berth));}
            var expected=Signals.Expected;if(expected!=lastExpected&&expected!=WhistleSignal.None)audioSystem.Say(RadioId("signal_"+(int)expected),true);lastExpected=expected;
            if(Terminal)RecordResult();
        }
        private string RadioId(string id)=>Run.Definition.Vessel==VesselId.Kumkale?id:"fleet_"+id;
        public void RecordResult()
        {
            if(!Terminal||resultRecorded)return;resultRecorded=true;ship.EmergencyStop();
            if(Run.Phase==DockingMissionPhase.Passed){Progress.Complete(Index,Run.Stars,Run.Elapsed);audioSystem.Say(Run.Definition.IsRescue?"mob_success":RadioId(Progress.CompletedCount==DockingMissionCatalog.Count?"career_complete":"success"),true);}
            else audioSystem.Say(Run.Definition.IsRescue?"mob_failure":RadioId("failure"),true);
            Time.timeScale=0;follow.SetInputBlocked(true);
        }
        public static Rect HudRect(HelmLayout l)=>new(316,152,Mathf.Min(620,l.Width-658),124);
        private void OnGUI()
        {
            if(Run==null||session.MenuOpen)return;ui??=new MaritimeUiTheme();var l=HelmLayout.Current;var previous=GUI.matrix;GUI.matrix=Matrix4x4.TRS(l.Offset,Quaternion.identity,Vector3.one*l.Scale);GUI.depth=-22;
            try
            {
                if(Run.Phase==DockingMissionPhase.Briefing){DrawBrief(l);return;}
                if(Terminal){DrawResult(l);return;}
                if(Run.Rescue!=null){DrawRescue(l);return;}
                if(session.Mooring!=null&&session.Mooring.PanelVisible)return; // Mooring card carries the live signal and settle telemetry here.
                if(session.Tugs.Available)return; // The tug card carries mission gates, actual-support evidence and whistle deadline.
                Rect p=HudRect(l);ui.Surface(p);ui.Text(new Rect(p.x+12,p.y+7,p.width-24,23),$"{Index+1}/{DockingMissionCatalog.Count}  ·  {Run.Definition.Title}",14,Teal,bold:true);
                float metres=Vector3.Distance(new Vector3(ship.transform.position.x,0,ship.transform.position.z),new Vector3(Run.Target.x,0,Run.Target.z))*4;
                ui.Text(new Rect(p.x+12,p.y+35,p.width-24,23),$"GİRİŞ {Run.GatesPassed}/3  ·  {TugAssistRules.Name(Run.Berth)}  ·  HEDEF {metres:0} m",14,Ink);
                ui.Text(new Rect(p.x+12,p.y+61,p.width-24,23),$"YAN {Run.LastSwayKnots:+0.00;-0.00;0.00} kn  ·  BAŞ HATASI {Run.HeadingError:0}°  ·  SABİT {Run.HoldSeconds:0.0}/5 sn",13,Muted);
                string message=Signals.Expected!=WhistleSignal.None?$"ZORUNLU: {ManeuverSignalMonitor.Meaning(Signals.Expected)}  ·  {Signals.SecondsLeft(Run.Elapsed):0} sn":Run.GatesPassed<3?$"Sıradaki giriş kapısı: {Run.GatesPassed+1}. Liman içi görev sürat sınırı {Run.Definition.HarborSpeedLimit:0} kn.":!Run.ApproachWorkReady?Run.ExerciseHint:!Run.MooringComplete?"STOP · 0,60 kn altı · baş ve kıç halatlarını al; ekip çeksin.":"İki uç bağlı. Hedefte 5 sn sakin kal; manevra tamamlanacak.";
                ui.Text(new Rect(p.x+12,p.y+88,p.width-24,30),message,12,Signals.Expected!=WhistleSignal.None?Amber:Muted);
            }
            finally{GUI.matrix=previous;}
        }
        private void DrawRescue(HelmLayout l)
        {
            var r=Run.Rescue;Rect p=HudRect(l);ui.Surface(p);
            ui.Text(new Rect(p.x+12,p.y+6,p.width-24,22),$"6/{DockingMissionCatalog.Count} · DENİZE ADAM DÜŞTÜ · {(r.Side>0?"SANCAK":"İSKELE")} İSTASYON",13,Teal,bold:true);
            ui.Text(new Rect(p.x+12,p.y+31,p.width-24,20),$"MESAFE {r.DistanceMetres:0} m · NİSPİ {(r.RelativeBearing<0?"İSKELE":"SANCAK")} {Mathf.Abs(r.RelativeBearing):0}° · İSTASYON {r.StationGapMetres:0.0} m",12,Ink);
            string hint=!r.InWater?"PERSONEL BORDADAN DENİZE DÜŞÜYOR · Alarmı ver ve görüşte tut":r.Stage==RescueStage.Recovering?$"EKİP ALIYOR · {r.RecoverySeconds:0.0}/8 sn · STOP'u koru":r.HoldReason??$"GÜVENLİ KONUM · {r.StableSeconds:0.0}/4 sn sabit tut";
            ui.Text(new Rect(p.x+12,p.y+55,p.width-24,22),hint,12,r.HoldReason==null?Teal:Amber);
            string action=r.Stage==RescueStage.Alarm?"1 · ALARM VER":r.Stage==RescueStage.Mark?"2 · KONUMU İŞARETLE":r.Stage==RescueStage.Recovering?"KURTARMA SÜRÜYOR":"3 · KAZAZEDEYİ AL";
            bool old=GUI.enabled;GUI.enabled=session.CanControl&&(r.Stage==RescueStage.Alarm||r.Stage==RescueStage.Mark||r.CanRecover);
            if(GUI.Button(new Rect(p.x+12,p.y+82,p.width-24,33),action,ui.Primary))RescueAction();GUI.enabled=old;
        }
        private Rect Modal(HelmLayout l,string eyebrow,string title)
        {
            GUI.depth=-40;Fill(new Rect(-l.Offset.x/l.Scale,-l.Offset.y/l.Scale,Screen.width/l.Scale,Screen.height/l.Scale),new Color(.012f,.022f,.035f,.94f));
            Rect r=new((l.Width-820)*.5f,(l.Height-480)*.5f,820,480);ui.Surface(r);ui.Text(new Rect(r.x+28,r.y+22,764,24),eyebrow,14,Teal,bold:true);ui.Text(new Rect(r.x+28,r.y+62,764,48),title,30,Ink,bold:true);return r;
        }
        private void DrawBrief(HelmLayout l)
        {
            Rect r=Modal(l,$"GÖREV BRİFİNGİ  /  {ship.Definition.Name}  /  {Index+1}–{DockingMissionCatalog.Count}",Run.Definition.Title);
            ui.Text(new Rect(r.x+28,r.y+120,764,96),Run.Definition.Briefing,18,Ink);
            ui.Text(new Rect(r.x+28,r.y+224,764,74),Run.Definition.ExerciseBrief,17,Teal);
            ui.Text(new Rect(r.x+28,r.y+303,764,49),Run.Definition.IsRescue?"MOB alarmı paneldeki ALARM VER ile verilir; bir/iki/üç kısa veya beş uyarı MOB alarmı yerine sayılmaz. Römorkör ve rıhtım halatları bu eğitimde kapalıdır.":"Görüşte gemi var: sancak 1, iskele 2 kısa; tornistan SEVK 3 kısa. İstenen sinyali 15 sn içinde tamamla; düdük kendiliğinden çalmaz.",14,Muted);
            ui.Text(new Rect(r.x+28,r.y+357,764,47),Run.Definition.IsRescue?"Oyun eşikleri: istasyona ≤6 m · <0,50 kn · tüm makineler STOP · 4 sn sabit + 8 sn kurtarma. Kazazede üstünden geçmek başarısızlıktır.":$"3 giriş kapısı · liman sınırı {Run.Definition.HarborSpeedLimit:0} kn · iki halat · baş ±6° / düşük sürat 5 sn. Sert temas başarısızlıktır.",14,Muted);
            if(GUI.Button(new Rect(r.x+28,r.y+412,230,44),"GÖREVLER MENÜSÜ",ui.Button)){session.GetComponent<ModeSelectionMenu>().ShowMissionOptions();session.OpenMenu();}
            if(GUI.Button(new Rect(r.xMax-320,r.y+412,292,44),"ANLAŞILDI · BAŞLA  →",ui.Primary)){BeginMission();GUIUtility.ExitGUI();}
        }
        private void DrawResult(HelmLayout l)
        {
            bool passed=Run.Phase==DockingMissionPhase.Passed;Rect r=Modal(l,"MANEVRA DEĞERLENDİRMESİ",passed?"Görev tamamlandı.":"Yeniden deneyebilirsin.");
            ui.Text(new Rect(r.x+28,r.y+126,764,55),passed?$"{new string('★',Run.Stars)}{new string('☆',3-Run.Stars)}   {Run.Definition.Title}":Run.FailureReason,23,passed?Teal:Amber,bold:true);
            ui.Text(new Rect(r.x+28,r.y+199,764,72),$"Süre {Run.Elapsed:0} sn   ·   Temas {Run.Contacts}   ·   Sürat ihlali {Run.SpeedViolations}\nDoğru manevra işareti {Signals.Complied}   ·   Yanlış sinyal seçimi {Signals.WrongSignals}",18,Ink);
            ui.Text(new Rect(r.x+28,r.y+294,764,65),Progress.SaveFailed?"İlerleme diske kaydedilemedi. Cihaz depolamasını kontrol edin.":passed?$"{ship.Definition.Name} ilerlemesi: {Progress.CompletedCount}/{DockingMissionCatalog.Count}. {(Progress.AllScenariosUnlocked?"Test sürümü: bütün görevleri istediğin sırada oynayabilirsin.":Index<DockingMissionCatalog.Count-1?"Sıradaki görev açıldı; eski görevleri yeniden oynayabilirsin.":"Bütün görevleri tekrar oynayabilirsin.")}":"Önceki tamamladığın görevler ve yıldızların korunur. Bu görev açık kalır.",17,Muted);
            if(GUI.Button(new Rect(r.x+28,r.y+412,226,44),"TEKRAR DENE",ui.Button)){StartMission(Index);GUIUtility.ExitGUI();}
            if(GUI.Button(new Rect(r.x+272,r.y+412,226,44),"GÖREVLER",ui.Button)){session.GetComponent<ModeSelectionMenu>().ShowMissionOptions();session.OpenMenu();}
            bool enabled=GUI.enabled;GUI.enabled=passed&&Index<DockingMissionCatalog.Count-1&&Progress.IsUnlocked(Index+1);
            if(GUI.Button(new Rect(r.x+516,r.y+412,276,44),"SONRAKİ GÖREV  →",ui.Primary)){StartMission(Index+1);GUIUtility.ExitGUI();}GUI.enabled=enabled;
        }
        private void OnDestroy(){if(audioSystem!=null)audioSystem.HornCompleted-=OnWhistle;ui?.Dispose();}
    }
}
