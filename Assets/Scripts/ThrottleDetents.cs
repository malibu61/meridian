using UnityEngine;
namespace Meridian
{
    public static class ThrottleDetents
    {
        public const int AheadSteps=10, AsternSteps=5;
        public static float Snap(float value)
        {
            if(float.IsNaN(value)||float.IsInfinity(value))return 0f;
            value=Mathf.Clamp(value,-1f,1f);int steps=value>=0?AheadSteps:AsternSteps;
            return Mathf.Round(value*steps)/steps;
        }
        public static float Step(float value,int direction)
        {
            value=Snap(value);if(direction==0)return value;
            float increment=(direction>0?value>=-.001f:value>.001f)?.1f:.2f;
            return Snap(value+Mathf.Sign(direction)*increment);
        }
        // All 15 detents have equal physical spacing; neutral is two thirds down the rail.
        public static float ToTravel(float value)=>value>=0?(1f+Mathf.Clamp01(value)*2f)/3f:(1f+Mathf.Clamp(value,-1f,0f))/3f;
        public static float FromTravel(float travel)
        {travel=Mathf.Clamp01(travel);return Snap(travel>=1f/3f?(travel*3f-1f)/2f:travel*3f-1f);}
        public static bool Synchronized(float port,float starboard)=>Mathf.Abs(port-starboard)<.001f;
    }
}
