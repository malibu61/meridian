using UnityEngine;
using UnityEngine.Rendering;
namespace Meridian
{
    public sealed class BowWaveSheet:MonoBehaviour
    {
        private const int Rows=28;
        private ShipController ship;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private readonly Vector3[] vertices=new Vector3[Rows*4];
        private readonly Color[] colors=new Color[Rows*4];
        public void Configure(ShipController controller,Material foam)
        {
            ship=controller;
            if(mesh!=null)return;
            Vector2[] uv=new Vector2[vertices.Length];int[] triangles=new int[(Rows-1)*12];int n=0;
            for(int side=0;side<2;side++)for(int row=0;row<Rows;row++)
            {
                int a=side*Rows*2+row*2;uv[a]=new Vector2(row*.28f,0);uv[a+1]=new Vector2(row*.28f,1);
                if(row==Rows-1)continue;
                triangles[n++]=a;triangles[n++]=a+2;triangles[n++]=a+1;triangles[n++]=a+1;triangles[n++]=a+2;triangles[n++]=a+3;
            }
            mesh=new Mesh{name="Diverging bow foam sheets"};mesh.MarkDynamic();mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;meshRenderer=gameObject.AddComponent<MeshRenderer>();meshRenderer.sharedMaterial=foam;meshRenderer.shadowCastingMode=ShadowCastingMode.Off;meshRenderer.receiveShadows=false;
        }
        private void LateUpdate()
        {
            if(ship==null||mesh==null)return;
            float speed=Vector3.Dot(ship.GetComponent<Rigidbody>().linearVelocity,ship.transform.forward);float strength=Mathf.Clamp01((speed-.2f)/3.0f);
            meshRenderer.enabled=strength>.01f;if(!meshRenderer.enabled)return;
            float scale=ship.Definition.EffectScale;
            for(int side=0;side<2;side++)for(int row=0;row<Rows;row++)
            {
                float t=row/(float)(Rows-1),sign=side==0?-1:1;
                float width=(.18f+t*.52f)*scale;float x=(.18f+t*6.5f)*scale;float z=ship.Definition.BowWakeZ-t*12f*scale;
                float alpha=Mathf.Sin(t*Mathf.PI)*strength*.45f;
                for(int edge=0;edge<2;edge++)
                {
                    Vector3 p=ship.transform.TransformPoint(new Vector3(sign*(x+(edge-.5f)*width),0,z));
                    p.y=WaterWaves.SurfaceY(p,Time.time)+.08f;int index=side*Rows*2+row*2+edge;
                    vertices[index]=transform.InverseTransformPoint(p);colors[index]=new Color(.83f,.95f,.96f,alpha);
                }
            }
            mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateBounds();
        }
        private void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
