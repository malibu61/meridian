using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    public sealed class MaritimeUiTheme:System.IDisposable
    {
        public static readonly Color Ink=new(.94f,.96f,.98f),Muted=new(.57f,.65f,.72f),Teal=new(.27f,.88f,.77f),Amber=new(1f,.74f,.32f),Red=new(1f,.38f,.36f);
        private readonly List<Texture2D> textures=new();
        private readonly Dictionary<(int,Color,TextAnchor,bool),GUIStyle> labels=new();
        public readonly GUIStyle Panel,Inset,Selected,Button,Primary,Danger,Handle;
        public bool SelectionLayoutMatches=>Selected.fontSize==Button.fontSize&&Selected.fontStyle==Button.fontStyle&&Selected.font==Button.font&&Selected.alignment==Button.alignment&&Selected.contentOffset==Button.contentOffset&&Selected.padding.left==Button.padding.left&&Selected.padding.top==Button.padding.top&&Selected.padding.right==Button.padding.right&&Selected.padding.bottom==Button.padding.bottom&&Selected.CalcSize(new GUIContent("D · 090°"))==Button.CalcSize(new GUIContent("D · 090°"));
        public MaritimeUiTheme()
        {
            Texture2D panel=Rounded(new Color(.045f,.065f,.09f,.96f),new Color(.16f,.21f,.27f,.85f));
            Texture2D inset=Rounded(new Color(.025f,.04f,.06f,.98f),new Color(.12f,.17f,.23f));
            Texture2D hover=Rounded(new Color(.095f,.14f,.18f),new Color(.3f,.4f,.47f));
            Texture2D selected=Rounded(new Color(.045f,.16f,.17f),new Color(.18f,.51f,.47f));
            Panel=Box(panel);Inset=Box(inset);Selected=MakeButton(selected,selected,selected,Ink);Button=MakeButton(panel,hover,selected,Ink);
            Primary=MakeButton(Rounded(Teal,Teal),Rounded(new Color(.47f,1f,.88f),Teal),selected,new Color(.025f,.09f,.10f));
            Danger=MakeButton(Rounded(new Color(.26f,.075f,.09f),new Color(.50f,.17f,.20f)),Rounded(new Color(.4f,.10f,.13f),Red),selected,new Color(1f,.74f,.73f));
            Handle=Box(Rounded(new Color(.75f,.80f,.83f),new Color(.95f,.98f,1f)));
        }
        private static GUIStyle Box(Texture2D t)=>new(GUI.skin.box){border=new RectOffset(12,12,12,12),normal={background=t},padding=new RectOffset(0,0,0,0)};
        private static GUIStyle MakeButton(Texture2D normal,Texture2D hover,Texture2D pressed,Color text)
        {
            GUIStyle s=Box(normal);s.alignment=TextAnchor.MiddleCenter;s.fontSize=16;s.fontStyle=FontStyle.Bold;s.contentOffset=Vector2.zero;s.wordWrap=false;
            s.normal.textColor=text;s.hover.background=hover;s.hover.textColor=text;s.active.background=pressed;s.active.textColor=Ink;s.focused.background=hover;s.focused.textColor=text;return s;
        }
        public GUIStyle Label(int size,Color color,TextAnchor anchor=TextAnchor.MiddleLeft,bool bold=false)
        {
            var key=(size,color,anchor,bold);
            if(!labels.TryGetValue(key,out GUIStyle style))
            {
                style=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,alignment=anchor,wordWrap=true,padding=new RectOffset(0,0,0,0),normal={textColor=color}};labels.Add(key,style);
            }
            return style;
        }
        public void Text(Rect r,string text,int size,Color color,TextAnchor anchor=TextAnchor.MiddleLeft,bool bold=false)=>GUI.Label(r,text,Label(size,color,anchor,bold));
        public void Surface(Rect r,bool selected=false)
        {Fill(new Rect(r.x,r.y+3,r.width,r.height),new Color(0,0,0,.13f));GUI.Box(r,GUIContent.none,selected?Selected:Panel);}
        public static void Fill(Rect r,Color color)
        {Color old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        public static void Line(Vector2 a,Vector2 b,float thickness,Color color)
        {
            Matrix4x4 previous=GUI.matrix;GUI.matrix=previous*Matrix4x4.TRS(a,Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);
            Fill(new Rect(0,-thickness*.5f,Vector2.Distance(a,b),thickness),color);GUI.matrix=previous;
        }
        private Texture2D Rounded(Color fill,Color rim)
        {
            const int size=64;Texture2D t=new(size,size,TextureFormat.RGBA32,false){name="Meridian UI surface",hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Vector2 p=new(Mathf.Abs(x-31.5f)-23.5f,Mathf.Abs(y-31.5f)-23.5f);
                float d=new Vector2(Mathf.Max(p.x,0),Mathf.Max(p.y,0)).magnitude+Mathf.Min(Mathf.Max(p.x,p.y),0)-7f;
                Color c=d>-.9f?rim:Color.Lerp(fill,rim,Mathf.Clamp01(d+1.9f));c.a*=Mathf.Clamp01(.5f-d);t.SetPixel(x,y,c);
            }
            t.Apply(false,true);textures.Add(t);return t;
        }
        public void Dispose(){foreach(Texture2D t in textures)if(t!=null)Object.Destroy(t);textures.Clear();labels.Clear();}
    }
}
