using UnityEngine;
namespace Meridian
{
    /// <summary>Reduced two-line game model. Units are 4 m; not a mooring-load or seamanship trainer.</summary>
    public static class MooringRules
    {
        public const float CastSeconds=1.4f,ReceiveSeconds=2.0f,HaulSeconds=14f;
        public static Vector3 Destination(VesselDefinition vessel,HarborBerth berth)
        {int side=berth==HarborBerth.East?1:-1;float edge=side>0?32.5f:-30.5f;return new Vector3(edge-side*(vessel.BeamMetres/8+.35f),.1f,side>0?70:54);}
        public static float PierY(HarborBerth berth)=>berth==HarborBerth.East?1.97f:2.125f;
        public static Vector3 Fairlead(VesselDefinition vessel,int station,int side)
        {
            float z=vessel.LengthMetres/8*(station==0?.64f:-.72f);
            float width=vessel.IsSmallCraft?SmallCraftHull.HalfWidth(vessel.Id,z):vessel.UsesPods?AnadoluModelBuilder.DeckHalfWidth(z):vessel.BeamMetres/8*.91f;
            float y=vessel.IsSmallCraft?SmallCraftHull.DeckHeight(vessel.Id,z):vessel.UsesPods?AnadoluModelBuilder.FlightDeckHeight(side*width,z):1.035f+Mathf.SmoothStep(0,.43f,Mathf.InverseLerp(7,15.1f,z));
            return new Vector3(side*(width-.10f),y+.18f,z);
        }
        public static Vector3 ShoreAnchor(VesselDefinition vessel,HarborBerth berth,int station)
        {int side=berth==HarborBerth.East?1:-1;Vector3 target=Destination(vessel,berth);return new Vector3(side>0?33.6f:-31.6f,PierY(berth)+.14f,target.z+Fairlead(vessel,station,side).z);}
        public static bool CastEnvelope(Vector3 fairlead,Vector3 anchor,float heading,float knots,float sway)
        {
            Vector3 gap=anchor-fairlead;gap.y=0;
            return float.IsFinite(gap.sqrMagnitude)&&gap.magnitude<=6&&gap.magnitude>.15f&&Mathf.Abs(Mathf.DeltaAngle(heading,0))<12&&knots<.60f&&Mathf.Abs(sway)<.35f;
        }
        // A unilateral spring with radial damping. A slack line never pushes the vessel away from shore.
        public static Vector3 PullAcceleration(Vector3 point,Vector3 anchor,Vector3 pointVelocity,float rest)
        {
            Vector3 gap=anchor-point;gap.y=0;pointVelocity.y=0;float length=gap.magnitude;
            if(length<.001f||!float.IsFinite(length)||!float.IsFinite(rest)||!float.IsFinite(pointVelocity.x)||!float.IsFinite(pointVelocity.z)||rest<0||length<=rest)return Vector3.zero;
            Vector3 direction=gap/length;float outward=-Vector3.Dot(pointVelocity,direction);
            return direction*Mathf.Clamp((length-rest)*.025f+outward*.30f,0,.022f);
        }
        public static bool Settled(Vector3 position,float heading,float knots,float sway,VesselDefinition vessel,HarborBerth berth)
        {Vector3 target=Destination(vessel,berth);return Mathf.Abs(position.x-target.x)*4<3&&Mathf.Abs(position.z-target.z)*4<8&&Mathf.Abs(Mathf.DeltaAngle(heading,0))<6&&knots<.30f&&Mathf.Abs(sway)<.15f;}
    }
}
