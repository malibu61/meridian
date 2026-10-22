using UnityEngine;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    public sealed class PrototypeHud:MonoBehaviour
    {
        private ShipController ship;
        private ShipBuoyancy buoyancy;
        private GameSessionController session;
        private MaritimeUiTheme ui;
        private readonly HelmPointerRouter pointers=new();
        private readonly float[] nextKeyRepeat=new float[5];
        private VesselId lastVessel;
        private bool resetMouse;
        public void SetShip(ShipController controller){ship=controller;buoyancy=controller.GetComponent<ShipBuoyancy>();}
        public float RollDegrees=>buoyancy!=null?buoyancy.RollDegrees:0;
        // Positive roll tilts the ship's up vector to port; negative is starboard.
        public static string RollSide(float degrees)=>Mathf.Abs(degrees)<.05f?"DENGE":degrees>0?"İSKELE":"SANCAK";
        public void SetSession(GameSessionController controller)=>session=controller;
        private bool CanControl=>ship!=null&&(session==null||session.CanControl);
        private void Update()
        {
            if(ship!=null&&lastVessel!=ship.Definition.Id){lastVessel=ship.Definition.Id;pointers.Clear();System.Array.Clear(nextKeyRepeat,0,nextKeyRepeat.Length);resetMouse=true;}
            if(!CanControl){pointers.Clear();return;}
            ReadKeyboard();HelmLayout layout=HelmLayout.Current;
            for(int i=0;i<Input.touchCount;i++)
            {
                Touch touch=Input.GetTouch(i);Vector2 point=layout.FromScreen(touch.position);
                if(touch.phase==TouchPhase.Ended||touch.phase==TouchPhase.Canceled){pointers.End(touch.fingerId);continue;}
                if(touch.phase==TouchPhase.Began)pointers.Begin(touch.fingerId,HelmPointerRouter.Hit(layout,point));
                if(pointers.TryGet(touch.fingerId,out int control))ApplyPointer(layout,point,control);
            }
        }
        private void ReadKeyboard()
        {
            Repeat(0,KeyCode.W,KeyCode.S);Repeat(1,KeyCode.Q,KeyCode.A);Repeat(2,KeyCode.E,KeyCode.D);
            if(ship.Definition.HasFourEngines){Repeat(3,KeyCode.Z,KeyCode.X);Repeat(4,KeyCode.C,KeyCode.V);}
            float step=.8f*Time.deltaTime;
            if(Input.GetKey(KeyCode.LeftArrow))ship.SetRudder(ship.Rudder-step);
            if(Input.GetKey(KeyCode.RightArrow))ship.SetRudder(ship.Rudder+step);
            if(Input.GetKeyDown(KeyCode.DownArrow))ship.SetRudder(0);
            if(Input.GetKeyDown(KeyCode.Space)){ship.EmergencyStop();pointers.Clear();}
        }
        private void Repeat(int channel,KeyCode ahead,KeyCode astern)
        {
            int direction=(Input.GetKey(ahead)?1:0)-(Input.GetKey(astern)?1:0);
            if(direction==0){nextKeyRepeat[channel]=0;return;}
            if(!Input.GetKeyDown(ahead)&&!Input.GetKeyDown(astern)&&Time.unscaledTime<nextKeyRepeat[channel])return;
            nextKeyRepeat[channel]=Time.unscaledTime+.16f;
            if(channel==0)
            {
                for(int i=0;i<ship.EngineCount;i++)ship.SetEngineThrottle(i,ThrottleDetents.Step(ship.GetEngineThrottle(i),direction));
            }
            else {int engine=channel-1;ship.SetEngineThrottle(engine,ThrottleDetents.Step(ship.GetEngineThrottle(engine),direction));}
        }
        private void ApplyPointer(HelmLayout layout,Vector2 point,int control)
        {
            if(control==3)
            {
                Rect rail=HelmLayout.SteeringRail(layout.Rudder);ship.SetRudder(Mathf.Clamp((point.x-rail.x)/rail.width*2f-1f,-1,1));return;
            }
            if(control>3&&!layout.FourEngines)return;
            Rect panel=control==0?layout.Port:control==1?layout.Starboard:control==4?layout.PortInner:control==5?layout.StarboardInner:layout.Master;
            Rect engineRail=HelmLayout.Rail(panel);
            float value=ThrottleDetents.FromTravel(1f-Mathf.Clamp01((point.y-engineRail.y)/engineRail.height));
            if(control==2)ship.SetCombinedThrottle(value);else ship.SetEngineThrottle(control<2?control:control-2,value);
        }
        private void OnGUI()
        {
            if(!CanControl)return;
            ui??=new MaritimeUiTheme();GUI.depth=-10;
            if(resetMouse){GUIUtility.hotControl=0;resetMouse=false;}
            HelmLayout layout=HelmLayout.Current;ReadMouse(layout);
            Matrix4x4 previous=GUI.matrix;GUI.matrix=Matrix4x4.TRS(layout.Offset,Quaternion.identity,Vector3.one*layout.Scale);
            try
            {
                DrawHeader(layout);RollGauge.Draw(ui,layout.RollInstrument,RollDegrees);
                DrawEngine(layout.Port,layout.FourEngines?"İSKELE DIŞ":"İSKELE",ship.PortThrottle,false);
                DrawEngine(layout.Starboard,layout.FourEngines?"SANCAK DIŞ":"SANCAK",ship.StarboardThrottle,false);
                if(layout.FourEngines){DrawEngine(layout.PortInner,"İSK. İÇ",ship.GetEngineThrottle(2),false);DrawEngine(layout.StarboardInner,"SANC. İÇ",ship.GetEngineThrottle(3),false);}
                DrawEngine(layout.Master,"ORTAK GAZ",ship.CombinedThrottle,true);DrawRudder(layout.Rudder);
            }
            finally{GUI.matrix=previous;}
        }
        private void ReadMouse(HelmLayout layout)
        {
            int captureId=GUIUtility.GetControlID(630184,FocusType.Passive);
            // Touch is read directly per finger, never duplicated through Unity's simulated mouse.
            if(Input.touchCount>0)return;
            Event e=Event.current;Vector2 point=layout.FromGui(e.mousePosition);const int mouse=-1000;
            if(e.type==EventType.MouseDown&&e.button==0)
            {
                int hit=HelmPointerRouter.Hit(layout,point);
                if(pointers.Begin(mouse,hit)){GUIUtility.hotControl=captureId;ApplyPointer(layout,point,hit);e.Use();}
            }
            else if(e.type==EventType.MouseDrag&&pointers.TryGet(mouse,out int control)){ApplyPointer(layout,point,control);e.Use();}
            else if(e.type==EventType.MouseUp&&pointers.TryGet(mouse,out _)){pointers.End(mouse);GUIUtility.hotControl=0;e.Use();}
        }
        private void DrawHeader(HelmLayout layout)
        {
            Rect r=layout.Header;
            ui.Surface(r);
            if(session!=null&&GUI.Button(new Rect(r.x+12,r.y+16,88,48),"MENÜ",ui.Button)){pointers.Clear();session.OpenMenu();GUIUtility.ExitGUI();}
            ui.Text(new Rect(r.x+120,r.y+13,240,22),ship.Definition.Name,19,Ink,bold:true);
            ui.Text(new Rect(r.x+120,r.y+40,240,20),ship.Definition.Pennant+"   /   SEYİR KUMANDASI",12,Muted);
            Metric(layout.HeaderMetric(0),"SÜRAT",$"{ship.SpeedKnots:0.0}","kn");
            Metric(layout.HeaderMetric(1),"BAŞ",$"{ship.Heading:000}","°");
            Metric(layout.HeaderMetric(2),"ROTA",$"{ship.CourseOverGround:000}","°");
            Rect roll=layout.HeaderMetric(3);float angle=RollDegrees;
            Fill(new Rect(roll.x-14,roll.y+20,1,roll.height-40),new Color(.2f,.27f,.33f));
            ui.Text(new Rect(roll.x,roll.y+12,roll.width-10,18),"YALPA",11,Muted);
            ui.Text(new Rect(roll.x,roll.y+32,roll.width-10,27),$"{Mathf.Abs(angle):0.0} °",22,Ink,bold:true);
            ui.Text(new Rect(roll.x,roll.y+61,roll.width-10,14),RollSide(angle),10,Teal,bold:true);
            string state=ship.AllEnginesSynchronized?(ship.CombinedThrottle==0?"STOP":ship.CombinedThrottle>0?"İLERİ":"TORNİSTAN"):"AYRI";
            Metric(layout.HeaderMetric(4),"MAKİNE",state,"");
        }
        private void Metric(Rect r,string label,string value,string unit)
        {
            Fill(new Rect(r.x-14,r.y+20,1,r.height-40),new Color(.2f,.27f,.33f));
            ui.Text(new Rect(r.x,r.y+12,r.width-10,18),label,11,Muted);
            ui.Text(new Rect(r.x,r.y+33,r.width-10,34),value+(unit==""?"":" "+unit),22,Ink,bold:true);
        }
        private void DrawEngine(Rect r,string title,float value,bool master)
        {
            bool linked=ship.AllEnginesSynchronized,compact=r.height<320,narrow=r.width<135;Color color=master?Teal:value<0?Red:Amber;
            ui.Surface(r);Fill(new Rect(r.x+12,r.y+17,3,16),color);ui.Text(new Rect(r.x+22,r.y+13,r.width-28,22),title,narrow?12:14,Ink,bold:true);
            string text=master&&!linked?"AYRI":$"{value*100:+0;-0;0}%";
            ui.Text(new Rect(r.x+10,r.y+(compact?35:42),r.width-20,compact?28:40),text,narrow?24:29,master&&!linked?Muted:color,TextAnchor.MiddleCenter,true);
            Rect rail=HelmLayout.Rail(r);float neutral=rail.y+rail.height*2f/3f;
            Fill(new Rect(rail.center.x-4,rail.y,8,rail.height),new Color(.015f,.025f,.04f));
            Fill(new Rect(rail.center.x-1,rail.y,2,neutral-rail.y),new Color(.13f,.3f,.29f));
            Fill(new Rect(rail.center.x-1,neutral,2,rail.yMax-neutral),new Color(.32f,.13f,.15f));
            for(int percent=-100;percent<=100;percent+=10)
            {
                float y=rail.y+(1f-ThrottleDetents.ToTravel(percent/100f))*rail.height;bool major=percent>=0||percent%20==0;
                Fill(new Rect(rail.center.x-11,y,22,percent==0?2:1),percent==0?Ink:major?new Color(.43f,.50f,.55f):new Color(.2f,.27f,.32f));
                if(major&&(!compact||percent==0||percent==50||Mathf.Abs(percent)==100))ui.Text(new Rect(r.x+4,y-8,narrow?27:36,16),percent.ToString(),narrow?9:11,percent==0?Ink:Muted,TextAnchor.MiddleRight);
            }
            float handleY=rail.y+(1f-ThrottleDetents.ToTravel(value))*rail.height;float handleWidth=narrow?36:48;Rect handle=new(rail.center.x-handleWidth*.5f,handleY-12,handleWidth,24);
            if(!master||linked)
            {
                Fill(new Rect(handle.x-2,handle.y-2,handle.width+4,handle.height+4),new Color(color.r,color.g,color.b,.15f));GUI.Box(handle,GUIContent.none,ui.Handle);
                for(int i=0;i<3;i++)Fill(new Rect(handle.center.x-8+i*7,handle.y+6,1,12),new Color(.28f,.35f,.4f));
            }
            else for(int i=0;i<ship.EngineCount;i++)
            {float y=rail.y+(1f-ThrottleDetents.ToTravel(ship.GetEngineThrottle(i)))*rail.height;Fill(new Rect(rail.center.x-15,y-2,30,4),Teal);}
            float hint=r.center.x+(narrow?23:31),hintWidth=narrow?24:32;
            ui.Text(new Rect(hint,rail.y-6,hintWidth,18),"İL.",9,Teal);ui.Text(new Rect(hint,neutral-8,hintWidth,18),"STOP",8,Ink);
            ui.Text(new Rect(hint,rail.yMax-11,hintWidth,18),"TOR.",9,Red);
            string footer=master?(linked?(ship.EngineCount==4?"4 MAKİNE BİRLİKTE":"İKİ MAKİNE BİRLİKTE"):"DOKUN → EŞİTLE"):narrow?"10 İL. / 5 TORN.":"10 İLERİ / 5 TORNİSTAN";
            ui.Text(new Rect(r.x+4,r.yMax-26,r.width-8,18),footer,narrow?8:9,Muted,TextAnchor.MiddleCenter);
        }
        private void DrawRudder(Rect r)
        {
            ui.Surface(r);ui.Text(new Rect(r.x+18,r.y+10,160,28),ship.Definition.UsesPods?"POD YÖNÜ":"DÜMEN",14,Ink,bold:true);
            ui.Text(new Rect(r.xMax-100,r.y+8,80,30),$"{ship.ActualRudder*35:+0;-0;0}°",24,Teal,TextAnchor.MiddleRight,true);
            Rect rail=HelmLayout.SteeringRail(r);float cy=rail.center.y;Fill(new Rect(rail.x,cy-2,rail.width,4),new Color(.14f,.22f,.27f));
            for(int i=-3;i<=3;i++)Fill(new Rect(rail.center.x+i*rail.width/6f,cy-6,1,12),i==0?Ink:Muted);
            float x=Mathf.Lerp(rail.x,rail.xMax,(ship.Rudder+1f)*.5f);GUI.Box(new Rect(x-11,cy-14,22,28),GUIContent.none,ui.Handle);
            float actualX=Mathf.Lerp(rail.x,rail.xMax,(ship.ActualRudder+1f)*.5f);Fill(new Rect(actualX-2,cy-20,4,8),Teal);
            ui.Text(new Rect(rail.x,r.y+41,100,18),"İSKELE",10,Muted);ui.Text(new Rect(rail.xMax-100,r.y+41,100,18),"SANCAK",10,Muted,TextAnchor.MiddleRight);
            if(GUI.Button(new Rect(r.x+16,r.yMax-54,136,44),"ORTALA  ·  0°",ui.Button))ship.SetRudder(0);
            if(GUI.Button(new Rect(r.xMax-194,r.yMax-54,178,44),"ACİL STOP",ui.Danger)){ship.EmergencyStop();pointers.Clear();GUIUtility.hotControl=0;}
        }
        private void OnApplicationFocus(bool focused){if(!focused)pointers.Clear();}
        private void OnApplicationPause(bool paused){if(paused)pointers.Clear();}
        private void OnDisable(){pointers.Clear();GUIUtility.hotControl=0;}
        private void OnDestroy()=>ui?.Dispose();
    }
}
