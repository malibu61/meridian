using UnityEngine;
using UnityEngine.Rendering;
namespace Meridian
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class OceanSurface:MonoBehaviour
    {
        private Material oceanMaterial;
        private Mesh mesh;
        private Vector3[] baseVertices,animatedVertices,normals;
        private float[] edgeWeights;
        private Transform followTarget;
        private float gridWidth,gridLength,cell;
        public int VertexCount=>mesh==null?0:mesh.vertexCount;
        public void Follow(Transform target)=>followTarget=target;
        public void Configure(Material material,float width,float length,int resolution)
        {
            if(mesh!=null)Destroy(mesh);if(oceanMaterial!=null)Destroy(oceanMaterial);
            oceanMaterial=new Material(material);gridWidth=width;gridLength=length;
            MeshRenderer renderer=GetComponent<MeshRenderer>();renderer.sharedMaterial=oceanMaterial;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            BuildGrid(width,length,Mathf.Clamp(resolution,12,96));SetPatch();Animate();
        }
        private void Update()
        {
            if(followTarget!=null)transform.position=new Vector3(Mathf.Round(followTarget.position.x/cell)*cell,WaterWaves.SeaLevel,Mathf.Round(followTarget.position.z/cell)*cell);
            SetPatch();Animate();
        }
        private void SetPatch()=>Shader.SetGlobalVector("_MeridianPatch",new Vector4(transform.position.x,transform.position.z,gridWidth*.5f,gridLength*.5f));
        private void Animate()
        {
            if(mesh==null)return;float time=Time.time;Vector3 offset=transform.position;
            for(int i=0;i<baseVertices.Length;i++)
            {
                Vector3 p=baseVertices[i],world=p+offset;float fade=edgeWeights[i];
                animatedVertices[i]=p+Vector3.up*(WaterWaves.Height(world.x,world.z,time)*fade);
                normals[i]=Vector3.Lerp(Vector3.up,WaterWaves.Normal(world.x,world.z,time),fade).normalized;
            }
            mesh.vertices=animatedVertices;mesh.normals=normals;
            // Fixed conservative bounds: no per-frame triangle-normal or bounds rebuild.
        }
        private void BuildGrid(float width,float length,int resolution)
        {
            int row=resolution+1;cell=Mathf.Min(width,length)/resolution;
            baseVertices=new Vector3[row*row];animatedVertices=new Vector3[baseVertices.Length];normals=new Vector3[baseVertices.Length];edgeWeights=new float[baseVertices.Length];
            Vector2[] uv=new Vector2[baseVertices.Length];int[] triangles=new int[resolution*resolution*6];
            for(int z=0;z<=resolution;z++)for(int x=0;x<=resolution;x++)
            {
                int index=z*row+x;float u=x/(float)resolution,v=z/(float)resolution;
                float px=u*2-1,pz=v*2-1;
                baseVertices[index]=new Vector3(Mathf.Sign(px)*Mathf.Pow(Mathf.Abs(px),1.45f)*width*.5f,0,Mathf.Sign(pz)*Mathf.Pow(Mathf.Abs(pz),1.45f)*length*.5f);uv[index]=new Vector2(u,v);
                float edge=Mathf.Max(Mathf.Abs(u-.5f),Mathf.Abs(v-.5f))*2;
                // Mathf.SmoothStep interpolates endpoints; unlike HLSL it does not take thresholds.
                // Keep the entire central region at full physical height and fade only at the rim.
                edgeWeights[index]=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.76f,1,edge));
            }
            int t=0;for(int z=0;z<resolution;z++)for(int x=0;x<resolution;x++)
            {int i=z*row+x;triangles[t++]=i;triangles[t++]=i+row;triangles[t++]=i+1;triangles[t++]=i+1;triangles[t++]=i+row;triangles[t++]=i+row+1;}
            mesh=new Mesh{name="Layered ocean wave patch"};mesh.MarkDynamic();mesh.vertices=baseVertices;mesh.uv=uv;mesh.triangles=triangles;
            mesh.bounds=new Bounds(Vector3.zero,new Vector3(width+1,4,length+1));GetComponent<MeshFilter>().sharedMesh=mesh;
        }
        private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(oceanMaterial!=null)Destroy(oceanMaterial);}
    }
}
