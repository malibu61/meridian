using UnityEngine;
using static Meridian.MaritimeUiTheme;
namespace Meridian
{
    /// <summary>Pendulum-style roll instrument. Positive physical roll is port/left.</summary>
    public static class RollGauge
    {
        public const float ScaleDegrees=40,SweepDegrees=60;
        public static float NeedleAngle(float roll)=>float.IsFinite(roll)?Mathf.Clamp(roll,-ScaleDegrees,ScaleDegrees)/ScaleDegrees*SweepDegrees:0;
        public static Vector2 Point(Vector2 pivot,float radius,float roll)
        {float a=NeedleAngle(roll)*Mathf.Deg2Rad;return pivot+new Vector2(-Mathf.Sin(a),Mathf.Cos(a))*radius;}
        public static void Draw(MaritimeUiTheme ui,Rect r,float roll)
        {
            bool valid=float.IsFinite(roll),overflow=valid&&Mathf.Abs(roll)>ScaleDegrees;
            Color accent=overflow?Amber:Teal;
            ui.Surface(r);ui.Surface(new Rect(r.x+7,r.y+7,155,r.height-14));
            Vector2 pivot=new(r.x+84,r.y+19);
            // Scale is spread over a wider arc so small wave-driven rolls remain readable on mobile.
            Arc(pivot,66,-ScaleDegrees,ScaleDegrees,1.3f,new Color(.34f,.43f,.49f));
            Line(pivot+Vector2.up*9,pivot+Vector2.up*62,1,new Color(.22f,.3f,.35f));
            for(int degree=-(int)ScaleDegrees;degree<=ScaleDegrees;degree+=2)
            {
                bool major=degree%10==0;
                Line(Point(pivot,major?57:62,degree),Point(pivot,66,degree),major?1.6f:.7f,degree==0?Ink:Muted);
                if(major)
                {
                    Vector2 p=Point(pivot,78,degree);
                    ui.Text(new Rect(p.x-12,p.y-7,24,14),Mathf.Abs(degree).ToString(),10,degree==0?Ink:Muted,TextAnchor.MiddleCenter,degree==0);
                }
            }
            if(valid)
            {
                Arc(pivot,69,0,Mathf.Clamp(roll,-ScaleDegrees,ScaleDegrees),2.1f,accent);
                Vector2 tip=Point(pivot,61,roll),direction=(tip-pivot).normalized;
                Line(pivot-direction*8,tip,3.5f,new Color(0,0,0,.6f));
                Line(pivot-direction*7,tip,2.1f,Amber);
            }
            // Small metal pivot cap; no textures or objects are allocated per frame.
            for(int y=-4;y<=4;y++){float half=Mathf.Sqrt(16-y*y);Fill(new Rect(pivot.x-half,pivot.y+y,half*2,1),Ink);}
            Fill(new Rect(pivot.x-1,pivot.y-1,2,2),new Color(.1f,.16f,.2f));
            float x=r.x+176,w=r.width-188;
            ui.Text(new Rect(x,r.y+9,w,17),"YALPA",12,Muted,bold:true);
            ui.Text(new Rect(x,r.y+31,w,34),valid?$"{Mathf.Abs(roll):0.0}°":"—",28,Ink,bold:true);
            ui.Text(new Rect(x,r.y+68,w,18),valid?PrototypeHud.RollSide(roll):"VERİ YOK",12,accent,bold:true);
            ui.Text(new Rect(x,r.y+96,w,13),overflow?"ÖLÇEK DIŞI":"ÖLÇEK ±40°",9,overflow?Amber:Muted);
            ui.Text(new Rect(r.x+13,r.y+104,58,12),"İSKELE",9,valid&&roll>=.05f?Teal:Muted);
            ui.Text(new Rect(r.x+99,r.y+104,58,12),"SANCAK",9,valid&&roll<=-.05f?Teal:Muted,TextAnchor.MiddleRight);
        }
        private static void Arc(Vector2 pivot,float radius,float from,float to,float width,Color color)
        {
            if(Mathf.Abs(to-from)<.0001f)return;
            const int segments=40;Vector2 previous=Point(pivot,radius,from);
            for(int i=1;i<=segments;i++)
            {Vector2 next=Point(pivot,radius,Mathf.Lerp(from,to,i/(float)segments));Line(previous,next,width,color);previous=next;}
        }
    }
}
