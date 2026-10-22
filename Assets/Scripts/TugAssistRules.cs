using UnityEngine;
namespace Meridian
{
    public enum HarborBerth { None, East, West }

    /// <summary>Game harbour geometry and reduced pushing model, not a certified tug-performance model.</summary>
    public static class TugAssistRules
    {
        public const float TugLength=7.5f, TugBeam=2.45f, ContactTolerance=.4f, MaximumAssistKnots=5f;
        public static bool Available(SimulationMode mode,VesselId vessel)=>mode==SimulationMode.Harbor&&vessel==VesselId.Anadolu;
        public static bool Valid(HarborBerth berth)=>berth==HarborBerth.East||berth==HarborBerth.West;
        public static Vector3 Destination(HarborBerth berth)=>berth==HarborBerth.East?new Vector3(28.1f,.1f,70):new Vector3(-26.1f,.1f,54);
        public static string Name(HarborBerth berth)=>berth==HarborBerth.East?"DOĞU RIHTIMI":"BATI RIHTIMI";
        public static Vector3 ShoreDirection(HarborBerth berth)=>berth==HarborBerth.East?Vector3.right:Vector3.left;
        public static int PushSign(Vector3 shipRight,HarborBerth berth)=>Vector3.Dot(shipRight,ShoreDirection(berth))>=0?1:-1;
        public static float SpeedFactor(float knots)=>Mathf.Clamp01((MaximumAssistKnots-knots)/3f);
        public static float StationZ(int tug)=>tug==0?16f:-16f;
        public static Vector3 LocalStation(int tug,int pushSign)
        {
            float z=StationZ(tug),halfBeam=AnadoluModelBuilder.WaterlineHalfWidth(z)*1.025f;
            return new Vector3(-pushSign*(halfBeam+TugLength*.5f+.12f),0,z);
        }
        // Assistance is available throughout the harbour approach, not just within 48 m of the final berth.
        public static bool InArea(Vector3 position,HarborBerth berth)=>Valid(berth)&&position.z>=-75&&position.z<=110&&position.x>-31&&position.x<33;
        public static bool Aligned(float heading)=>Mathf.Abs(Mathf.DeltaAngle(heading,0))<=30;
        public static bool AtBerth(Vector3 position,float heading,float knots,HarborBerth berth)=>Valid(berth)&&Mathf.Abs(position.x-Destination(berth).x)<.65f&&Mathf.Abs(position.z-Destination(berth).z)<8&&Mathf.Abs(Mathf.DeltaAngle(heading,0))<6&&knots<.8f;
        public static float ForceMagnitude(float mass,float pressure,float knots,float remaining)
            =>mass*.022f*Mathf.Clamp01(pressure)*SpeedFactor(knots)*Mathf.Clamp01(remaining/.8f);
    }
}
