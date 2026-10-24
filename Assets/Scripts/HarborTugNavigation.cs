using UnityEngine;
namespace Meridian
{
    /// <summary>Bounded, allocation-free planar visibility graph for this game's harbour, not a maritime autopilot.</summary>
    public sealed class HarborTugNavigation
    {
        // Circumscribes hull, bow fender and side tyres in ANY yaw; also protects rotation at quay corners.
        public const float Radius=4.3f, StandOff=2f, ShoreLookAhead=12f;
        // Includes the visible non-collider pier caps. Values correspond to PrototypeBootstrap.BuildCoastline.
        public static readonly Rect[] Terrain={
            Rect.MinMaxRect(-86,-25,-44,165),Rect.MinMaxRect(46,1,90,171),
            Rect.MinMaxRect(-45.5f,5.5f,-30.5f,102.5f),Rect.MinMaxRect(32.5f,28.5f,51.5f,111.5f),
            Rect.MinMaxRect(-60,123.5f,60,132.5f)};
        private readonly Rect[] traffic=new Rect[2];private int trafficCount;
        private readonly Vector2[] nodes=new Vector2[64];private readonly float[] costs=new float[64];
        private readonly int[] previous=new int[64];private readonly bool[] visited=new bool[64];private int count;
        public static Vector2 Flat(Vector3 p)=>new(p.x,p.z);
        public static Vector3 World(Vector2 p,float y=0)=>new(p.x,y,p.y);
        public static Rect Expand(Rect r,float radius)=>Rect.MinMaxRect(r.xMin-radius,r.yMin-radius,r.xMax+radius,r.yMax+radius);
        public static Vector2 ToLocal(Vector2 p,Vector3 ship,float heading)
        {Vector3 q=Quaternion.Euler(0,-heading,0)*(World(p)-new Vector3(ship.x,0,ship.z));return Flat(q);}
        public static Vector2 ToWorld(Vector2 p,Vector3 ship,float heading)=>Flat(new Vector3(ship.x,0,ship.z)+Quaternion.Euler(0,heading,0)*World(p));
        public void Configure(DockingMissionRun run)
        {
            trafficCount=0;if(run==null||run.Definition.IsRescue)return;
            var d=VesselCatalog.Get(VesselId.Kumkale);Vector3 p=run.Target;
            float separation=VesselCatalog.Get(run.Definition.Vessel).LengthMetres/8+d.LengthMetres/8+2.3f;
            if(run.Definition.TightBerth){AddTraffic(p+Vector3.forward*separation,d);AddTraffic(p-Vector3.forward*separation,d);}
            else AddTraffic(new Vector3(-20,.1f,-8),d);
        }
        private void AddTraffic(Vector3 p,VesselDefinition d)=>traffic[trafficCount++]=Rect.MinMaxRect(p.x-d.BeamMetres/8,p.z-d.LengthMetres/8,p.x+d.BeamMetres/8,p.z+d.LengthMetres/8);
        public bool WaterClear(Vector2 p,float radius=Radius)
        {foreach(var r in Terrain)if(Expand(r,radius).Contains(p))return false;for(int i=0;i<trafficCount;i++)if(Expand(traffic[i],radius).Contains(p))return false;return true;}
        public bool WaterSegmentClear(Vector2 a,Vector2 b)
        {foreach(var r in Terrain)if(Intersects(a,b,Expand(r,Radius)))return false;for(int i=0;i<trafficCount;i++)if(Intersects(a,b,Expand(traffic[i],Radius)))return false;return true;}
        public static bool Intersects(Vector2 a,Vector2 b,Rect r)
        {
            float lo=0,hi=1;Vector2 delta=b-a;
            for(int axis=0;axis<2;axis++)
            {
                float start=axis==0?a.x:a.y,d=axis==0?delta.x:delta.y,min=axis==0?r.xMin:r.yMin,max=axis==0?r.xMax:r.yMax;
                if(Mathf.Abs(d)<.00001f){if(start<min||start>max)return false;continue;}
                float t0=(min-start)/d,t1=(max-start)/d;if(t0>t1)(t0,t1)=(t1,t0);lo=Mathf.Max(lo,t0);hi=Mathf.Min(hi,t1);if(lo>hi)return false;
            }
            return true;
        }
        private static Rect ShipEnvelope(VesselDefinition vessel)=>Rect.MinMaxRect(-vessel.BeamMetres/8-Radius,-vessel.LengthMetres/8-Radius,vessel.BeamMetres/8+Radius,vessel.LengthMetres/8+Radius);
        private bool SegmentClear(Vector2 a,Vector2 b,Vector3 ship,float heading,VesselDefinition vessel,Vector2 other)
        {
            if(!WaterSegmentClear(a,b))return false;
            if(Intersects(ToLocal(a,ship,heading),ToLocal(b,ship,heading),ShipEnvelope(vessel)))return false;
            return !Intersects(a,b,new Rect(other.x-Radius*2,other.y-Radius*2,Radius*4,Radius*4));
        }
        private bool NodeClear(Vector2 p,Vector3 ship,float heading,VesselDefinition vessel,Vector2 other)
            =>WaterClear(p)&&!ShipEnvelope(vessel).Contains(ToLocal(p,ship,heading))&&Vector2.Distance(p,other)>Radius*2+.03f;
        private void AddCorners(Rect rect,Vector3 ship,float heading,VesselDefinition vessel,Vector2 other,bool local=false)
        {
            Rect r=Expand(rect,.10f);
            for(int i=0;i<4;i++)
            {
                Vector2 p=new(i<2?r.xMin:r.xMax,i%2==0?r.yMin:r.yMax);
                if(local)p=ToWorld(p,ship,heading);if(count<nodes.Length&&NodeClear(p,ship,heading,vessel,other))nodes[count++]=p;
            }
        }
        public bool NextWaypoint(Vector2 start,Vector2 goal,Vector3 ship,float heading,VesselDefinition vessel,Vector2 other,out Vector2 waypoint)
        {
            waypoint=start;if(!NodeClear(goal,ship,heading,vessel,other))return false;
            if(SegmentClear(start,goal,ship,heading,vessel,other)){waypoint=goal;return true;}
            count=2;nodes[0]=start;nodes[1]=goal;
            foreach(var r in Terrain)AddCorners(Expand(r,Radius),ship,heading,vessel,other);
            for(int i=0;i<trafficCount;i++)AddCorners(Expand(traffic[i],Radius),ship,heading,vessel,other);
            AddCorners(ShipEnvelope(vessel),ship,heading,vessel,other,true);
            AddCorners(new Rect(other.x-Radius*2,other.y-Radius*2,Radius*4,Radius*4),ship,heading,vessel,other);
            for(int i=0;i<count;i++){costs[i]=float.PositiveInfinity;previous[i]=-1;visited[i]=false;}costs[0]=0;
            for(int step=0;step<count;step++)
            {
                int best=-1;float cost=float.PositiveInfinity;for(int i=0;i<count;i++)if(!visited[i]&&costs[i]<cost){best=i;cost=costs[i];}
                if(best<0)break;if(best==1){int next=1;while(previous[next]>0)next=previous[next];waypoint=nodes[next];return true;}
                visited[best]=true;
                for(int i=1;i<count;i++)if(!visited[i])
                {
                    float next=cost+Vector2.Distance(nodes[best],nodes[i]);if(next>=costs[i]||!SegmentClear(nodes[best],nodes[i],ship,heading,vessel,other))continue;
                    costs[i]=next;previous[i]=best;
                }
            }
            return false;
        }
        public Vector2 SafeSpawn(Vector2 desired,Vector3 ship,float heading,VesselDefinition vessel,Vector2 other)
        {
            if(NodeClear(desired,ship,heading,vessel,other))return desired;
            for(float radius=2;radius<=80;radius+=2)for(int i=0;i<32;i++)
            {float a=i*Mathf.PI/16;Vector2 p=desired+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;if(NodeClear(p,ship,heading,vessel,other))return p;}
            return new Vector2(ship.x,-100); // Last-resort initial spawn in open water; never used to teleport a moving tug.
        }
        // Exact-ish waterline polygon rather than the inflated transit envelope, allowing a bow fender to contact.
        public static bool HullPoseClear(Vector2 center,float yaw,Vector3 ship,float heading,VesselDefinition vessel)
        {
            Vector2 c=ToLocal(center,ship,heading);float halfLength=vessel.LengthMetres/8;
            if(Mathf.Abs(c.x)>vessel.BeamMetres/8+Radius||Mathf.Abs(c.y)>halfLength+Radius+1)return true;
            float stern=vessel.UsesPods?-AnadoluModelBuilder.HalfLength:-14.25f,bow=vessel.UsesPods?AnadoluModelBuilder.HalfLength:15.1f;
            float Width(float z)=>vessel.UsesPods?AnadoluModelBuilder.WaterlineHalfWidth(z)*1.025f:BarbarosModelBuilder.HalfWidth(z)*.92f;
            if(c.y>=stern&&c.y<=bow&&Mathf.Abs(c.x)<Width(c.y))return false;
            Rect tug=new(-TugAssistRules.TugBeam*.5f,-TugAssistRules.TugLength*.5f,TugAssistRules.TugBeam,TugAssistRules.TugLength);
            Vector2 TugLocal(Vector2 p){Vector2 w=ToWorld(p,ship,heading);return ToLocal(w,World(center),yaw);}
            for(int side=-1;side<=1;side+=2)for(int i=0;i<24;i++)
            {float z0=Mathf.Lerp(stern,bow,i/24f),z1=Mathf.Lerp(stern,bow,(i+1)/24f);if(Intersects(TugLocal(new Vector2(side*Width(z0),z0)),TugLocal(new Vector2(side*Width(z1),z1)),tug))return false;}
            return !Intersects(TugLocal(new Vector2(-Width(stern),stern)),TugLocal(new Vector2(Width(stern),stern)),tug)&&!Intersects(TugLocal(new Vector2(-Width(bow),bow)),TugLocal(new Vector2(Width(bow),bow)),tug);
        }
        public bool SafeSweep(Vector2 from,float oldYaw,Vector2 to,float yaw,Vector3 ship,float heading,VesselDefinition vessel,Vector2 other)
        {
            if(!WaterSegmentClear(from,to))return false;
            // A swept circumscribed disk prevents two tugs cutting through each other, independent of their yaw.
            Vector2 delta=to-from;float t=delta.sqrMagnitude<.00001f?0:Mathf.Clamp01(Vector2.Dot(other-from,delta)/delta.sqrMagnitude);
            if(Vector2.Distance(from+delta*t,other)<Radius*2-.05f)return false;
            int samples=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)/.2f+Mathf.Abs(Mathf.DeltaAngle(oldYaw,yaw))/3));
            for(int i=0;i<=samples;i++)if(!HullPoseClear(Vector2.Lerp(from,to,i/(float)samples),Mathf.LerpAngle(oldYaw,yaw,i/(float)samples),ship,heading,vessel))return false;
            return true;
        }
    }
}
