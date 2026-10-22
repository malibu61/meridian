using UnityEngine;
using UnityEngine.Rendering;
namespace Meridian
{
    public sealed class ShorelineWash:MonoBehaviour
    {
        private Mesh mesh;
        private Vector3 start,end,across;
        private Vector3[] vertices;
        private Color[] colors;
        private int rows;
        public void Configure(Vector3 from,Vector3 to,float width,Material foam)
        {
            start=from;end=to;across=Vector3.Cross((end-start).normalized,Vector3.up)*width;
            rows=Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(from,to)/1.4f)+1,8,160);
            vertices=new Vector3[rows*3];colors=new Color[vertices.Length];Vector2[] uv=new Vector2[vertices.Length];int[] indices=new int[(rows-1)*12];
            for(int i=0;i<rows;i++)for(int c=0;c<3;c++)uv[i*3+c]=new Vector2(i/(float)(rows-1)*Vector3.Distance(from,to)/2.8f,c*.5f);
            int n=0;for(int i=0;i<rows-1;i++)for(int c=0;c<2;c++)
            {int a=i*3+c,b=a+3;indices[n++]=a;indices[n++]=b;indices[n++]=a+1;indices[n++]=a+1;indices[n++]=b;indices[n++]=b+1;}
            mesh=new Mesh{name="Feathered animated shoreline wash"};mesh.MarkDynamic();mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;MeshRenderer renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=foam;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;Update();
        }
        private void Update()
        {
            if(mesh==null)return;
            for(int i=0;i<rows;i++)
            {
                Vector3 center=Vector3.Lerp(start,end,i/(float)(rows-1));float phase=Time.time*1.1f+center.x*.13f+center.z*.19f;
                float pulse=Mathf.Pow(Mathf.Max(0,Mathf.Sin(phase)),2);
                for(int c=0;c<3;c++)
                {
                    Vector3 p=center+across*((c*.5f-.5f)*(1f+pulse*.35f));p.y=WaterWaves.SurfaceY(p,Time.time)+.085f;
                    vertices[i*3+c]=transform.InverseTransformPoint(p);colors[i*3+c]=new Color(.82f,.94f,.94f,c==1?.12f+pulse*.38f:0);
                }
            }
            mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateBounds();
        }
        private void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
