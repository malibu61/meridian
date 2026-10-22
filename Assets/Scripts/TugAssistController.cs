using UnityEngine;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    /// <summary>Two independently commanded pushing tugs. Ship displacement always comes from Rigidbody forces.</summary>
    public sealed class TugAssistController:MonoBehaviour
    {
        private sealed class Tug
        {
            public Transform Root;
            public bool Commanded;
            public float Pressure;
            public bool Contact;
            public float Delivered;
            public int Side,FromSide,RouteStep=-1;
        }
        private static TugAssistController active;
        private GameSessionController session;
        private ShipController ship;
        private Rigidbody body;
        private Camera view;
        private GameObject fleetRoot;
        private readonly Tug[] tugs={new(),new()};
        private MaritimeUiTheme ui;
        private LineRenderer guide;
        public bool Available{get;private set;}
        public bool IntentOpen{get;private set;}
        public bool Berthed{get;private set;}
        public HarborBerth Berth{get;private set;}
        public int Count=>fleetRoot==null?0:2;
        public int PushSign=>TugAssistRules.Valid(Berth)&&ship!=null?TugAssistRules.PushSign(ship.RightOnWater,Berth):1;
        public bool Commanded(int index)=>index>=0&&index<2&&tugs[index].Commanded;
        public float Pressure(int index)=>index>=0&&index<2?tugs[index].Pressure:0;
        public float Delivered(int index)=>index>=0&&index<2?tugs[index].Delivered:0;
        public float SwayKnots=>body==null?0:Vector3.Dot(body.linearVelocity,ship.RightOnWater)*VesselCatalog.ModelMetresPerUnit*1.9438445f;
        public Vector3 TugPosition(int index)=>tugs[index].Root.position;
        public bool InContact(int index)=>tugs[index].Contact;
        public void Initialize(GameSessionController owner,ShipController controller,Camera camera)
        {session=owner;ship=controller;body=ship.GetComponent<Rigidbody>();view=camera;active=this;}

        public void BeginMode(SimulationMode mode,VesselId vessel)
        {
            active=this;
            if(fleetRoot!=null){fleetRoot.SetActive(false);Destroy(fleetRoot);fleetRoot=null;}
            StopAll();Available=TugAssistRules.Available(mode,vessel);IntentOpen=Available;Berthed=false;Berth=HarborBerth.None;guide=null;
            foreach(Tug tug in tugs){tug.Root=null;tug.Pressure=tug.Delivered=0;tug.Contact=false;tug.RouteStep=-1;}
            // No tug renderers, bodies, GUI, or forces exist in other ships/modes.
        }
        public bool SelectBerth(HarborBerth berth)
        {
            if(!Available||!TugAssistRules.Valid(berth))return false;
            Berth=berth;IntentOpen=false;Berthed=false;StopAll();
            if(fleetRoot==null)
            {
                fleetRoot=new GameObject("Anadolu harbour tug assistance");fleetRoot.transform.SetParent(transform,false);
                for(int i=0;i<2;i++)
                {
                    var go=new GameObject(i==0?"Bow harbour tug":"Stern harbour tug");go.transform.SetParent(fleetRoot.transform,false);
                    tugs[i].Root=go.transform;new HarborTugModel(go.transform).Build();
                    tugs[i].Side=-PushSign;
                    Vector3 local=TugAssistRules.LocalStation(i,PushSign);local.x-=PushSign*6;
                    Vector3 p=ship.transform.position+ship.RightOnWater*local.x+ship.ForwardOnWater*local.z;
                    p.y=.1f+WaterWaves.Height(p.x,p.z,Time.time);go.transform.SetPositionAndRotation(p,Quaternion.LookRotation(ship.RightOnWater*PushSign));
                }
                CreateGuide();
            }
            return true;
        }
        public bool Toggle(int index)
        {
            if(!Available||IntentOpen||Berthed||index<0||index>=2||session==null||!session.CanControl)return false;
            tugs[index].Commanded=!tugs[index].Commanded;return true;
        }
        public void StopAll(){foreach(Tug tug in tugs)tug.Commanded=false;}
        public void RequestIntent(){if(!Available)return;StopAll();IntentOpen=true;Berthed=false;}
        private string HoldReason()
        {
            if(!TugAssistRules.InArea(ship.transform.position,Berth))return "Liman yaklaşma sahasına dön";
            if(!TugAssistRules.Aligned(ship.Heading))return "Rıhtıma paralel ol · baş 000° ±30°";
            if(ship.SpeedKnots>=TugAssistRules.MaximumAssistKnots)return "Sürati 5 kn altına düşür";
            return null;
        }
        private void FixedUpdate()=>SimulateStep(Time.fixedDeltaTime,Time.time);
        public void SimulateStep(float dt,float time)
        {
            if(!Available||fleetRoot==null||ship==null||!float.IsFinite(dt)||dt<=0)return;
            bool controlled=session!=null&&session.CanControl&&!IntentOpen;
            if(controlled&&TugAssistRules.AtBerth(body.position,ship.Heading,ship.SpeedKnots,Berth)){Berthed=true;StopAll();}
            int sign=PushSign;Vector3 right=ship.RightOnWater,forward=ship.ForwardOnWater;
            float remaining=Vector3.Dot(TugAssistRules.Destination(Berth)-body.position,TugAssistRules.ShoreDirection(Berth));
            bool permitted=controlled&&!Berthed&&HoldReason()==null&&remaining>0;
            for(int i=0;i<2;i++)
            {
                Tug tug=tugs[i];Vector3 station=TugAssistRules.LocalStation(i,sign);
                if(tug.Side!=-sign){tug.FromSide=tug.Side;tug.Side=-sign;tug.RouteStep=0;}
                // When released, remain visibly clear of the hull. Never parent a tug to the player ship.
                float clearance=tug.Commanded?0:2f;
                Vector3 goal=body.position+right*(station.x-sign*clearance)+forward*station.z;
                bool routing=tug.RouteStep>=0;
                if(routing)
                {
                    // A side change goes around the bow/stern, never through the ship.
                    float routeZ=(i==0?1:-1)*(ship.Definition.ColliderSize.z*.5f+6);
                    float routeX=(tug.RouteStep==0?tug.FromSide:tug.Side)*11;
                    goal=body.position+right*routeX+forward*routeZ;
                    Vector3 gap=goal-tug.Root.position;gap.y=0;
                    if(gap.magnitude<.5f)tug.RouteStep=tug.RouteStep==0?1:-1;
                }
                goal.y=.1f+WaterWaves.Height(goal.x,goal.z,time);
                Vector3 p=tug.Root.position;
                if(controlled)p=Vector3.MoveTowards(p,goal,dt*1.2f);
                Vector3 planar=goal-p;planar.y=0;
                Quaternion facing=Quaternion.LookRotation(right*sign,Vector3.up);
                float roll=Mathf.Clamp(Vector3.Dot(WaterWaves.Normal(p.x,p.z,time),right)*-24,-12,12);
                if(controlled)tug.Root.SetPositionAndRotation(p,Quaternion.Slerp(tug.Root.rotation,facing*Quaternion.Euler(0,0,roll),1-Mathf.Exp(-dt*3)));
                tug.Contact=!routing&&planar.magnitude<TugAssistRules.ContactTolerance&&Mathf.Abs(p.y-goal.y)<.45f&&Quaternion.Angle(tug.Root.rotation,facing)<18;
                bool pushing=permitted&&tug.Commanded&&tug.Contact;
                tug.Pressure=Mathf.MoveTowards(tug.Pressure,pushing?1:0,dt*(pushing?.4f:1.2f));
                tug.Delivered=0;
                if(!pushing)continue; // Never transmit a force across an open gap or during a menu/modal.
                float force=TugAssistRules.ForceMagnitude(body.mass,tug.Pressure,ship.SpeedKnots,remaining);
                tug.Delivered=force/(body.mass*.022f);
                Vector3 contact=body.position+forward*station.z-right*sign*(Mathf.Abs(station.x)-TugAssistRules.TugLength*.5f-.12f);
                contact.y=body.worldCenterOfMass.y;
                body.AddForceAtPosition(right*sign*force,contact,ForceMode.Force);
            }
        }
        private void LateUpdate()
        {
            if(guide==null)return;Vector3 c=TugAssistRules.Destination(Berth);
            Vector3[] points={c+new Vector3(-4.2f,0,-29.2f),c+new Vector3(-4.2f,0,29.2f),c+new Vector3(4.2f,0,29.2f),c+new Vector3(4.2f,0,-29.2f),c+new Vector3(-4.2f,0,-29.2f)};
            for(int i=0;i<points.Length;i++)points[i].y=WaterWaves.SurfaceY(points[i],Time.time)+.08f;
            guide.SetPositions(points);
        }
        private void CreateGuide()
        {
            var go=new GameObject("Selected berth guide");go.transform.SetParent(fleetRoot.transform,false);guide=go.AddComponent<LineRenderer>();
            guide.positionCount=5;guide.useWorldSpace=true;guide.widthMultiplier=.07f;guide.startColor=guide.endColor=Teal;
            var owner=go.AddComponent<EnvironmentResources>();guide.sharedMaterial=owner.Own(new Material(Shader.Find("Sprites/Default")));guide.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        public static Rect PanelRect(HelmLayout layout)=>new(316,158,Mathf.Min(620,layout.Width-658),112);
        public static bool BlocksCamera(Vector2 screen)
        {
            var a=active;if(a==null||!a.Available||a.session==null||a.session.MenuOpen)return false;
            if(a.IntentOpen)return true;
            HelmLayout layout=HelmLayout.Current;if(PanelRect(layout).Contains(layout.FromScreen(screen)))return true;
            for(int i=0;i<2;i++)if(a.MarkerRect(i,layout,out Rect r)&&r.Contains(layout.FromScreen(screen)))return true;
            return false;
        }
        private bool MarkerRect(int i,HelmLayout layout,out Rect rect)
        {
            rect=default;if(tugs[i].Root==null||view==null)return false;
            Vector3 point=view.WorldToScreenPoint(tugs[i].Root.position+Vector3.up*4.4f);if(point.z<=0)return false;
            Vector2 p=layout.FromScreen(point);rect=new Rect(p.x-69,p.y-20,138,40);
            if(rect.x<0||rect.xMax>layout.Width||rect.y<148||rect.yMax>layout.Height-12)return false;
            foreach(Vector2 corner in new[]{rect.min,rect.max,new Vector2(rect.xMax,rect.y),new Vector2(rect.x,rect.yMax)})if(layout.BlocksCamera(corner)||PanelRect(layout).Contains(corner))return false;
            return true;
        }
        private void OnGUI()
        {
            if(!Available||session==null||session.MenuOpen)return;
            ui??=new MaritimeUiTheme();GUI.depth=-20;HelmLayout layout=HelmLayout.Current;Matrix4x4 previous=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(layout.Offset,Quaternion.identity,Vector3.one*layout.Scale);
            try
            {
                if(IntentOpen){DrawIntent(layout);return;}
                DrawPanel(layout);
                for(int i=0;i<2;i++)if(MarkerRect(i,layout,out Rect marker))
                {
                    DrawForceArrow(i,layout);
                    if(GUI.Button(marker,(i==0?"BAŞ":"KIÇ")+(tugs[i].Commanded?" · DURDUR":" · İT"),tugs[i].Commanded?ui.Selected:ui.Button))Toggle(i);
                }
            }
            finally{GUI.matrix=previous;}
        }
        private void DrawIntent(HelmLayout layout)
        {
            GUI.depth=-40;Fill(new Rect(-layout.Offset.x/layout.Scale,-layout.Offset.y/layout.Scale,Screen.width/layout.Scale,Screen.height/layout.Scale),new Color(.015f,.025f,.04f,.88f));
            Rect card=new((layout.Width-740)*.5f,(layout.Height-402)*.5f,740,402);ui.Surface(card);
            ui.Text(new Rect(card.x+28,card.y+22,684,24),"LİMAN KONTROL  /  TCG ANADOLU",14,Teal,bold:true);
            ui.Text(new Rect(card.x+28,card.y+62,684,48),"Nereye yanaşmak istiyorsun?",30,Ink,bold:true);
            ui.Text(new Rect(card.x+28,card.y+115,684,50),"Hedef rıhtımını seç. Baş ve kıç römorkörlerine ayrı ayrı komut verebilirsin.",17,Muted);
            foreach(HarborBerth berth in new[]{HarborBerth.West,HarborBerth.East})
            {
                Rect choice=new(card.x+28+(berth==HarborBerth.East?350:0),card.y+185,334,96);
                string label=berth==HarborBerth.East?"DOĞU RIHTIMI\nSancak aborda  →":"BATI RIHTIMI\n←  İskele aborda";
                if(GUI.Button(choice,label,Berth==berth?ui.Selected:ui.Button)){SelectBerth(berth);GUIUtility.hotControl=0;}
            }
            ui.Text(new Rect(card.x+28,card.y+297,684,44),"Turkuaz hedefe paralel ilerle, sürati düşür. İtme 5 kn altında; tam destek 2 kn ve altında.",15,Muted);
            if(GUI.Button(new Rect(card.x+28,card.y+351,130,30),"ANA MENÜ",ui.Button))session.OpenMenu();
            ui.Text(new Rect(card.x+180,card.y+350,532,30),"İT → temasa yaklaşır   •   tekrar tıkla → bırakır",14,Teal,TextAnchor.MiddleRight);
        }
        private void DrawPanel(HelmLayout layout)
        {
            Rect p=PanelRect(layout);ui.Surface(p);
            string hint=Berthed?"YANAŞMA TAMAMLANDI":HoldReason()??(PushSign>0?"İskele tarafından sancağa itme →":"← Sancak tarafından iskeleye itme");
            ui.Text(new Rect(p.x+12,p.y+7,p.width-125,20),TugAssistRules.Name(Berth)+"  /  RÖMORKÖR",12,Teal,bold:true);
            if(GUI.Button(new Rect(p.xMax-107,p.y+7,95,26),"HEDEF",ui.Button))RequestIntent();
            float w=(p.width-126)*.5f;
            for(int i=0;i<2;i++)
            {
                Tug tug=tugs[i];string state=!tug.Commanded?"İT":tug.Delivered>.001f?$"İTİYOR %{tug.Delivered*100:0}":tug.Contact?"TEMASTA · BEKLİYOR":"YAKLAŞIYOR";
                string label=(i==0?"BAŞ":"KIÇ")+"  "+state;
                GUI.enabled=!Berthed;
                if(GUI.Button(new Rect(p.x+12+i*(w+8),p.y+39,w,36),label,tug.Commanded?ui.Selected:ui.Button))Toggle(i);
            }
            GUI.enabled=true;
            if(GUI.Button(new Rect(p.xMax-94,p.y+39,82,36),"İKİSİ DUR",ui.Danger))StopAll();
            float metres=Vector3.Distance(new Vector3(ship.transform.position.x,0,ship.transform.position.z),new Vector3(TugAssistRules.Destination(Berth).x,0,TugAssistRules.Destination(Berth).z))*VesselCatalog.ModelMetresPerUnit;
            string sway=Mathf.Abs(SwayKnots)<.005f?"YAN 0,00 kn":$"YAN {(SwayKnots>0?"→":"←")} {Mathf.Abs(SwayKnots):0.00} kn";
            ui.Text(new Rect(p.x+12,p.y+81,p.width-24,23),hint+$"  ·  {sway}  ·  {metres:0} m",13,Berthed?Teal:Muted);
        }
        private void DrawForceArrow(int index,HelmLayout layout)
        {
            // A force arrow means a force was actually delivered, not merely a queued command.
            if(view==null||tugs[index].Delivered<=.001f)return;
            Vector3 p=tugs[index].Root.position+Vector3.up*.9f;
            Vector3 a=view.WorldToScreenPoint(p),b=view.WorldToScreenPoint(p+ship.RightOnWater*PushSign*3.5f);
            if(a.z<=0||b.z<=0)return;Vector2 from=layout.FromScreen(a),to=layout.FromScreen(b),d=(to-from).normalized;
            Color color=Teal;Line(from,to,3,color);Vector2 n=new(-d.y,d.x);Line(to,to-d*12+n*6,3,color);Line(to,to-d*12-n*6,3,color);
        }
        private void OnDestroy(){if(active==this)active=null;ui?.Dispose();}
    }
}
