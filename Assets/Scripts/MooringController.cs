using UnityEngine;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    public enum MooringLinePhase{Ready,Throwing,Receiving,Hauling,Secured}
    /// <summary>Shore-to-ship heaving line, received hawser and two independent force-at-position springs.</summary>
    public sealed class MooringController:MonoBehaviour
    {
        private sealed class Station
        {
            public MooringLinePhase Phase;public float Clock,CapturedLength,Rest,Delivered;
            public LineRenderer Heaving,Hawser,Tail,Coil;public Transform Ball,Bollard,Cleat;
            public readonly HarborCrewMember[] Shore=new HarborCrewMember[2],Deck=new HarborCrewMember[2];
        }
        private readonly Station[] stations={new(),new()};private GameSessionController session;private ShipController ship;private Rigidbody body;
        private GameObject shoreRoot,deckRoot;private EnvironmentResources resources;private MaritimeUiTheme ui;
        private float hold;private string notice="";private float noticeUntil;
        public bool Available{get;private set;}public bool Complete{get;private set;}public HarborBerth Berth{get;private set;}
        public bool Engaged=>stations[0].Phase!=MooringLinePhase.Ready||stations[1].Phase!=MooringLinePhase.Ready;
        public bool BothSecured=>stations[0].Phase==MooringLinePhase.Secured&&stations[1].Phase==MooringLinePhase.Secured;
        public int Side=>Berth==HarborBerth.East?1:-1;
        public Vector3 Target=>MooringRules.Destination(ship.Definition,Berth);
        public float HoldSeconds=>hold;
        public float SwayKnots=>Vector3.Dot(body.linearVelocity,ship.RightOnWater)*4*1.9438445f;
        public MooringLinePhase Phase(int i)=>stations[i].Phase;
        public float Delivered(int i)=>stations[i].Delivered;
        public Vector3 Fairlead(int i)=>ship.transform.TransformPoint(MooringRules.Fairlead(ship.Definition,i,Side));
        public Vector3 Anchor(int i)=>MooringRules.ShoreAnchor(ship.Definition,Berth,i);
        public void Initialize(GameSessionController owner,ShipController controller){session=owner;ship=controller;body=ship.GetComponent<Rigidbody>();}
        public void BeginMode(SimulationMode mode)
        {
            Cleanup();Available=mode==SimulationMode.Harbor;Berth=HarborBerth.East;Complete=false;hold=0;notice="";
            foreach(var s in stations){s.Phase=MooringLinePhase.Ready;s.Clock=s.Delivered=0;}
            if(!Available)return;
            shoreRoot=new GameObject("Harbour mooring teams and lines");shoreRoot.transform.SetParent(transform,false);resources=shoreRoot.AddComponent<EnvironmentResources>();
            deckRoot=new GameObject("Active ship mooring teams");deckRoot.transform.SetParent(ship.transform.Find("Visuals")??ship.transform,false);
            Material[] palette=HarborShoreLife.Palette(resources,true);Material rope=resources.Own(new Material(Shader.Find("Standard")){name="Mooring hawser fibre",color=new Color(.66f,.52f,.31f)});
            Material heaving=resources.Own(new Material(Shader.Find("Standard")){color=new Color(.88f,.72f,.36f)});
            for(int i=0;i<2;i++)
            {
                Station s=stations[i];s.Heaving=Line("Light heaving line",.008f,heaving,25);s.Hawser=Line("Load-bearing mooring hawser",ship.Definition.UsesPods?.026f:.016f,rope,25);s.Tail=Line("Deck working end through all four hands",.014f,rope,5);s.Coil=Line("Flaked shore working line",.012f,rope,81);
                s.Ball=Primitive("Heaving line soft monkey fist",PrimitiveType.Sphere,shoreRoot.transform,new(.046f,.046f,.046f),heaving);
                s.Bollard=Primitive("Dedicated mooring bollard",PrimitiveType.Cylinder,shoreRoot.transform,new(.13f,.13f,.13f),palette[1]);
                var cross=Primitive("Bollard crossbar",PrimitiveType.Cube,s.Bollard,new(2.5f,.25f,.5f),palette[1]);cross.localPosition=new Vector3(0,.8f,0);
                s.Cleat=Primitive("Ship mooring bitt",PrimitiveType.Cylinder,deckRoot.transform,new(.11f,.09f,.11f),palette[1]);
                var cleatBar=Primitive("Ship bitt crossbar",PrimitiveType.Cube,s.Cleat,new(2.4f,.25f,.5f),palette[1]);cleatBar.localPosition=new Vector3(0,.8f,0);
                for(int j=0;j<2;j++)
                {
                    var shore=new GameObject((i==0?"Bow":"Stern")+" shore line handler "+j);shore.transform.SetParent(shoreRoot.transform,false);
                    s.Shore[j]=shore.AddComponent<HarborCrewMember>();s.Shore[j].Build(palette,true,false,i*1.7f+j*.6f,resources);
                    var deck=new GameObject((i==0?"Bow":"Stern")+" receiving and hauling sailor "+j);deck.transform.SetParent(deckRoot.transform,false);
                    s.Deck[j]=deck.AddComponent<HarborCrewMember>();s.Deck[j].Build(palette,true,false,i*1.5f+j*.6f,resources);
                }
            }
            UpdateVisuals();
        }
        private void Cleanup()
        {
            if(deckRoot!=null){deckRoot.SetActive(false);Destroy(deckRoot);deckRoot=null;}
            if(shoreRoot!=null){shoreRoot.SetActive(false);Destroy(shoreRoot);shoreRoot=null;}
        }
        private HarborBerth DesiredBerth()
        {
            if(session.Missions!=null&&session.Missions.Assigned)return session.Missions.Run.Berth;
            if(session.Tugs!=null&&session.Tugs.Available&&TugAssistRules.Valid(session.Tugs.Berth))return session.Tugs.Berth;
            if(Engaged)return Berth;
            return (ship.transform.position-MooringRules.Destination(ship.Definition,HarborBerth.East)).sqrMagnitude<
                (ship.transform.position-MooringRules.Destination(ship.Definition,HarborBerth.West)).sqrMagnitude?HarborBerth.East:HarborBerth.West;
        }
        public void RefreshBerth()
        {if(!Available)return;HarborBerth desired=DesiredBerth();if(desired!=Berth){Release(false);Berth=desired;}}
        private bool EnginesStopped()
        {for(int i=0;i<ship.EngineCount;i++)if(Mathf.Abs(ship.GetEngineThrottle(i))>.001f||Mathf.Abs(ship.GetEngineSignedLoad(i))>.08f)return false;return true;}
        private bool MissionReady=>session.Missions==null||!session.Missions.Assigned||session.Missions.Running&&session.Missions.Run.GatesPassed==3;
        public string HoldReason(int index)
        {
            if(!Available||!session.CanControl)return "Manevra bekliyor";
            if(!MissionReady)return "Önce üç giriş kapısını tamamla";
            if(session.Missions!=null&&session.Missions.Assigned&&!session.Missions.Run.ApproachWorkReady)return session.Missions.Run.ExerciseHint;
            if(!EnginesStopped())return "Tüm makineleri STOP'a getir";
            if(Mathf.Abs(Mathf.DeltaAngle(ship.Heading,0))>=12)return "Rıhtıma paralel ol · baş 000° ±12°";
            if(ship.SpeedKnots>=.6f||Mathf.Abs(SwayKnots)>=.35f)return "Sürati 0,60 kn altına düşür";
            if(!MooringRules.CastEnvelope(Fairlead(index),Anchor(index),ship.Heading,ship.SpeedKnots,SwayKnots))return "Halat mesafesine yaklaş · en fazla 24 m";
            var other=stations[1-index].Phase;if(other!=MooringLinePhase.Ready&&other!=MooringLinePhase.Secured)return "İlk halatın alınmasını ve bağlanmasını bekle";
            return null;
        }
        public bool Cast(int index)
        {
            if(index<0||index>1)return false;RefreshBerth();Station s=stations[index];if(s.Phase!=MooringLinePhase.Ready||HoldReason(index)!=null)return false;
            s.Phase=MooringLinePhase.Throwing;s.Clock=0;Complete=false;hold=0;SetNotice((index==0?"Baş":"Kıç")+" el incesi sahilden atılıyor. Gemi ekibi karşılıyor.");return true;
        }
        public void Release(bool notify=true)
        {
            foreach(var s in stations){s.Phase=MooringLinePhase.Ready;s.Clock=s.Delivered=0;}
            hold=0;Complete=false;session?.Missions?.Run?.SetMooringComplete(false);
            if(notify)SetNotice("Halatlar fora edildi. Sürati düşürüp yeniden alabilirsin.");
        }
        private void SetNotice(string message){notice=message;noticeUntil=Time.time+5;}
        private void Update(){if(!Available)return;RefreshBerth();}
        private void FixedUpdate()=>SimulateStep(Time.fixedDeltaTime);
        public void SimulateStep(float dt)
        {
            if(!Available||!float.IsFinite(dt)||dt<=0)return;foreach(var s in stations)s.Delivered=0;
            if(session==null||!session.CanControl)return;
            bool safe=EnginesStopped();
            for(int i=0;i<2;i++)
            {
                Station s=stations[i];if(s.Phase==MooringLinePhase.Ready)continue;
                Vector3 point=Fairlead(i),anchor=Anchor(i),flat=anchor-point;flat.y=0;
                if(flat.magnitude>9||ship.SpeedKnots>2.5f){Release();SetNotice("Halat emniyet sınırı aşıldı; ekip bıraktı. Yeniden yaklaş.");return;}
                if(!safe)continue;
                s.Clock+=dt;
                if(s.Phase==MooringLinePhase.Throwing&&s.Clock>=MooringRules.CastSeconds){s.Phase=MooringLinePhase.Receiving;s.Clock=0;SetNotice("El incesi alındı. Ana halat güverteye çekiliyor.");}
                else if(s.Phase==MooringLinePhase.Receiving&&s.Clock>=MooringRules.ReceiveSeconds){s.Phase=MooringLinePhase.Hauling;s.Clock=0;s.CapturedLength=flat.magnitude;SetNotice("Ana halat alındı. Irgat ve ekip boşunu alıyor.");}
                else if(s.Phase==MooringLinePhase.Hauling&&s.Clock>=MooringRules.HaulSeconds){s.Phase=MooringLinePhase.Secured;s.Clock=0;SetNotice((i==0?"Baş":"Kıç")+" halatı babaya volta edildi. "+(BothSecured?"İki uç bağlı.":"Diğer uçtan da halat al."));}
                if(s.Phase!=MooringLinePhase.Hauling&&s.Phase!=MooringLinePhase.Secured)continue;
                Vector3 local=MooringRules.Fairlead(ship.Definition,i,Side),desired=Target+local,desiredGap=anchor-desired;desiredGap.y=0;
                float u=s.Phase==MooringLinePhase.Secured?1:Mathf.Clamp01(s.Clock/MooringRules.HaulSeconds);
                s.Rest=Mathf.Lerp(Mathf.Max(s.CapturedLength,desiredGap.magnitude),desiredGap.magnitude,u);
                Vector3 forcePoint=body.position+ship.RightOnWater*local.x+ship.ForwardOnWater*local.z;forcePoint.y=body.worldCenterOfMass.y;
                Vector3 acceleration=MooringRules.PullAcceleration(point,anchor,body.GetPointVelocity(forcePoint),s.Rest);
                s.Delivered=acceleration.magnitude;body.AddForceAtPosition(acceleration*body.mass,forcePoint,ForceMode.Force);
            }
            bool settled=BothSecured&&safe&&MooringRules.Settled(body.position,ship.Heading,ship.SpeedKnots,SwayKnots,ship.Definition,Berth);
            hold=settled?Mathf.Min(3,hold+dt):0;Complete=hold>=3;
            session.Missions?.Run?.SetMooringComplete(Complete);
            if(Complete)session.Tugs?.StopAll();
        }
        public static Rect PanelRect(HelmLayout l,bool tugControls=false)=>new(316,152,Mathf.Min(620,l.Width-658),tugControls?166:124);
        private bool HasTugControls=>session?.Tugs!=null&&session.Tugs.Available&&session.Tugs.Count==2;
        public bool PanelVisible=>Available&&session!=null&&!session.MenuOpen&&session.HasStarted&&(session.Missions==null||!session.Missions.BlocksControl&&(!session.Missions.Assigned||session.Missions.Run.ApproachWorkReady))&&
            (Engaged||Vector3.Distance(new Vector3(body.position.x,0,body.position.z),new Vector3(Target.x,0,Target.z))<24);
        public bool BlocksCamera(Vector2 screen)=>PanelVisible&&PanelRect(HelmLayout.Current,HasTugControls).Contains(HelmLayout.Current.FromScreen(screen));
        private void OnGUI()
        {
            if(!PanelVisible)return;ui??=new MaritimeUiTheme();var l=HelmLayout.Current;var previous=GUI.matrix;GUI.matrix=Matrix4x4.TRS(l.Offset,Quaternion.identity,Vector3.one*l.Scale);GUI.depth=-24;
            try
            {
                Rect p=PanelRect(l,HasTugControls);ui.Surface(p);ui.Text(new(p.x+12,p.y+4,p.width-24,21),Complete?"HALATLAR BAĞLI · YANAŞMA TAMAMLANDI":"HALAT MANEVRASI  /  "+TugAssistRules.Name(Berth),13,Complete?Teal:Ink,bold:true);
                float w=(p.width-115)*.5f;
                for(int i=0;i<2;i++)
                {
                    var s=stations[i];bool old=GUI.enabled;GUI.enabled=s.Phase==MooringLinePhase.Ready&&HoldReason(i)==null;
                    string label=s.Phase switch{MooringLinePhase.Throwing=>"ATILIYOR",MooringLinePhase.Receiving=>"ALINIYOR",MooringLinePhase.Hauling=>"ÇEKİLİYOR",MooringLinePhase.Secured=>"BAĞLI",_=>"HALAT AL"};
                    if(GUI.Button(new Rect(p.x+12+i*(w+8),p.y+29,w,32),(i==0?"BAŞ · ":"KIÇ · ")+label,s.Phase==MooringLinePhase.Ready?ui.Button:ui.Selected))Cast(i);GUI.enabled=old;
                }
                bool enabled=GUI.enabled;GUI.enabled=Engaged;if(GUI.Button(new Rect(p.xMax-82,p.y+29,70,32),"FORA",ui.Danger))Release();GUI.enabled=enabled;
                string status=Time.time<noticeUntil?notice:!EnginesStopped()?"Çekme için bütün makineler STOP olmalı.":BothSecured?$"İki uç bağlı. Sakinleşme {hold:0.0}/3 sn.":HoldReason(stations[0].Phase==MooringLinePhase.Secured?1:0)??(Engaged?"Diğer uçtan da halat alıp baş ve kıçı birlikte bağla.":"Önce baş veya kıç halatını al, sonra diğer ucu bağla.");
                ui.Text(new Rect(p.x+12,p.y+67,p.width-24,27),status,12,Muted);
                var run=session.Missions?.Run;
                string detail=run!=null&&run.Signals.Expected!=WhistleSignal.None?$"ZORUNLU: {ManeuverSignalMonitor.Meaning(run.Signals.Expected)} · {run.Signals.SecondsLeft(run.Elapsed):0} sn":$"YAN {SwayKnots:+0.00;-0.00;0.00} kn · BAŞ {Mathf.Abs(Mathf.DeltaAngle(ship.Heading,0)):0}°"+(run!=null?$" · GÖREV SABİT {run.HoldSeconds:0.0}/5 sn":" · 2 BAĞIMSIZ HALAT");
                ui.Text(new Rect(p.x+12,p.y+96,p.width-24,22),detail,12,run!=null&&run.Signals.Expected!=WhistleSignal.None?Amber:Muted);
                if(HasTugControls)session.Tugs.DrawMooringControls(new Rect(p.x+12,p.y+126,p.width-24,30));
            }
            finally{GUI.matrix=previous;}
        }
        private void LateUpdate(){if(Available)UpdateVisuals();}
        private void UpdateVisuals()
        {
            if(shoreRoot==null||deckRoot==null)return;float now=Time.time;
            bool near=(ship.transform.position-Target).sqrMagnitude<32*32;deckRoot.SetActive(near||Engaged);
            for(int i=0;i<2;i++)
            {
                Station s=stations[i];Vector3 local=MooringRules.Fairlead(ship.Definition,i,Side),anchor=Anchor(i);s.Bollard.position=anchor-Vector3.up*.02f;s.Cleat.localPosition=local-Vector3.up*.09f;
                for(int k=0;k<81;k++){float u=k/80f,angle=u*Mathf.PI*8,r=Mathf.Lerp(.07f,.22f,u);s.Coil.SetPosition(k,new Vector3(anchor.x+Side*.65f+Mathf.Cos(angle)*r,MooringRules.PierY(Berth)+.012f,anchor.z-.75f+Mathf.Sin(angle)*r*1.5f));}
                float progress=s.Phase==MooringLinePhase.Throwing?s.Clock/MooringRules.CastSeconds:s.Phase==MooringLinePhase.Receiving?s.Clock/MooringRules.ReceiveSeconds:0;
                for(int j=0;j<2;j++)
                {
                    var shore=s.Shore[j];shore.transform.position=new Vector3(anchor.x+Side*.42f,MooringRules.PierY(Berth),anchor.z+(j==0?-.5f:.55f));shore.transform.rotation=Quaternion.LookRotation(Vector3.left*Side);
                    var deck=s.Deck[j];deck.transform.localPosition=local+new Vector3(-Side*(.23f+j*.16f),-.18f,(j==0?-.33f:.33f));deck.transform.localRotation=Quaternion.LookRotation(Vector3.right*Side);
                    HarborCrewPose pose=s.Phase switch{MooringLinePhase.Throwing=>HarborCrewPose.Throw,MooringLinePhase.Receiving=>HarborCrewPose.Receive,MooringLinePhase.Hauling=>HarborCrewPose.Haul,MooringLinePhase.Secured=>HarborCrewPose.Secure,_=>HarborCrewPose.Watch};
                    shore.SamplePose(now,s.Phase==MooringLinePhase.Throwing?pose:s.Phase==MooringLinePhase.Hauling?HarborCrewPose.Secure:HarborCrewPose.Watch,progress);
                    deck.SamplePose(now,s.Phase==MooringLinePhase.Throwing?HarborCrewPose.Receive:pose,progress);
                }
                bool light=s.Phase==MooringLinePhase.Throwing||s.Phase==MooringLinePhase.Receiving;
                s.Heaving.enabled=light;s.Ball.gameObject.SetActive(s.Phase==MooringLinePhase.Throwing);
                if(light)
                {
                    Vector3 from=s.Shore[0].RightHand,to=(s.Deck[0].LeftHand+s.Deck[0].RightHand)*.5f;float u=s.Phase==MooringLinePhase.Throwing?Mathf.Clamp01(progress):1;
                    Vector3 tip=Vector3.Lerp(from,to,u)+Vector3.up*(Mathf.Sin(u*Mathf.PI)*1.0f);s.Ball.position=tip;
                    Curve(s.Heaving,from,tip,.025f);
                }
                bool main=s.Phase==MooringLinePhase.Hauling||s.Phase==MooringLinePhase.Secured;s.Hawser.enabled=main||s.Phase==MooringLinePhase.Receiving;s.Tail.enabled=main;
                if(s.Phase==MooringLinePhase.Receiving)Curve(s.Hawser,anchor,Vector3.Lerp(anchor,Fairlead(i),Mathf.Clamp01(progress)),.04f);
                if(main)
                {
                    Vector3 point=Fairlead(i);Vector3 gap=anchor-point;gap.y=0;
                    float slack=Mathf.Max(0,s.Rest-gap.magnitude);Curve(s.Hawser,anchor,point,Mathf.Min(.33f,.025f+slack*.22f));
                    s.Tail.SetPosition(0,point);s.Tail.SetPosition(1,s.Deck[0].LeftHand);s.Tail.SetPosition(2,s.Deck[0].RightHand);s.Tail.SetPosition(3,s.Deck[1].LeftHand);s.Tail.SetPosition(4,s.Deck[1].RightHand);
                }
            }
        }
        private LineRenderer Line(string name,float width,Material paint,int count)
        {var go=new GameObject(name);go.transform.SetParent(shoreRoot.transform,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;line.positionCount=count;line.widthMultiplier=width;line.sharedMaterial=paint;line.generateLightingData=true;line.numCapVertices=3;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return line;}
        private static void Curve(LineRenderer line,Vector3 from,Vector3 to,float sag)
        {int count=line.positionCount;for(int k=0;k<count;k++){float u=k/(float)(count-1);line.SetPosition(k,Vector3.Lerp(from,to,u)-Vector3.up*(4*u*(1-u)*sag));}}
        private static Transform Primitive(string name,PrimitiveType type,Transform parent,Vector3 scale,Material material)
        {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);return go.transform;}
        private void OnDestroy(){Cleanup();ui?.Dispose();}
    }
}
