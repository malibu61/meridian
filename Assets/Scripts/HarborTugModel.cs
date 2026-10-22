using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    /// <summary>Original low-poly naval harbour tug, photo-inspired by ÖNDER and Y57; not a surveyed replica.</summary>
    public sealed class HarborTugModel
    {
        private readonly Transform root;
        private readonly EnvironmentResources resources;
        private readonly List<MeshFilter> parts=new();
        private readonly Material hull,deck,upper,glass,rubber,white,orange,red;
        public HarborTugModel(Transform parent)
        {
            root=parent;resources=root.gameObject.AddComponent<EnvironmentResources>();
            hull=Paint("Naval tug grey",new Color(.37f,.43f,.46f));deck=Paint("Tug working deck",new Color(.18f,.23f,.24f));
            upper=Paint("Tug bridge grey",new Color(.60f,.65f,.67f));glass=Paint("Tug dark glazing",new Color(.025f,.09f,.13f));
            rubber=Paint("Heavy rubber fenders",new Color(.055f,.065f,.07f));white=Paint("Tug white",new Color(.88f,.9f,.87f));
            orange=Paint("Safety orange",new Color(.95f,.34f,.07f));red=Paint("Ensign red",new Color(.78f,.035f,.055f));
        }
        public void Build()
        {
            Mesh hullMesh=CreateHull();MeshPart("Closed rounded tug hull",hullMesh,hull);
            Box("Working deck",new Vector3(0,.28f,-.2f),new Vector3(2.05f,.12f,5.7f),deck);
            Box("Deckhouse",new Vector3(0,.78f,.5f),new Vector3(1.62f,.92f,2.75f),hull);
            Box("Bridge",new Vector3(0,1.57f,1.1f),new Vector3(1.86f,.72f,1.85f),upper);
            Box("Bridge visor",new Vector3(0,1.98f,1.1f),new Vector3(2.03f,.12f,2.03f),upper);
            for(int i=0;i<5;i++)Box("Forward glazing",new Vector3(-.73f+i*.365f,1.65f,2.033f),new Vector3(.28f,.38f,.025f),glass);
            foreach(int side in new[]{-1,1})
            {
                for(int i=0;i<4;i++)Box("Bridge side glazing",new Vector3(side*.936f,1.65f,.45f+i*.42f),new Vector3(.025f,.38f,.31f),glass);
                Box("Bridge access",new Vector3(side*.82f,.78f,1.1f),new Vector3(.025f,.64f,.30f),deck);
                for(int i=0;i<5;i++)
                {
                    float z=-2.7f+i*.68f;Beam("Handrail stanchion",new Vector3(side*1.05f,.39f,z),new Vector3(side*1.05f,.71f,z),.025f,upper);
                }
                Beam("Working deck handrail",new Vector3(side*1.05f,.71f,-2.7f),new Vector3(side*1.05f,.71f,-.0f),.026f,upper);
                for(int i=0;i<7;i++)Sphere("Side rubber fender",new Vector3(side*1.17f,.13f,-2.6f+i*.78f),new Vector3(.20f,.30f,.51f),rubber);
                Box("Exhaust stack",new Vector3(side*.6f,1.5f,-.35f),new Vector3(.38f,1.75f,.58f),hull);
                Box("Exhaust cap",new Vector3(side*.6f,2.40f,-.35f),new Vector3(.43f,.10f,.65f),rubber);
                Cylinder("Life ring",new Vector3(side*.835f,.88f,-.45f),new Vector3(.30f,.045f,.30f),orange,new Vector3(0,0,90));
            }
            for(int i=0;i<17;i++)
            {
                float a=i*Mathf.PI/16;Sphere("Bow pushing fender",new Vector3(Mathf.Cos(a)*1.12f,.12f,2.75f+Mathf.Sin(a)*.98f),new Vector3(.36f,.43f,.36f),rubber);
            }
            for(int i=0;i<9;i++)Sphere("Stern fender",new Vector3(-.88f+i*.22f,.12f,-3.35f),new Vector3(.32f,.30f,.36f),rubber);
            Box("Towing winch base",new Vector3(0,.44f,-1.8f),new Vector3(1.2f,.22f,.8f),rubber);
            Cylinder("Towing winch drum",new Vector3(0,.72f,-1.8f),new Vector3(.65f,.45f,.65f),upper,new Vector3(0,0,90));
            for(int i=-1;i<=1;i++)Cylinder("Winch cable turns",new Vector3(i*.13f,.72f,-1.8f),new Vector3(.69f,.025f,.69f),rubber,new Vector3(0,0,90));
            foreach(float z in new[]{-2.8f,2.8f})foreach(float x in new[]{-.62f,.62f})
            {Cylinder("Bollard",new Vector3(x,.45f,z),new Vector3(.14f,.14f,.14f),rubber);Box("Bollard cap",new Vector3(x,.60f,z),new Vector3(.27f,.07f,.16f),rubber);}
            Beam("Mainmast",new Vector3(0,2.05f,.85f),new Vector3(0,4.15f,.60f),.075f,upper);
            Beam("Mast yard",new Vector3(-.68f,3.10f,.70f),new Vector3(.68f,3.10f,.70f),.045f,upper);
            Box("Radar platform",new Vector3(0,3.65f,.64f),new Vector3(.60f,.07f,.40f),upper);
            Box("Radar scanner",new Vector3(0,3.79f,.64f),new Vector3(.81f,.10f,.14f),white);
            Sphere("Port navigation light",new Vector3(-.68f,3.13f,.70f),Vector3.one*.12f,red);
            Sphere("Starboard navigation light",new Vector3(.68f,3.13f,.70f),Vector3.one*.12f,Paint("Navigation green",new Color(.08f,.56f,.28f)));
            Beam("Aft crane pedestal",new Vector3(.6f,.4f,-2.6f),new Vector3(.6f,1.35f,-2.6f),.10f,upper);
            Beam("Aft crane boom",new Vector3(.6f,1.35f,-2.6f),new Vector3(.6f,2.35f,-1.8f),.08f,upper);
            Crew(new Vector3(-.72f,.39f,-2.3f));Crew(new Vector3(.96f,.39f,2.45f));
            // A tiny original geometric crescent/star, no third-party image assets.
            Box("Turkish ensign",new Vector3(.24f,3.32f,.72f),new Vector3(.46f,.30f,.018f),red);
            Sphere("Ensign crescent white",new Vector3(.17f,3.32f,.735f),new Vector3(.17f,.17f,.012f),white);
            Sphere("Ensign crescent cutout",new Vector3(.20f,3.34f,.746f),new Vector3(.145f,.145f,.012f),red);
            Box("Ensign star",new Vector3(.31f,3.32f,.739f),new Vector3(.065f,.065f,.015f),white,new Vector3(0,0,45));
            Combine();
        }
        public static Mesh CreateHull()
        {
            const int count=48;var vertices=new List<Vector3>();var triangles=new List<int>();
            Vector3 Ring(int index,int level)
            {
                float a=2*Mathf.PI*(index%count)/count,z=Mathf.Cos(a)*3.75f;
                float w=1.225f*Mathf.Sin(a)*(z>0?Mathf.Lerp(1,.80f,z/3.75f):1);
                return new Vector3(w*(level==0?.62f:level==1?1:1),level==0?-1.55f:level==1?-.56f:.38f,z*(level==0?.90f:1));
            }
            void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 normal)
            {int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)<0)triangles.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});else triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
            for(int i=0;i<count;i++)
            {
                for(int ring=0;ring<2;ring++){Vector3 a=Ring(i,ring),b=Ring(i+1,ring),c=Ring(i+1,ring+1),d=Ring(i,ring+1);Face(a,b,c,d,new Vector3((a.x+b.x)*.5f,0,(a.z+b.z)*.5f));}
                Vector3 top=new(0,.38f,0),bottom=new(0,-1.55f,0);
                int n=vertices.Count;vertices.AddRange(new[]{top,Ring(i,2),Ring(i+1,2),bottom,Ring(i+1,0),Ring(i,0)});
                triangles.AddRange(new[]{n,n+1,n+2,n+3,n+4,n+5});
            }
            Mesh mesh=new(){name="Closed 30m harbour tug hull"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private void Crew(Vector3 p)
        {Sphere("Deck crew torso",p+Vector3.up*.25f,new Vector3(.15f,.28f,.11f),orange);Sphere("Crew hardhat",p+Vector3.up*.43f,new Vector3(.13f,.11f,.13f),white);Beam("Crew legs",p,p+Vector3.up*.16f,.085f,rubber);}
        private Material Paint(string name,Color color)=>resources.Own(new Material(Shader.Find("Standard")){name=name,color=color});
        private void MeshPart(string name,Mesh mesh,Material material)
        {var go=new GameObject(name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=resources.Own(mesh);go.AddComponent<MeshRenderer>().sharedMaterial=material;}
        private void Primitive(PrimitiveType type,string name,Vector3 p,Vector3 s,Material m,Vector3 rotation)
        {GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=s;go.transform.localEulerAngles=rotation;var c=go.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);go.GetComponent<Renderer>().sharedMaterial=m;parts.Add(go.GetComponent<MeshFilter>());}
        private void Box(string n,Vector3 p,Vector3 s,Material m,Vector3 r=default)=>Primitive(PrimitiveType.Cube,n,p,s,m,r);
        private void Sphere(string n,Vector3 p,Vector3 s,Material m)=>Primitive(PrimitiveType.Sphere,n,p,s,m,default);
        private void Cylinder(string n,Vector3 p,Vector3 s,Material m,Vector3 r=default)=>Primitive(PrimitiveType.Cylinder,n,p,s,m,r);
        private void Beam(string n,Vector3 a,Vector3 b,float width,Material m)
        {Vector3 v=b-a;Primitive(PrimitiveType.Cylinder,n,(a+b)*.5f,new Vector3(width,v.magnitude*.5f,width),m,Quaternion.FromToRotation(Vector3.up,v).eulerAngles);}
        private void Combine()
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var part in parts){Material m=part.GetComponent<Renderer>().sharedMaterial;if(!groups.ContainsKey(m))groups[m]=new();groups[m].Add(new CombineInstance{mesh=part.sharedMesh,transform=root.worldToLocalMatrix*part.transform.localToWorldMatrix});part.gameObject.SetActive(false);}
            foreach(var group in groups){Mesh mesh=new(){name=group.Key.name+" combined"};mesh.CombineMeshes(group.Value.ToArray());MeshPart(mesh.name,mesh,group.Key);}
            foreach(var part in parts)Object.Destroy(part.gameObject);parts.Clear();
        }
    }
}
