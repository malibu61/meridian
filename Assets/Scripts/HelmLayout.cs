using UnityEngine;
namespace Meridian
{
    public readonly struct HelmLayout
    {
        public readonly float Scale,Width,Height;
        public readonly Vector2 Offset;
        public readonly bool FourEngines;
        private static bool activeFourEngines;
        public static void Configure(VesselDefinition definition)=>activeFourEngines=definition!=null&&definition.HasFourEngines;
        public Rect Header=>new(24,20,Width-48,80);
        public const int HeaderMetricCount=5;
        public Rect HeaderMetric(int index)
        {Rect r=Header;float width=(r.width-410)/HeaderMetricCount;return new Rect(r.x+390+index*width,r.y,width,r.height);}
        public Rect Status=>new(24,110,Width-48,36);
        public Rect RollInstrument=>new(24,152,278,124);
        public Rect RouteMap=>new(Width-182,158,158,104);
        public Rect Port=>new(24,Height-438,142,414);
        public Rect Starboard=>new(Width-166,Height-438,142,414);
        public Rect PortInner=>FourEngines?new Rect(178,Height-398,112,374):default;
        public Rect StarboardInner=>FourEngines?new Rect(Width-290,Height-398,112,374):default;
        public Rect Master=>FourEngines?new Rect(Width*.5f+120,Height-246,128,222):new Rect(Width-326,Height-438,142,414);
        public Rect Rudder=>FourEngines?new Rect(Width*.5f-248,Height-182,352,158):new Rect(190+(Width-540-Mathf.Min(470,Width-540))*.5f,Height-198,Mathf.Min(470,Width-540),174);
        public static Rect Rail(Rect panel)=>panel.height<320?new Rect(panel.center.x-25,panel.y+82,50,panel.height-112):new Rect(panel.center.x-25,panel.y+100,50,panel.height-138);
        public static Rect SteeringRail(Rect panel)=>new(panel.x+28,panel.y+64,panel.width-56,34);
        public HelmLayout(int width,int height,Rect safe,bool fourEngines=false)
        {
            FourEngines=fourEngines;
            if(safe.width<1||safe.height<1)safe=new Rect(0,0,width,height);
            Scale=Mathf.Max(.01f,Mathf.Min(safe.width/1280f,safe.height/720f));Width=safe.width/Scale;Height=safe.height/Scale;
            Offset=new Vector2(safe.x,height-safe.yMax);
        }
        public static HelmLayout Current=>new(Screen.width,Screen.height,Screen.safeArea,activeFourEngines);
        public Vector2 FromScreen(Vector2 point)=>(new Vector2(point.x,Screen.height-point.y)-Offset)/Scale;
        public Vector2 FromGui(Vector2 point)=>(point-Offset)/Scale;
        public Rect Physical(Rect rect)=>new(Offset.x+rect.x*Scale,Offset.y+rect.y*Scale,rect.width*Scale,rect.height*Scale);
        public bool BlocksCamera(Vector2 point)=>Header.Contains(point)||Status.Contains(point)||RollInstrument.Contains(point)||RouteMap.Contains(point)||Port.Contains(point)||Starboard.Contains(point)||Master.Contains(point)||Rudder.Contains(point)||(FourEngines&&(PortInner.Contains(point)||StarboardInner.Contains(point)));
    }
}
