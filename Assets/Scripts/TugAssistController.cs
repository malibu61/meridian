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
            public int Side;public bool Approaching,Escaping,Routing,Waiting;
        }
        private static TugAssistController active;
        private GameSessionController session;
        private ShipController ship;
        private Rigidbody body;
        private Camera view;
        private GameObject fleetRoot;
        private readonly Tug[] tugs={new(),new()};
        private readonly HarborTugNavigation navigation=new();
        private MaritimeUiTheme ui;
        private LineRenderer guide;
        public bool Available{get;private set;}
        public bool IntentOpen{get;private set;}
        public bool Berthed{get;private set;}
        public HarborBerth Berth{get;private set;}
        public Vector3 Destination=>TugAssistRules.Destination(Berth,ship.Definition);
        public int Count=>fleetRoot==null?0:2;
        public int PushSign=>TugAssistRules.Valid(Berth)&&ship!=null?TugAssistRules.PushSign(ship.RightOnWater,Berth):1;
        public bool Commanded(int index)=>index>=0&&index<2&&tugs[index].Commanded;
        public float Pressure(int index)=>index>=0&&index<2?tugs[index].Pressure:0;
        public float Delivered(int index)=>index>=0&&index<2?tugs[index].Delivered:0;
        public float SwayKnots=>body==null?0:Vector3.Dot(body.linearVelocity,ship.RightOnWater)*VesselCatalog.ModelMetresPerUnit*1.9438445f;
        public Vector3 TugPosition(int index)=>tugs[index].Root.position;
        public Quaternion TugRotation(int index)=>tugs[index].Root.rotation;
        public bool InContact(int index)=>tugs[index].Contact;
        public bool ClearingShore(int index)=>tugs[index].Escaping;
        public bool WaitingForRoute(int index)=>tugs[index].Waiting;
        public void Initialize(GameSessionController owner,ShipController controller,Camera camera)
        {session=owner;ship=controller;body=ship.GetComponent<Rigidbody>();view=camera;active=this;}

        public void BeginMode(SimulationMode mode,VesselId vessel,bool enable=true)
        {
            active=this;
            if(fleetRoot!=null){fleetRoot.SetActive(false);Destroy(fleetRoot);fleetRoot=null;}
            StopAll();Available=enable&&TugAssistRules.Available(mode,vessel);IntentOpen=Available;Berthed=false;Berth=HarborBerth.None;guide=null;
            foreach(Tug tug in tugs){tug.Root=null;tug.Pressure=tug.Delivered=0;tug.Contact=false;tug.Approaching=tug.Escaping=tug.Routing=tug.Waiting=false;}
            // No tug renderers, bodies, GUI, or forces exist in other ships/modes.
        }
        public bool SelectBerth(HarborBerth berth)
        {
            if(!Available||!TugAssistRules.Valid(berth))return false;
            if(session.Missions!=null&&session.Missions.Assigned&&berth!=session.Missions.Run.Berth)return false;
            Berth=berth;IntentOpen=false;Berthed=false;StopAll();
            navigation.Configure(session.Missions?.Run);
            foreach(Tug tug in tugs)tug.Delivered=0;
            if(fleetRoot==null)
            {
                fleetRoot=new GameObject((ship.Definition.UsesPods?"Anadolu":"Barbaros")+" harbour tug assistance");fleetRoot.transform.SetParent(transform,false);
                for(int i=0;i<2;i++)
                {
                    var go=new GameObject(i==0?"Bow harbour tug":"Stern harbour tug");go.transform.SetParent(fleetRoot.transform,false);
                    tugs[i].Root=go.transform;new HarborTugModel(go.transform).Build();
                    tugs[i].Side=-PushSign;tugs[i].Approaching=false;
                    Vector3 local=TugAssistRules.LocalStation(i,PushSign,ship.Definition);local.x-=PushSign*6;
                    Vector3 p=ship.transform.position+ship.RightOnWater*local.x+ship.ForwardOnWater*local.z;
                    Vector2 other=i==0?new Vector2(10000,10000):HarborTugNavigation.Flat(tugs[0].Root.position);
                    p=HarborTugNavigation.World(navigation.SafeSpawn(HarborTugNavigation.Flat(p),body.position,ship.Heading,ship.Definition,other));
                    p.y=.1f+WaterWaves.Height(p.x,p.z,Time.time);go.transform.SetPositionAndRotation(p,Quaternion.LookRotation(ship.ForwardOnWater));
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
        public void RequestIntent(){if(!Available||session.Missions!=null&&session.Missions.Assigned)return;StopAll();IntentOpen=true;Berthed=false;}
        private string HoldReason()
        {
            if(!TugAssistRules.InArea(ship.transform.position,Berth))return "Liman yaklaşma sahasına dön";
            if(!TugAssistRules.Aligned(ship.Heading))return "Rıhtıma paralel ol · baş 000° ±30°";
            if(ship.SpeedKnots>=TugAssistRules.MaximumAssistKnots)return "Sürati 5 kn altına düşür";
            return null;
        }
        private void FixedUpdate()=>SimulateStep(Time.fixedDeltaTime,Time.time);
        private Vector3 Formation(int index,int side,float clearance,Vector3 position,float heading)
        {
            Vector3 station=TugAssistRules.LocalStation(index,-side,ship.Definition);station.x+=side*clearance;
            return position+Quaternion.Euler(0,heading,0)*station;
        }
        private bool SideClear(int index,int side,float margin=0)
        {
            Vector3 predicted=body.position+Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up)*HarborTugNavigation.ShoreLookAhead;
            float heading=ship.Heading+Mathf.Clamp(body.angularVelocity.y*Mathf.Rad2Deg*HarborTugNavigation.ShoreLookAhead,-45,45);
            return navigation.WaterClear(HarborTugNavigation.Flat(Formation(index,side,HarborTugNavigation.StandOff,body.position,ship.Heading)),HarborTugNavigation.Radius+margin)
                &&navigation.WaterClear(HarborTugNavigation.Flat(Formation(index,side,HarborTugNavigation.StandOff,predicted,heading)),HarborTugNavigation.Radius+margin);
        }
        private void Navigate(Tug tug,int index,int sign,bool mayApproach,float dt,float time)
        {
            int preferred=-sign;
            if(!SideClear(index,preferred)){tug.Escaping=true;tug.Approaching=false;}
            else if(tug.Escaping&&SideClear(index,preferred,2)&&!tug.Routing)tug.Escaping=false;
            int side=tug.Escaping?-preferred:preferred;
            if(!SideClear(index,side)&&SideClear(index,-side))side=-side;
            if(tug.Side!=side){tug.Side=side;tug.Approaching=false;}
            Vector3 position=tug.Root.position,staging=Formation(index,side,HarborTugNavigation.StandOff,body.position,ship.Heading);
            Vector2 start=HarborTugNavigation.Flat(position),local=HarborTugNavigation.ToLocal(start,body.position,ship.Heading);
            Vector3 station=TugAssistRules.LocalStation(index,sign,ship.Definition);
            if(!mayApproach||tug.Escaping||side!=preferred)tug.Approaching=false;
            else if(Vector2.Distance(start,HarborTugNavigation.Flat(staging))<.85f)tug.Approaching=true;
            Vector3 goal=tug.Approaching?Formation(index,side,0,body.position,ship.Heading):staging;
            Vector2 target=HarborTugNavigation.Flat(goal),other=HarborTugNavigation.Flat(tugs[1-index].Root.position),waypoint=target;
            // Inside the conservative transit envelope, only withdraw directly outwards along the same borda.
            // The precise hull sweep below still runs; this does not permit crossing through the ship.
            bool withdrawing=!tug.Approaching&&Mathf.Abs(local.x)<ship.Definition.BeamMetres/8+HarborTugNavigation.Radius+.15f
                &&Mathf.Abs(local.y)<ship.Definition.LengthMetres/8+HarborTugNavigation.Radius;
            bool direct=tug.Approaching||withdrawing;
            tug.Routing=false;tug.Waiting=false;
            if(withdrawing){int currentSide=local.x>=0?1:-1;waypoint=HarborTugNavigation.ToWorld(new Vector2(currentSide*(ship.Definition.BeamMetres/8+HarborTugNavigation.Radius+.5f),local.y),body.position,ship.Heading);}
            else if(!tug.Approaching)
            {
                if(!navigation.NextWaypoint(start,target,body.position,ship.Heading,ship.Definition,other,out waypoint)){tug.Waiting=true;return;}
                direct=Vector2.Distance(waypoint,target)<.01f;tug.Routing=!direct;
            }
            Vector2 error=waypoint-start;
            // Feed-forward includes surge, sway AND yaw velocity of the moving station. No parenting/teleporting.
            Vector3 feed=direct?Vector3.ProjectOnPlane(body.GetPointVelocity(goal),Vector3.up):Vector3.zero;
            Vector2 velocity=HarborTugNavigation.Flat(feed)+Vector2.ClampMagnitude(error*1.4f,tug.Approaching?1.2f:2.2f);
            if(tug.Approaching&&Mathf.Abs(Mathf.DeltaAngle(tug.Root.eulerAngles.y,Quaternion.LookRotation(ship.RightOnWater*sign).eulerAngles.y))>5)
                velocity=HarborTugNavigation.Flat(feed); // Turn at stand-off before putting the bow fender alongside.
            Vector2 next=start+velocity*dt;
            if(!direct&&velocity.magnitude*dt>error.magnitude)next=waypoint;
            Vector3 facing=tug.Approaching||withdrawing?ship.RightOnWater*sign:tug.Routing&&error.sqrMagnitude>.01f?HarborTugNavigation.World(error):ship.ForwardOnWater;
            float yaw=Mathf.MoveTowardsAngle(tug.Root.eulerAngles.y,Quaternion.LookRotation(facing).eulerAngles.y,dt*60);
            if(!navigation.SafeSweep(start,tug.Root.eulerAngles.y,next,yaw,body.position,ship.Heading,ship.Definition,other))
            {
                // Try translating without turning, otherwise stop. Never snap through a quay to catch up.
                yaw=tug.Root.eulerAngles.y;
                if(!navigation.SafeSweep(start,yaw,next,yaw,body.position,ship.Heading,ship.Definition,other)){tug.Waiting=true;return;}
            }
            Vector3 p=HarborTugNavigation.World(next);p.y=.1f+WaterWaves.Height(p.x,p.z,time);
            float roll=Mathf.Clamp(Vector3.Dot(WaterWaves.Normal(p.x,p.z,time),ship.RightOnWater)*-24,-12,12);
            tug.Root.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,roll));
        }
        public void SimulateStep(float dt,float time)
        {
            if(!Available||fleetRoot==null||ship==null||!float.IsFinite(dt)||dt<=0)return;
            bool controlled=session!=null&&session.CanControl&&!IntentOpen;
            if(controlled&&session.Mooring!=null){Berthed=session.Mooring.Complete;if(Berthed)StopAll();}
            int sign=PushSign;Vector3 right=ship.RightOnWater,forward=ship.ForwardOnWater;
            float remaining=Vector3.Dot(Destination-body.position,TugAssistRules.ShoreDirection(Berth));
            bool permitted=controlled&&!Berthed&&HoldReason()==null&&remaining>0;
            for(int i=0;i<2;i++)
            {
                Tug tug=tugs[i];Vector3 station=TugAssistRules.LocalStation(i,sign,ship.Definition);
                if(controlled)Navigate(tug,i,sign,permitted&&tug.Commanded,dt,time);
                Vector3 goal=body.position+right*station.x+forward*station.z;
                goal.y=.1f+WaterWaves.Height(goal.x,goal.z,time);
                Vector3 p=tug.Root.position;
                Vector3 planar=goal-p;planar.y=0;
                Quaternion facing=Quaternion.LookRotation(right*sign,Vector3.up);
                tug.Contact=tug.Approaching&&!tug.Escaping&&!tug.Waiting&&planar.magnitude<TugAssistRules.ContactTolerance&&Mathf.Abs(p.y-goal.y)<.45f&&Quaternion.Angle(tug.Root.rotation,facing)<18;
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
            if(controlled)session.Missions?.ReportTugDelivery(dt,tugs[0].Delivered,tugs[1].Delivered,Berth);
        }
        private void LateUpdate()
        {
            if(guide==null)return;Vector3 c=Destination;float w=ship.Definition.BeamMetres/8+.2f,h=ship.Definition.LengthMetres/8+.35f;
            Vector3[] points={c+new Vector3(-w,0,-h),c+new Vector3(-w,0,h),c+new Vector3(w,0,h),c+new Vector3(w,0,-h),c+new Vector3(-w,0,-h)};
            for(int i=0;i<points.Length;i++)points[i].y=WaterWaves.SurfaceY(points[i],Time.time)+.08f;
            guide.SetPositions(points);
        }
        private void CreateGuide()
        {
            var go=new GameObject("Selected berth guide");go.transform.SetParent(fleetRoot.transform,false);guide=go.AddComponent<LineRenderer>();
            guide.positionCount=5;guide.useWorldSpace=true;guide.widthMultiplier=.07f;guide.startColor=guide.endColor=Teal;
            var owner=go.AddComponent<EnvironmentResources>();guide.sharedMaterial=owner.Own(new Material(Shader.Find("Sprites/Default")));guide.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        public static Rect PanelRect(HelmLayout layout)=>new(316,152,Mathf.Min(620,layout.Width-658),124);
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
            if(!Available||session==null||session.MenuOpen||session.Missions!=null&&session.Missions.BlocksControl)return;
            ui??=new MaritimeUiTheme();GUI.depth=-20;HelmLayout layout=HelmLayout.Current;Matrix4x4 previous=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(layout.Offset,Quaternion.identity,Vector3.one*layout.Scale);
            try
            {
                if(IntentOpen){DrawIntent(layout);return;}
                if(session.Mooring==null||!session.Mooring.PanelVisible)DrawPanel(layout);
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
            ui.Text(new Rect(card.x+28,card.y+22,684,24),"LİMAN KONTROL  /  "+ship.Definition.Name,14,Teal,bold:true);
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
            var mission=session.Missions;var run=mission!=null&&mission.Assigned?mission.Run:null;
            string hint=Berthed?"HALATLAR BAĞLI · YANAŞMA TAMAMLANDI":session.Mooring!=null&&session.Mooring.Engaged?"Halatlar ve römorkör desteği birlikte çalışıyor":HoldReason()??(PushSign>0?"İskele tarafından sancağa itme →":"← Sancak tarafından iskeleye itme");
            ui.Text(new Rect(p.x+12,p.y+7,p.width-125,20),run!=null?$"{ship.Definition.Name} {mission.Index+1}/{DockingMissionCatalog.Count} · GİRİŞ {run.GatesPassed}/3":TugAssistRules.Name(Berth)+"  /  RÖMORKÖR",12,Teal,bold:true);
            bool targetEnabled=GUI.enabled;GUI.enabled=run==null;if(GUI.Button(new Rect(p.xMax-107,p.y+7,95,26),run!=null?"ATANDI":"HEDEF",ui.Button))RequestIntent();GUI.enabled=targetEnabled;
            float w=(p.width-126)*.5f;
            for(int i=0;i<2;i++)
            {
                Tug tug=tugs[i];string state=tug.Waiting?"GÜVENLİ YOL BEKLİYOR":tug.Escaping?"RIHTIMDAN ÇEKİLİYOR":tug.Routing?"BORDAYA İNTİKAL":!tug.Commanded?"REFAKAT · İT":tug.Delivered>.001f?$"İTİYOR %{tug.Delivered*100:0}":tug.Contact?"TEMASTA · BEKLİYOR":ship.SpeedKnots>=TugAssistRules.MaximumAssistKnots?"REFAKAT · 5 kn ALTI":"YAKLAŞIYOR";
                string label=(i==0?"BAŞ":"KIÇ")+"  "+state;
                GUI.enabled=!Berthed;
                if(GUI.Button(new Rect(p.x+12+i*(w+8),p.y+39,w,36),label,tug.Commanded?ui.Selected:ui.Button))Toggle(i);
            }
            GUI.enabled=true;
            if(GUI.Button(new Rect(p.xMax-94,p.y+39,82,36),"İKİSİ DUR",ui.Danger))StopAll();
            float metres=Vector3.Distance(new Vector3(ship.transform.position.x,0,ship.transform.position.z),new Vector3(Destination.x,0,Destination.z))*VesselCatalog.ModelMetresPerUnit;
            string sway=Mathf.Abs(SwayKnots)<.005f?"YAN 0,00 kn":$"YAN {(SwayKnots>0?"→":"←")} {Mathf.Abs(SwayKnots):0.00} kn";
            string first=run!=null?run.ExerciseHint:hint+$" · {sway} · {metres:0} m";
            ui.Text(new Rect(p.x+12,p.y+80,p.width-24,20),first,12,Berthed?Teal:Muted);
            string detail=run!=null&&run.Signals.Expected!=WhistleSignal.None?$"DÜDÜK: {ManeuverSignalMonitor.Meaning(run.Signals.Expected)} · {run.Signals.SecondsLeft(run.Elapsed):0} sn":run!=null?$"{TugAssistRules.Name(Berth)} · {metres:0} m · {sway} · LİMAN {run.Definition.HarborSpeedLimit:0} kn · {HoldReason()??"İTMEYE HAZIR"}":"Halat alındıktan sonra da destek verebilirsin; tamamlanınca itme durur.";
            ui.Text(new Rect(p.x+12,p.y+102,p.width-24,18),detail,11,run!=null&&run.Signals.Expected!=WhistleSignal.None?Amber:Muted);
        }
        public void DrawMooringControls(Rect row)
        {
            ui??=new MaritimeUiTheme();bool enabled=GUI.enabled;
            GUI.enabled=enabled&&session.CanControl&&!Berthed;
            float width=(row.width-94)*.5f;
            for(int i=0;i<2;i++)
            {
                string status=tugs[i].Delivered>.001f?$"İTİYOR %{tugs[i].Delivered*100:0}":tugs[i].Commanded?"DURDUR":"İT";
                if(GUI.Button(new Rect(row.x+i*(width+6),row.y,width,row.height),(i==0?"BAŞ · ":"KIÇ · ")+status,tugs[i].Commanded?ui.Selected:ui.Button))Toggle(i);
            }
            if(GUI.Button(new Rect(row.xMax-82,row.y,82,row.height),"İKİSİ DUR",ui.Danger))StopAll();
            GUI.enabled=enabled;
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
