using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    /// <summary>Closed public-reference exterior hulls, at the project's 1:4 scale.</summary>
    public static class SmallCraftHull
    {
        public const int Sections=72;
        private static readonly float[] Stations={-1,-.85f,-.6f,-.2f,.2f,.5f,.72f,.87f,.96f,1};
        private static readonly float[] RuzgarWidths={.76f,.87f,.96f,1,1,.89f,.64f,.38f,.13f,.012f};
        private static readonly float[] KumkaleWidths={.76f,.9f,.99f,1,.98f,.91f,.73f,.48f,.22f,.012f};
        public static float HalfLength(VesselId id)=>VesselCatalog.Get(id).LengthMetres/8;
        public static float HalfWidth(VesselId id,float z)
        {
            float s=z/HalfLength(id);float[] widths=id==VesselId.Ruzgar?RuzgarWidths:KumkaleWidths;
            for(int i=1;i<Stations.Length;i++)if(s<=Stations[i])return Mathf.Lerp(widths[i-1],widths[i],Mathf.InverseLerp(Stations[i-1],Stations[i],s))*VesselCatalog.Get(id).BeamMetres/8;
            return widths[widths.Length-1]*VesselCatalog.Get(id).BeamMetres/8;
        }
        public static float DeckHeight(VesselId id,float z)
        {
            float s=z/HalfLength(id);
            return id==VesselId.Ruzgar?-.33f+.37f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.4f,1,s)):-.25f+.4f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.15f,1,s));
        }
        private static Vector3[] Ring(VesselId id,int station)
        {
            float h=HalfLength(id),z=Mathf.Lerp(-h,h,station/(float)Sections),w=HalfWidth(id,z),y=DeckHeight(id,z);
            float rake=Mathf.SmoothStep(0,.22f,Mathf.InverseLerp(h*.84f,h,z));
            float keel=-.82f-(id==VesselId.Ruzgar?2.68f:2.6f)/4;
            return new[]{new Vector3(-w,y,z),new Vector3(w,y,z),new Vector3(w*.86f,-.86f,z-rake),new Vector3(w*.42f,keel,z-rake*1.65f),new Vector3(-w*.42f,keel,z-rake*1.65f),new Vector3(-w*.86f,-.86f,z-rake)};
        }
        public static Mesh CreateMesh(VesselId id)
        {
            List<Vector3> vertices=new();List<int>[] triangles={new(),new(),new()};
            Vector3[] directions={Vector3.up,Vector3.right,new(1,-1,0),Vector3.down,new(-1,-1,0),Vector3.left};
            for(int i=0;i<Sections;i++)
            {
                Vector3[] a=Ring(id,i),b=Ring(id,i+1);
                for(int e=0;e<6;e++)Face(vertices,triangles[e==0?1:e>=2&&e<=4?2:0],new[]{a[e],b[e],b[(e+1)%6],a[(e+1)%6]},directions[e]);
            }
            Face(vertices,triangles[0],Ring(id,0),Vector3.back);Face(vertices,triangles[0],Ring(id,Sections),Vector3.forward);
            Mesh mesh=new(){name=id+" sealed hull",subMeshCount=3};mesh.SetVertices(vertices);
            for(int i=0;i<3;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static Vector3 SurfacePoint(VesselId id,int side,float z,float y)
        {
            int candidate=Mathf.Clamp(Mathf.FloorToInt((z/HalfLength(id)+1)*.5f*Sections),0,Sections-1);
            for(int i=Mathf.Max(0,candidate-1);i<=Mathf.Min(Sections-1,candidate+1);i++)
            {
                Vector3[] a=Ring(id,i),b=Ring(id,i+1);int top=side>0?1:0,chine=side>0?2:5;
                if(SampleTriangle(a[top],b[top],b[chine],y,z,out Vector3 p)||SampleTriangle(a[top],b[chine],a[chine],y,z,out p))return p;
            }
            float factor=Mathf.Lerp(.86f,1,Mathf.InverseLerp(-.86f,DeckHeight(id,z),y));return new Vector3(side*HalfWidth(id,z)*factor,y,z);
        }
        private static bool SampleTriangle(Vector3 a,Vector3 b,Vector3 c,float y,float z,out Vector3 point)
        {
            Vector2 u=new(b.y-a.y,b.z-a.z),v=new(c.y-a.y,c.z-a.z),p=new(y-a.y,z-a.z);
            float det=u.x*v.y-u.y*v.x,beta=(p.x*v.y-p.y*v.x)/det,gamma=(u.x*p.y-u.y*p.x)/det;
            point=new Vector3(a.x+beta*(b.x-a.x)+gamma*(c.x-a.x),y,z);return beta>=-.0001f&&gamma>=-.0001f&&beta+gamma<=1.0001f;
        }
        internal static void Face(List<Vector3> vertices,List<int> triangles,Vector3[] points,Vector3 outward)
        {
            bool flip=Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),outward)<0;
            int start=vertices.Count;vertices.AddRange(points);
            for(int i=1;i<points.Length-1;i++){triangles.Add(start);triangles.Add(start+(flip?i+1:i));triangles.Add(start+(flip?i:i+1));}
        }
    }
}
