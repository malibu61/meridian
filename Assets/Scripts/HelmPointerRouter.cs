using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    public sealed class HelmPointerRouter
    {
        private readonly Dictionary<int,int> pointers=new();
        public int Count=>pointers.Count;
        public bool Begin(int pointer,int control)
        {
            if(control<0||control>5||pointers.ContainsKey(pointer)||pointers.ContainsValue(control))return false;
            if(IsEngine(control)&&pointers.ContainsValue(2))return false;
            if(control==2)
            {
                List<int> release=new();foreach(var pair in pointers)if(IsEngine(pair.Value))release.Add(pair.Key);
                foreach(int key in release)pointers.Remove(key);
            }
            pointers.Add(pointer,control);return true;
        }
        public bool TryGet(int pointer,out int control)=>pointers.TryGetValue(pointer,out control);
        public void End(int pointer)=>pointers.Remove(pointer);
        public void Clear()=>pointers.Clear();
        private static bool IsEngine(int control)=>control==0||control==1||control==4||control==5;
        public static int Hit(HelmLayout layout,Vector2 point)
        {
            Rect engine=layout.Port;
            bool OnRail(Rect panel){Rect rail=HelmLayout.Rail(panel);return new Rect(panel.x,rail.y-14,panel.width,rail.height+28).Contains(point);}
            if(OnRail(engine))return 0;if(OnRail(layout.Starboard))return 1;if(OnRail(layout.Master))return 2;
            if(layout.FourEngines){if(OnRail(layout.PortInner))return 4;if(OnRail(layout.StarboardInner))return 5;}
            Rect rudder=layout.Rudder;return new Rect(rudder.x,rudder.y+46,rudder.width,60).Contains(point)?3:-1;
        }
    }
}
