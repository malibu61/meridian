using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meridian
{
    /// <summary>Public-photo exterior study of L400; 1 unit = 4 m. Not a shipyard CAD model.</summary>
    public sealed class AnadoluModelBuilder
    {
        public const float HalfLength = 28.85f, DeckLevel = 4.35f;
        private readonly Transform root;
        private readonly EnvironmentResources owner;
        private readonly Material hull, upper, deck, dark, glass, paint, yellow, red;
        private readonly List<MeshFilter> parts = new();
        private Transform portOutlet, starboardOutlet;
        private static readonly float[] WidthStations = { -28.85f,-27f,-20f,8f,16f,21f,25f,28.85f };
        private static readonly float[] WaterlineWidths = { 3.38f,3.65f,3.72f,3.76f,3.48f,2.90f,1.65f,.08f };

        public AnadoluModelBuilder(Transform parent)
        {
            root = parent;
            owner = root.GetComponent<EnvironmentResources>() ?? root.gameObject.AddComponent<EnvironmentResources>();
            hull = Paint("L400 hull grey",new Color(.49f,.54f,.56f),.25f);
            upper = Paint("L400 island grey",new Color(.60f,.64f,.65f),.28f);
            deck = Paint("L400 flight deck",new Color(.21f,.24f,.25f),.13f);
            dark = Paint("L400 apertures and machinery",new Color(.095f,.12f,.13f),.2f);
            glass = Paint("L400 bridge glazing",new Color(.025f,.085f,.11f),.75f);
            paint = Paint("L400 deck white",new Color(.85f,.87f,.84f),.15f);
            yellow = Paint("L400 safety ochre",new Color(.84f,.63f,.24f),.15f);
            red = Paint("L400 safety equipment",new Color(.70f,.10f,.065f),.18f);
        }

        public void Build()
        {
            MeshObject("L400 closed hull and flight deck",CreateHullMesh(),hull,deck,dark);
            BuildIsland(); BuildFlightDeck(); BuildStern(); BuildSideDetails(); BuildMarkings();
            Combine();
            Transform helicopter = Group("Parked S-70B",new Vector3(-1.25f,DeckLevel + .04f,-15.7f));
            helicopter.localRotation = Quaternion.Euler(0f,15f,0f);
            new SeahawkModelBuilder(helicopter).Build();
            Transform exhaust = Group("L400 engine exhaust",Vector3.zero);
            exhaust.gameObject.AddComponent<EngineExhaustSmoke>().Configure(portOutlet,starboardOutlet);
        }

        public static float WaterlineHalfWidth(float z)
        {
            for(int i=1;i<WidthStations.Length;i++) if(z<=WidthStations[i])
                return Mathf.Lerp(WaterlineWidths[i-1],WaterlineWidths[i],Mathf.InverseLerp(WidthStations[i-1],WidthStations[i],z));
            return WaterlineWidths[WaterlineWidths.Length-1];
        }

        public static float DeckHalfWidth(float z) => z < -26f ? Mathf.Lerp(3.62f,4f,Mathf.InverseLerp(-HalfLength,-26f,z))
            : z > 18f ? Mathf.Lerp(4f,3.30f,Mathf.InverseLerp(18f,HalfLength,z)) : 4f;

        public static float FlightDeckHeight(float x,float z)
        {
            float t = Mathf.InverseLerp(17.5f,HalfLength,z);
            return DeckLevel + 1.2f * t * t * (1f - Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(1.4f,2.3f,x)));
        }

        private static Vector3[] Ring(float z)
        {
            float w=WaterlineHalfWidth(z), top=DeckHalfWidth(z), bottom=w*.66f;
            Vector3 Top(float x) => new(x,FlightDeckHeight(x,z),z);
            return new[] {
                new Vector3(-bottom,-2.5f,z),new Vector3(bottom,-2.5f,z),new Vector3(w,-.82f,z),
                new Vector3(w*1.025f,1.1f,z),Top(top),Top(2.3f),Top(1.4f),Top(0f),Top(-2f),Top(-top),
                new Vector3(-w*1.025f,1.1f,z),new Vector3(-w,-.82f,z)
            };
        }

        public static Mesh CreateHullMesh()
        {
            const int sections=97;
            List<Vector3> vertices=new(); List<int>[] triangles={new(),new(),new()};
            Vector3[] previous=Ring(-HalfLength);
            for(int row=1;row<sections;row++)
            {
                Vector3[] next=Ring(Mathf.Lerp(-HalfLength,HalfLength,row/(float)(sections-1)));
                for(int side=0;side<previous.Length;side++)
                {
                    int after=(side+1)%previous.Length;
                    int material=side>=4 && side<=8 ? 1 : side==0 || side==1 || side==11 ? 2 : 0;
                    // Ring order is counter-clockwise in XY. Its right-hand edge normal
                    // remains outward even where the raised bow is not radially convex.
                    Vector3 edge = previous[after] - previous[side];
                    AddFace(vertices,triangles[material],new[] {previous[side],next[side],next[after],previous[after]},new Vector3(edge.y,-edge.x,0f));
                }
                previous=next;
            }
            // Triangle fans use an interior point and retain the ski-jump's non-flat front outline.
            Cap(Ring(-HalfLength),Vector3.back); Cap(Ring(HalfLength),Vector3.forward);
            void Cap(Vector3[] ring,Vector3 normal)
            {
                Vector3 center=new(0f,.2f,ring[0].z);
                for(int i=0;i<ring.Length;i++)AddFace(vertices,triangles[0],new[] {center,ring[i],ring[(i+1)%ring.Length]},normal);
            }
            Mesh mesh=new(){name="L400 watertight 230.8m hull",indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(vertices); mesh.subMeshCount=3;
            for(int i=0;i<3;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        private void BuildIsland()
        {
            Block("Starboard island lower",new Vector3(2.72f,5.35f,.5f),2.28f,2.05f,2f,27.5f,0f,upper);
            Block("Fore island and bridge base",new Vector3(2.62f,6.95f,9.4f),2.26f,2.04f,1.45f,7.6f,.35f,upper);
            Block("Forward bridge",new Vector3(2.60f,7.85f,10.25f),3.02f,2.85f,.65f,3.7f,.15f,hull);
            Block("Bridge roof",new Vector3(2.60f,8.22f,10.25f),3.12f,3.12f,.12f,3.9f,0f,upper);
            for(int i=0;i<8;i++) Box("Forward bridge window",new Vector3(1.30f+i*.36f,7.92f,12.04f),new Vector3(.28f,.29f,.035f),glass);
            foreach(int side in new[]{-1,1})for(int i=0;i<6;i++)
                Box("Bridge side window",new Vector3(2.60f+side*1.47f,7.92f,8.78f+i*.48f),new Vector3(.035f,.29f,.34f),glass);
            Block("Aft island control",new Vector3(2.72f,6.52f,-9.5f),2.16f,1.90f,1.45f,5.8f,.2f,upper);
            for(int i=0;i<5;i++)Box("Aft flight-control glazing",new Vector3(1.68f,6.80f,-11.4f+i*.75f),new Vector3(.035f,.32f,.50f),glass);
            foreach(float z in new[]{3.2f,-1.4f})
            {
                Block("Exhaust casing",new Vector3(2.75f,7.50f,z),1.30f,1.12f,2.30f,2.1f,.08f,hull);
                Box("Exhaust cap",new Vector3(2.75f,8.73f,z),new Vector3(1.15f,.18f,1.95f),dark);
                Transform outlet=Group(z>0f?"L400 forward exhaust outlet":"L400 aft exhaust outlet",new Vector3(2.75f,8.86f,z));
                if(z>0f)portOutlet=outlet;else starboardOutlet=outlet;
            }
            Mast(new Vector3(2.68f,8.35f,10.0f),4.5f,true);
            Mast(new Vector3(2.68f,7.35f,-9.0f),4.4f,false);
            for(int i=0;i<6;i++)Sphere("Communications dome",new Vector3(2.35f+(i%2)*.70f,7.05f,-5.8f+i*.65f),new Vector3(.44f,.48f,.44f),paint);
            for(int i=0;i<24;i++)
            {
                float z=-11.9f+i*1.02f;
                Box("Island door",new Vector3(1.565f,5.05f,z),new Vector3(.028f,.56f,.26f),i%5==0?dark:hull);
                if(i%4==0)Box("Island service glazing",new Vector3(1.55f,5.63f,z),new Vector3(.04f,.20f,.40f),glass);
            }
            // Deck crane is folded, not an animated cargo-handling system.
            Box("Flight-deck crane pedestal",new Vector3(1.15f,4.75f,14.9f),new Vector3(.62f,.8f,.72f),hull);
            Beam("Folded deck crane",new Vector3(1.15f,5.14f,14.9f),new Vector3(1.15f,5.3f,11.8f),.17f,hull);
        }

        private void Mast(Vector3 basePoint,float height,bool forward)
        {
            Block("Faceted mast",basePoint+Vector3.up*(height*.38f),1.05f,.36f,height*.76f,1.1f,.15f,hull);
            Box("Mast platform",basePoint+Vector3.up*(height*.63f),new Vector3(1.50f,.12f,1.2f),upper);
            Beam("Mast antenna",basePoint+Vector3.up*(height*.7f),basePoint+Vector3.up*height,.045f,dark);
            Box(forward?"Forward radar face":"Aft radar face",basePoint+new Vector3(0f,height*.78f,.29f),new Vector3(.86f,.22f,.36f),dark);
            Beam("Mast yard",basePoint+new Vector3(-1f,height*.63f,0f),basePoint+new Vector3(1f,height*.63f,0f),.04f,hull);
            Sphere("Mast sensor",basePoint+new Vector3(0f,height*.91f,0f),new Vector3(.32f,.25f,.32f),upper);
        }

        private void BuildFlightDeck()
        {
            // Six T-shaped landing spots, not copied F244 circular H markings.
            for(int i=0;i<6;i++)
            {
                float z=-20.4f+i*7.0f;
                DeckLine("Landing spot crossbar",new Vector2(-3.48f,z),new Vector2(.85f,z),.065f,paint);
                DeckLine("Landing spot arm",new Vector2(-1.25f,z-1.75f),new Vector2(-1.25f,z+1.75f),.065f,paint);
                DeckLine("Landing spot diagonal",new Vector2(.15f,z-1.3f),new Vector2(.85f,z),.055f,paint);
            }
            for(float z=-25f;z<27.5f;z+=1.35f)
                DeckLine("Dashed flying line",new Vector2(-1.25f,z),new Vector2(-1.25f,Mathf.Min(z+.68f,28.6f)),.055f,paint);
            for(int side=-1;side<=1;side+=2)
            {
                for(int row=0;row<92;row++)
                {
                    float z=Mathf.Lerp(-27.7f,28.4f,row/92f),next=Mathf.Lerp(-27.7f,28.4f,(row+1)/92f);
                    DeckLine("Flight-deck edge paint",new Vector2(side*(DeckHalfWidth(z)-.12f),z),new Vector2(side*(DeckHalfWidth(next)-.12f),next),.055f,yellow);
                    if(row%2==0 && (side<0 || z<-15f || z>16f))
                    {
                        Vector3 edge=new(side*DeckHalfWidth(z),FlightDeckHeight(side*DeckHalfWidth(z),z)-.07f,z);
                        Beam("Folded safety-net outrigger",edge,edge+new Vector3(side*.50f,-.14f,0f),.018f,hull);
                        Beam("Safety net edge",edge+new Vector3(side*.50f,-.14f,0f),edge+new Vector3(side*.50f,-.14f,.92f),.015f,dark);
                    }
                }
            }
            Elevator("Aft aircraft elevator",new Vector2(1.45f,-23.0f),new Vector2(3.6f,4.3f));
            Elevator("Forward aircraft elevator",new Vector2(.75f,14.2f),new Vector2(2.35f,3.25f));
            for(int row=0;row<30;row++)for(int column=0;column<5;column++)
            {
                float x=-3.25f+column*.9f,z=-24f+row*1.42f;
                Box("Deck tie-down",new Vector3(x,FlightDeckHeight(x,z)+.012f,z),new Vector3(.045f,.014f,.045f),dark);
            }
            DeckLine("Ski jump lip",new Vector2(-3.28f,28.55f),new Vector2(1.3f,28.55f),.14f,yellow);
        }

        private void Elevator(string name,Vector2 center,Vector2 size)
        {
            Box(name+" closed flush platform",new Vector3(center.x,DeckLevel+.012f,center.y),new Vector3(size.x,.015f,size.y),hull);
            Vector2 half=size*.5f;
            Vector2[] corners={center+new Vector2(-half.x,-half.y),center+new Vector2(half.x,-half.y),center+half,center+new Vector2(-half.x,half.y)};
            for(int i=0;i<4;i++)DeckLine(name+" yellow outline",corners[i],corners[(i+1)%4],.045f,yellow);
        }

        private void BuildStern()
        {
            Box("Closed well-dock stern gate",new Vector3(0f,.68f,-28.88f),new Vector3(5.15f,2.70f,.055f),dark);
            Box("Stern gate outer plating",new Vector3(0f,.68f,-28.92f),new Vector3(4.95f,2.46f,.035f),hull);
            for(int row=0;row<8;row++)Box("Stern gate horizontal seam",new Vector3(0f,-.35f+row*.30f,-28.945f),new Vector3(4.90f,.018f,.014f),dark);
            foreach(int side in new[]{-1,1})
            {
                Box("Stern service sponson",new Vector3(side*3.2f,3.25f,-27.8f),new Vector3(1.0f,1.55f,1.8f),upper);
                Box("Stern control window",new Vector3(side*3.2f,3.55f,-28.72f),new Vector3(.64f,.30f,.03f),glass);
                Box("Aft safety guard",new Vector3(side*2.3f,4.65f,-28.4f),new Vector3(1.45f,.42f,.08f),hull);
            }
        }

        private void BuildSideDetails()
        {
            foreach(int side in new[]{-1,1})
            {
                for(int i=0;i<17;i++)
                {
                    float z=-23f+i*2.75f, x=HullSideX(z,3.4f)*side;
                    Box("Hull opening surround",new Vector3(x+side*.015f,3.4f,z),new Vector3(.035f,.45f,.64f),hull);
                    Box("Hull recess dark backing",new Vector3(x+side*.037f,3.4f,z),new Vector3(.02f,.29f,.48f),dark);
                    if(i%3==0)for(int slat=0;slat<4;slat++)Box("Vent louver",new Vector3(x+side*.050f,3.30f+slat*.065f,z),new Vector3(.018f,.018f,.45f),hull);
                }
                for(int i=0;i<3;i++)
                {
                    float z=-12f+i*9f, x=side*HullSideX(z,2.10f);
                    Box("Boat bay closed backing",new Vector3(x+side*.02f,2.1f,z),new Vector3(.04f,1.10f,2.7f),dark);
                    Box("Boat davit sill",new Vector3(x+side*.20f,1.62f,z),new Vector3(.45f,.13f,2.85f),hull);
                    Sphere("Embarked utility boat",new Vector3(x+side*.20f,1.95f,z),new Vector3(.47f,.43f,2.15f),upper);
                }
                for(int i=0;i<8;i++)
                {
                    float z=-16f+i*3.7f;
                    Box("Survival raft cradle",new Vector3(side*4.12f,3.85f,z),new Vector3(.34f,.24f,.67f),paint);
                }
                Block("Forward defensive sponson",new Vector3(side*3.25f,4.5f,20.1f),1.15f,.88f,.60f,2.6f,.15f,hull);
                Block("Defensive mount silhouette",new Vector3(side*3.25f,5.05f,20.1f),.60f,.43f,.65f,.72f,.05f,upper);
                Sphere("Forward electro-optic dome",new Vector3(side*3.25f,5.5f,20.1f),new Vector3(.32f,.4f,.32f),paint);
                for(int i=0;i<3;i++)Sphere("Bow hawse recess",new Vector3(side*HullSideX(25f,1.6f)+side*.03f,1.6f-i*.17f,25f-i*.45f),new Vector3(.035f,.14f,.21f),dark);
                Box("Anchor shank",new Vector3(side*(HullSideX(24.3f,1.1f)+.04f),1.1f,24.3f),new Vector3(.06f,.47f,.10f),dark);
            }
        }

        private static float HullSideX(float z,float y) => Mathf.Lerp(WaterlineHalfWidth(z)*1.025f,DeckHalfWidth(z),Mathf.InverseLerp(1.1f,DeckLevel,y));

        private void BuildMarkings()
        {
            foreach(int side in new[]{-1,1})
            {
                HullText("L400 bow pennant",side,20.7f,2.25f,3.55f,.82f);
                HullText("L400 aft pennant",side,-25.2f,1.75f,2.8f,.65f);
            }
        }

        private void HullText(string name,int side,float centerZ,float centerY,float width,float height)
        {
            string[] glyphs={"10000/10000/10000/10000/10000/10000/11111","10010/10010/10010/11111/00010/00010/00010","01110/10001/10011/10101/11001/10001/01110","01110/10001/10011/10101/11001/10001/01110"};
            List<Vector3> vertices=new(); List<int> triangles=new();
            for(int letter=0;letter<4;letter++)
            {
                string[] rows=glyphs[letter].Split('/');
                for(int row=0;row<7;row++)for(int col=0;col<5;col++)if(rows[row][col]=='1')
                {
                    float u=(letter*6f+col)/23f,v=(6f-row)/7f;
                    Vector3 P(float a,float b)
                    {
                        float z=centerZ+side*(a-.5f)*width,y=centerY+(b-.5f)*height;
                        return HullSurfacePoint(side,z,y) + Vector3.right * side * .022f;
                    }
                    AddFace(vertices,triangles,new[]{P(u,v),P(u+1f/23f,v),P(u+1f/23f,v+1f/7f),P(u,v+1f/7f)},Vector3.right*side);
                }
            }
            Mesh mesh=new(){name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            MeshObject(name+(side<0?" port":" starboard"),mesh,paint);
        }

        public static Vector3 HullSurfacePoint(int side,float z,float y)
        {
            const int segments=96;
            float row=Mathf.Clamp((z+HalfLength)/(HalfLength*2f)*segments,0f,segments-.0001f);
            int index=Mathf.FloorToInt(row), edge=side>0?3:9;
            Vector3[] first=Ring(Mathf.Lerp(-HalfLength,HalfLength,index/(float)segments));
            Vector3[] next=Ring(Mathf.Lerp(-HalfLength,HalfLength,(index+1)/(float)segments));
            Vector3 a=first[edge],b=next[edge],c=next[(edge+1)%first.Length],d=first[(edge+1)%first.Length];
            if (Interpolate(a,b,c,out Vector3 hit)) return hit;
            Interpolate(a,c,d,out hit); return hit;
            bool Interpolate(Vector3 p,Vector3 q,Vector3 r,out Vector3 result)
            {
                float det=(q.y-r.y)*(p.z-r.z)+(r.z-q.z)*(p.y-r.y);
                float u=((q.y-r.y)*(z-r.z)+(r.z-q.z)*(y-r.y))/det;
                float v=((r.y-p.y)*(z-r.z)+(p.z-r.z)*(y-r.y))/det;
                result=p*u+q*v+r*(1f-u-v);
                return u>=-.0001f && v>=-.0001f && u+v<=1.0001f;
            }
        }

        private void DeckLine(string name,Vector2 from,Vector2 to,float width,Material material)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)*2f));
            for(int i=0;i<steps;i++)
            {
                Vector2 a=Vector2.Lerp(from,to,i/(float)steps),b=Vector2.Lerp(from,to,(i+1)/(float)steps);
                Beam(name,new Vector3(a.x,FlightDeckHeight(a.x,a.y)+.027f,a.y),new Vector3(b.x,FlightDeckHeight(b.x,b.y)+.027f,b.y),width,material);
            }
        }

        private Material Paint(string name,Color color,float gloss)
        {
            Material material=owner.Own(new Material(Shader.Find("Standard")){name=name,color=color});
            material.SetFloat("_Metallic",.025f);material.SetFloat("_Glossiness",gloss);return material;
        }

        private Transform Group(string name,Vector3 position)
        { Transform group=new GameObject(name).transform;group.SetParent(root,false);group.localPosition=position;return group; }

        private GameObject Box(string name,Vector3 position,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube)
        {
            GameObject part=GameObject.CreatePrimitive(type);part.name=name;part.transform.SetParent(root,false);
            part.transform.localPosition=position;part.transform.localScale=size;
            Collider collider=part.GetComponent<Collider>();collider.enabled=false;Object.Destroy(collider);
            part.GetComponent<MeshRenderer>().sharedMaterial=material;parts.Add(part.GetComponent<MeshFilter>());return part;
        }

        private void Sphere(string name,Vector3 position,Vector3 scale,Material material)=>Box(name,position,scale,material,PrimitiveType.Sphere);
        private void Beam(string name,Vector3 a,Vector3 b,float width,Material material)
        {
            if((b-a).sqrMagnitude<.0000001f)return;
            GameObject beam=Box(name,(a+b)*.5f,new Vector3(width,width,Vector3.Distance(a,b)),material);
            beam.transform.localRotation=Quaternion.LookRotation(b-a,Vector3.up);
        }

        private void Block(string name,Vector3 center,float bottomWidth,float topWidth,float height,float length,float sweep,Material material)
        {
            Vector3[] bottom={new(-bottomWidth/2,-height/2,-length/2),new(bottomWidth/2,-height/2,-length/2),new(bottomWidth/2,-height/2,length/2),new(-bottomWidth/2,-height/2,length/2)};
            Vector3[] top={new(-topWidth/2,height/2,-length/2),new(topWidth/2,height/2,-length/2),new(topWidth/2,height/2,length/2-sweep),new(-topWidth/2,height/2,length/2-sweep)};
            List<Vector3> v=new();List<int> t=new();
            for(int i=0;i<4;i++)AddFace(v,t,new[]{bottom[i]+center,bottom[(i+1)%4]+center,top[(i+1)%4]+center,top[i]+center},new Vector3((bottom[i].x+bottom[(i+1)%4].x),0f,(bottom[i].z+bottom[(i+1)%4].z)));
            Vector3[] up=new Vector3[4],down=new Vector3[4];for(int i=0;i<4;i++){up[i]=top[i]+center;down[i]=bottom[i]+center;}
            AddFace(v,t,up,Vector3.up);AddFace(v,t,down,Vector3.down);
            Mesh mesh=new(){name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshObject(name,mesh,material);
        }

        private static void AddFace(List<Vector3> vertices,List<int> triangles,Vector3[] points,Vector3 expected)
        {
            if(expected==Vector3.zero)
            {
                foreach(Vector3 p in points)expected+=new Vector3(p.x,p.y-1f,0f);
            }
            bool reverse=Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),expected)<0f;
            int first=vertices.Count;vertices.AddRange(points);
            for(int i=1;i<points.Length-1;i++){triangles.Add(first);triangles.Add(first+(reverse?i+1:i));triangles.Add(first+(reverse?i:i+1));}
        }

        private void MeshObject(string name,Mesh mesh,params Material[] materials)
        {
            owner.Own(mesh);GameObject go=new(name);go.transform.SetParent(root,false);
            MeshFilter filter=go.AddComponent<MeshFilter>();filter.sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials=materials;parts.Add(filter);
        }

        private void Combine()
        {
            Dictionary<Material,List<CombineInstance>> batches=new();
            foreach(MeshFilter filter in parts)
            {
                Material[] materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++)
                {
                    Material material=materials[Mathf.Min(sub,materials.Length-1)];
                    if(!batches.ContainsKey(material))batches[material]=new List<CombineInstance>();
                    batches[material].Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=sub,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                }
                filter.gameObject.SetActive(false);
            }
            foreach(KeyValuePair<Material,List<CombineInstance>> batch in batches)
            {
                Mesh mesh=owner.Own(new Mesh{name="L400 combined "+batch.Key.name,indexFormat=IndexFormat.UInt32});
                mesh.CombineMeshes(batch.Value.ToArray(),true,true);
                GameObject go=new("L400 exterior / "+batch.Key.name);go.transform.SetParent(root,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=batch.Key;
            }
        }
    }
}
