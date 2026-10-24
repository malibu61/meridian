using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    public enum HarborCrewPose{Watch,Walk,Throw,Receive,Haul,Secure}
    /// <summary>One low-poly mesh, five material slots per person; all joints animate without per-frame allocations.</summary>
    public sealed class HarborCrewMember:MonoBehaviour
    {
        private const int Boxes=24,VerticesPerBox=24;
        private readonly Vector3[] vertices=new Vector3[Boxes*VerticesPerBox],normals=new Vector3[Boxes*VerticesPerBox];
        private Mesh mesh;private int box;private bool worker;private float phase;
        private static readonly Vector3[] Corners={new(-1,-1,-1),new(1,-1,-1),new(1,1,-1),new(-1,1,-1),new(-1,-1,1),new(1,-1,1),new(1,1,1),new(-1,1,1)};
        private static readonly int[] Faces={4,5,6,7,1,0,3,2,0,4,7,3,5,1,2,6,3,7,6,2,0,1,5,4};
        private static readonly Vector3[] FaceNormals={Vector3.forward,Vector3.back,Vector3.left,Vector3.right,Vector3.up,Vector3.down};
        private readonly List<int>[] indices={new(),new(),new(),new(),new()};
        public Vector3 LeftHand{get;private set;}public Vector3 RightHand{get;private set;}
        public int PoseFrames{get;private set;}
        public void Build(Material[] palette,bool lineHandler,bool officer,float offset,EnvironmentResources resources)
        {
            worker=lineHandler;phase=offset;mesh=resources.Own(new Mesh{name="Articulated naval harbour person"});mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterials=palette;
            // Material indices are assigned in exactly the same box order as SamplePose.
            int[] paints={0,0,1,1,1,0,0,0,0,0,0,0,0,worker?4:0,2,2,worker?4:0,1,3,3,3,3,2,2};
            for(int b=0;b<Boxes;b++)for(int f=0;f<6;f++){int v=b*24+f*4;indices[paints[b]].AddRange(new[]{v,v+1,v+2,v,v+2,v+3});}
            mesh.subMeshCount=5;SamplePose(0,HarborCrewPose.Watch,0);for(int i=0;i<5;i++)mesh.SetTriangles(indices[i],i);mesh.bounds=new Bounds(new Vector3(0,.24f,0),new Vector3(.5f,.65f,.6f));
            // White service uniform with simplified shoulder/slip-on details, not certified rank insignia.
            RankBars=officer?2:1;
        }
        public int RankBars{get;private set;}
        public void SamplePose(float time,HarborCrewPose pose,float progress)
        {
            if(mesh==null)return;box=0;float cycle=time*7+phase,step=pose==HarborCrewPose.Walk?Mathf.Sin(cycle):0;
            float pull=pose==HarborCrewPose.Haul?Mathf.Sin(time*3.8f+phase):0;
            float lean=pose==HarborCrewPose.Haul?-5+pull*4:pose==HarborCrewPose.Throw?Mathf.Sin(progress*Mathf.PI)*-10:0;
            Quaternion bodyTurn=Quaternion.Euler(lean,0,0);Vector3 hips=new(0,.215f,0);
            Box(hips+new Vector3(0,.075f,0),new(.108f,.15f,.064f),bodyTurn); // jacket
            Box(hips,new(.096f,.065f,.067f),Quaternion.identity);
            Box(hips+new Vector3(0,-.001f,0),new(.10f,.014f,.071f),Quaternion.identity);
            // Shoes, articulated legs and upper/lower arms.
            for(int side=-1;side<=1;side+=2)Box(new Vector3(side*.031f,.019f,.019f+side*step*.035f),new(.038f,.027f,.075f),Quaternion.identity);
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 hip=new(side*.031f,.20f,0),knee=new(side*.032f,.108f,-side*step*.030f),ankle=new(side*.031f,.035f,side*step*.035f);
                Segment(hip,knee,.040f,.042f);Segment(knee,ankle,.034f,.036f);
            }
            Vector3 leftHand=Hand(-1,pose,progress,step,pull),rightHand=Hand(1,pose,progress,step,pull);
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 shoulder=new(side*.062f,.345f,0),hand=side<0?leftHand:rightHand;
                Vector3 elbow=(shoulder+hand)*.5f+new Vector3(side*.025f,0,-.025f);
                Segment(shoulder,elbow,.032f,.035f);Segment(elbow,hand,.029f,.031f);
            }
            Box(new(0,.29f,.006f),worker?new Vector3(.115f,.115f,.081f):new Vector3(.001f,.001f,.001f),bodyTurn);
            Quaternion headTurn=Quaternion.Euler(0,pose==HarborCrewPose.Watch?Mathf.Sin(time*.55f+phase)*32:Mathf.Sin(time*.8f+phase)*5,0);
            Box(new(0,.397f,0),new(.061f,.069f,.060f),headTurn);
            Box(new(0,.393f,.033f),new(.014f,.017f,.015f),headTurn);
            Box(new(0,.433f,0),new(.070f,.025f,.070f),headTurn);
            Box(new(0,.424f,.025f),new(.069f,.008f,.040f),headTurn);
            Box(new(-.040f,.366f,0),new(.035f,.008f,.041f),Quaternion.identity);
            Box(new(.040f,.366f,0),new(.035f,.008f,.041f),Quaternion.identity);
            Box(new(-.040f,.372f,0),new(.007f,.004f,RankBars>1?.037f:.001f),Quaternion.identity);
            Box(new(.040f,.372f,0),new(.007f,.004f,RankBars>1?.037f:.001f),Quaternion.identity);
            Box(leftHand,new(.030f,.030f,.037f),Quaternion.identity);Box(rightHand,new(.030f,.030f,.037f),Quaternion.identity);
            LeftHand=transform.TransformPoint(leftHand);RightHand=transform.TransformPoint(rightHand);
            mesh.vertices=vertices;mesh.normals=normals;PoseFrames++;
        }
        private static Vector3 Hand(int side,HarborCrewPose pose,float progress,float step,float pull)
        {
            if(pose==HarborCrewPose.Throw){float a=Mathf.SmoothStep(0,1,progress);return new(side*.073f,.29f+Mathf.Sin(a*Mathf.PI)*.17f,Mathf.Lerp(-.12f,.16f,a));}
            if(pose==HarborCrewPose.Receive)return new(side*.060f,.31f,.16f);
            if(pose==HarborCrewPose.Haul)return new(side*.055f,.29f+pull*.013f,.10f+pull*.055f);
            if(pose==HarborCrewPose.Secure)return new(side*.065f,.255f,.055f);
            return new(side*.073f,.225f,side*step*.052f);
        }
        private void Segment(Vector3 a,Vector3 b,float width,float depth)=>Box((a+b)*.5f,new(width,Vector3.Distance(a,b),depth),Quaternion.FromToRotation(Vector3.up,b-a));
        private void Box(Vector3 center,Vector3 size,Quaternion rotation)
        {
            int start=box++*24;for(int f=0;f<6;f++)for(int v=0;v<4;v++){vertices[start+f*4+v]=center+rotation*Vector3.Scale(Corners[Faces[f*4+v]],size*.5f);normals[start+f*4+v]=rotation*FaceNormals[f];}
        }
    }
}
