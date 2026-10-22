using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Meridian
{
    /// <summary>Separate Ruzgar/Tuzla layouts from public photographs, not a construction drawing.</summary>
    public sealed class SmallCraftModelBuilder
    {
        private readonly Transform root;
        private readonly VesselDefinition vessel;
        private readonly EnvironmentResources owner;
        private readonly List<MeshFilter> staticParts=new();
        private readonly Material hull,upper,deck,dark,glass,white,rubber,red,metal,rope,uniform,skin,vest;
        private bool Attack=>vessel.HasFourEngines;
        public SmallCraftModelBuilder(Transform parent,VesselDefinition definition)
        {
            root=parent;vessel=definition;owner=root.gameObject.AddComponent<EnvironmentResources>();
            hull=Paint("hull naval grey",new(.39f,.44f,.46f));upper=Paint("superstructure satin grey",new(.56f,.60f,.61f));
            deck=Paint("nonslip deck",new(.20f,.25f,.26f));dark=Paint("machinery and recesses",new(.075f,.10f,.115f));
            glass=Paint("opaque dark bridge glazing",new(.035f,.075f,.09f),.68f);white=Paint("stencil and radomes",new(.83f,.87f,.85f));
            rubber=Paint("RHIB rubber",new(.11f,.14f,.15f));red=Paint("safety red",new(.72f,.16f,.09f));
            metal=Paint("propeller bronze",new(.40f,.32f,.17f),.4f);metal.SetFloat("_Metallic",.45f);
            rope=Paint("mooring rope",new(.45f,.39f,.27f));uniform=Paint("navy working blue",new(.08f,.13f,.20f));
            skin=Paint("crew skin",new(.53f,.36f,.25f));vest=Paint("deck safety vest",new(.76f,.59f,.15f));
        }
        public void Build()
        {
            Mesh hullMesh=SmallCraftHull.CreateMesh(vessel.Id);MeshObject("Closed hull and deck",hullMesh,new[]{hull,deck,dark});
            if(Attack)BuildRuzgar();else BuildKumkale();
            BuildRails();BuildMooring();BuildMarkings();BuildExhaust();BuildUnderbody();Combine();
            root.gameObject.AddComponent<SmallCraftDeckLife>().Build(vessel,uniform,skin,vest,dark);
        }
        private void BuildRuzgar()
        {
            Housing("Bridge",new(0,.12f,.5f),new(1.46f,1f,4.7f),new(1.32f,1f,4.42f),upper,true);
            Box("Bridge roof cap",new(0,.65f,.5f),new(1.42f,.055f,4.55f),hull);
            Housing("Aft machinery casing",new(0,-.12f,-2.35f),new(1.12f,.42f,1.25f),new(.95f,.42f,1.05f),hull);
            Gun("76 mm forward mount",new(0,Deck(4.7f),4.7f),false,false,1);
            Gun("35 mm aft twin mount",new(0,Deck(-5.9f),-5.9f),true,true,.80f);
            Group("Lattice mast",new(0,.65f,-.72f));
            for(int tier=0;tier<5;tier++)
            {
                float y=.68f+tier*.43f,w=Mathf.Lerp(.31f,.19f,tier/5f),wn=Mathf.Lerp(.31f,.19f,(tier+1)/5f);
                for(int corner=0;corner<4;corner++)
                {
                    int sx=corner%2==0?-1:1,sz=corner<2?-1:1;
                    Vector3 a=new(sx*w,y,-.72f+sz*w),b=new(sx*wn,y+.43f,-.72f+sz*wn),c=new(-sx*wn,y+.43f,-.72f+sz*wn);
                    Rod("Mast upright",a,b,.021f,hull);Rod("Mast cross brace",a,c,.013f,hull);
                    Rod("Mast transverse",a,new Vector3(-sx*w,y,-.72f+sz*w),.014f,hull);
                }
            }
            Cylinder("Radome pedestal",new(0,2.87f,-.72f),new(.28f,.14f,.28f),upper);
            Sphere("WM-style spherical radome",new(0,3.25f,-.72f),new(.74f,.82f,.74f),upper);
            Rod("Mast peak",new(0,3.6f,-.72f),new(0,3.96f,-.72f),.017f,hull);
            Rod("Mast yardarm",new(-.65f,2.48f,-.72f),new(.65f,2.48f,-.72f),.022f,hull);
            for(int side=-1;side<=1;side+=2)
            {
                Rod("Radio whip",new(side*.6f,.67f,1.45f),new(side*.6f,1.8f,1.45f),.009f,hull);
                Ladder(new(side*.35f,.66f,-1.02f),new(side*.22f,2.66f,-1.02f),.11f);
                for(int i=0;i<3;i++)
                {
                    float z=-3.65f+(i-1)*.43f;Vector3 a=new(side*.27f,.03f,z),b=a+new Vector3(side*.87f,.45f,.18f)*1.03f;
                    if(i==0)Group(side<0?"Port canister bank":"Starboard canister bank",a);
                    Rod("Sealed external canister",a,b,.145f,hull);
                    Vector3 direction=(b-a).normalized;
                    Rod("Canister end cap",b-direction*.035f,b,.16f,upper);
                    for(int band=1;band<=2;band++){Vector3 p=Vector3.Lerp(a,b,band/3f);Rod("Canister retaining band",p-direction*.018f,p+direction*.018f,.164f,upper);}
                    Box("Canister support foot",new(side*.49f,-.13f,z),new(.45f,.28f,.11f),dark);
                }
                LifeRing(new(side*.721f,.12f,.1f),Vector3.right*side);
                Door(new(side*.705f,-.03f,1.16f),Vector3.right*side,-.07f);
                for(int i=0;i<4;i++)Box("Machinery louvre",new(side*.566f,-.07f,-2.5f+i*.10f),new(.018f,.18f,.025f),dark);
            }
            Radar(new(0,.72f,1.35f),.72f,.9f);
            Rhib(new(-.67f,.14f,-2.55f),.68f);
            Box("Aft equipment locker",new(0,-.08f,-4.74f),new(.68f,.42f,.52f),upper);
            for(int side=-1;side<=1;side+=2)SmallMount(new(side*.73f,.70f,-1.05f),side);
        }
        private void BuildKumkale()
        {
            Housing("Sloping enclosed forecastle",new(0,.075f,1.1f),new(1.83f,.85f,6.20f),new(1.54f,.85f,4.50f),upper);
            Housing("Bridge",new(0,.95f,.15f),new(1.76f,.9f,3.05f),new(1.85f,.9f,3.22f),upper,true);
            Box("Bridge roof overhang",new(0,1.43f,.15f),new(1.96f,.065f,3.32f),hull);
            Gun("Twin 40 mm forward mount",new(0,Deck(5.05f),5.05f),true,false,1.02f);
            Housing("Solid sensor mast",new(0,2.3f,-.68f),new(.40f,1.65f,.52f),new(.19f,1.65f,.26f),hull);
            Rod("Mast upper pole",new(0,3.11f,-.68f),new(0,4.04f,-.68f),.022f,hull);
            Rod("Navigation light cross tree",new(-.73f,2.83f,-.68f),new(.73f,2.83f,-.68f),.028f,hull);
            Rod("Upper mast cross tree",new(-.42f,3.52f,-.68f),new(.42f,3.52f,-.68f),.021f,hull);
            for(int side=-1;side<=1;side+=2)
            {
                Rod("Mast aerial",new(side*.42f,3.52f,-.68f),new(side*.42f,3.87f,-.68f),.011f,upper);
                Rod("Mast cross-tree aerial",new(side*.73f,2.83f,-.68f),new(side*.73f,3.22f,-.68f),.017f,white);
                Cylinder("Satellite dome pedestal",new(side*.68f,1.6f,.15f),new(.09f,.25f,.09f),hull);
                Sphere("Satellite dome",new(side*.68f,1.82f,.15f),new(.29f,.35f,.29f),white);
                Box("Bridge wing platform",new(side*1.02f,.63f,-.50f),new(.33f,.07f,1.03f),hull);
                Rod("Wing outer safety rail",new(side*1.13f,.92f,-1.0f),new(side*1.13f,.92f,.02f),.012f,hull);
                for(int step=0;step<4;step++)Box("Side boarding steps",new(side*.96f,.1f+step*.13f,-1.45f+step*.07f),new(.24f,.03f,.13f),deck);
                LifeRing(new(side*.823f,.31f,-1.25f),Vector3.right*side);
                Door(new(side*.83f,.15f,-.61f),Vector3.right*side,-.171f);
                SmallMount(new(side*.96f,.66f,-1.04f),side);
            }
            Radar(new(0,3.02f,-.68f),.51f,.83f);
            Cylinder("Forward sensor dome support",new(-.49f,1.65f,.90f),new(.08f,.35f,.08f),hull);
            Sphere("Forward sensor dome",new(-.49f,1.97f,.90f),new(.35f,.43f,.35f),upper);
            Rhib(new(.48f,Deck(-3.25f)+.14f,-3.25f),1.0f);
            Group("Boat davit",new(-.63f,0,-3.3f));
            Rod("Davit pedestal",new(-.65f,-.21f,-3.32f),new(-.65f,.50f,-3.32f),.10f,hull);
            Rod("Davit articulated boom",new(-.65f,.48f,-3.32f),new(.12f,.78f,-3.32f),.065f,upper);
            Rod("Davit cable",new(.12f,.78f,-3.32f),new(.12f,.12f,-3.32f),.007f,dark);
            Group("Aft patrol equipment",new(0,0,-5.1f));
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
            {
                Vector3 p=new(side*.69f,-.08f,-4.7f-i*.31f);
                Rod("Stern cylindrical stores",p-Vector3.right*.11f,p+Vector3.right*.11f,.15f,hull);
                Box("Stern store rack",p+Vector3.down*.12f,new(.34f,.04f,.24f),dark);
            }
            Box("Aft centre equipment foundation",new(0,-.06f,-4.28f),new(.48f,.35f,.42f),hull);
            for(int column=0;column<3;column++)for(int row=0;row<2;row++)
            {Vector3 p=new((column-1)*.11f,.12f+row*.10f,-4.22f);Rod("Six-tube deck equipment",p,p+new Vector3(0,.12f,-.35f),.075f,dark);}
        }
        private float Deck(float z)=>SmallCraftHull.DeckHeight(vessel.Id,z);
        private void BuildRails()
        {
            float half=SmallCraftHull.HalfLength(vessel.Id);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<42;i++)
            {
                float z=Mathf.Lerp(-half+.13f,half-.23f,i/42f),next=Mathf.Lerp(-half+.13f,half-.23f,(i+1)/42f);
                Vector3 a=new(side*(SmallCraftHull.HalfWidth(vessel.Id,z)-.04f),Deck(z)+.01f,z),b=new(side*(SmallCraftHull.HalfWidth(vessel.Id,next)-.04f),Deck(next)+.01f,next);
                // Open working-deck rail, not a solid wall hiding equipment.
                Rod("Deck stanchion",a,a+Vector3.up*.29f,.012f,hull);
                for(int bar=1;bar<=2;bar++)Rod("Safety rail",a+Vector3.up*(bar*.145f),b+Vector3.up*(bar*.145f),.009f,hull);
            }
            float rear=-half+.13f,w=SmallCraftHull.HalfWidth(vessel.Id,rear)-.04f;
            for(int bar=1;bar<=2;bar++)Rod("Transom safety rail",new(-w,Deck(rear)+bar*.145f,rear),new(w,Deck(rear)+bar*.145f,rear),.009f,hull);
            Rod("Bow jackstaff",new(0,Deck(half-.2f),half-.2f),new(0,Deck(half-.2f)+.75f,half-.2f),.012f,hull);
            Rod("Stern ensign staff",new(0,Deck(-half+.14f),-half+.14f),new(0,.70f,-half+.14f),.017f,hull);
        }
        private void BuildMooring()
        {
            float half=SmallCraftHull.HalfLength(vessel.Id);
            foreach(float z in new[]{half-1.0f,half-1.55f,-half+.68f,-half+1.5f})for(int side=-1;side<=1;side+=2)
            {
                float x=side*SmallCraftHull.HalfWidth(vessel.Id,z)*.68f,y=Deck(z);
                Box("Mooring bollard base",new(x,y+.025f,z),new(.20f,.05f,.17f),dark);
                for(int p=-1;p<=1;p+=2){Cylinder("Bollard",new(x,y+.10f,z+p*.055f),new(.055f,.13f,.055f),hull);Box("Bollard head",new(x,y+.16f,z+p*.055f),new(.10f,.025f,.055f),hull);}
                Ring("Coiled mooring line",new(x-side*.17f,y+.035f,z),Vector3.up,.085f,.013f,rope);
                Ring("Coiled mooring line",new(x-side*.17f,y+.035f,z),Vector3.up,.061f,.013f,rope);
            }
            float bow=half-.66f;
            Cylinder("Anchor capstan",new(0,Deck(bow)+.12f,bow),new(.18f,.23f,.18f),hull);
            Rod("Anchor chain",new(0,Deck(bow)+.03f,bow+.12f),new(0,Deck(half-.15f)+.025f,half-.15f),.017f,dark);
            foreach(int side in new[]{-1,1})
            {
                Vector3 a=SmallCraftHull.SurfacePoint(vessel.Id,side,half-1.0f,-.37f);a.x+=side*.014f;
                Box("Anchor hawse recess",a,new(.018f,.13f,.20f),dark);
                Rod("Anchor shank",a+Vector3.up*.03f,a-Vector3.up*.12f,.023f,hull);
                Rod("Anchor flukes",a+new Vector3(0,-.12f,-.08f),a+new Vector3(0,-.12f,.08f),.026f,hull);
            }
            for(int i=0;i<5;i++)
            {float z=-1.1f+i*.35f;Box("Ventilation grille",new(0,Attack?.682f:.536f,z),new(.36f,.025f,.18f),dark);}
        }
        private void BuildMarkings()
        {
            foreach(int side in new[]{-1,1})Pennant(side,vessel.Pennant.Replace("-",""),Attack?4.8f:4.65f,-.38f,Attack?1.15f:1.43f,Attack?.24f:.28f);
        }
        private void Pennant(int side,string text,float z,float y,float width,float height)
        {
            List<Vector3> v=new();List<int> t=new();
            for(int letter=0;letter<text.Length;letter++)
            {
                string glyph=text[letter] switch
                {
                    'P'=>"11110/10001/10001/11110/10000/10000/10000",'0'=>"01110/10001/10011/10101/11001/10001/01110",
                    '1'=>"00100/01100/00100/00100/00100/00100/01110",'2'=>"01110/10001/00001/00010/00100/01000/11111",
                    '3'=>"11110/00001/00001/01110/00001/00001/11110",'4'=>"10010/10010/10010/11111/00010/00010/00010",_=>"00000/00000/00000/00000/00000/00000/00000"
                };
                string[] rows=glyph.Split('/');
                for(int row=0;row<7;row++)for(int column=0;column<5;column++)if(rows[row][column]=='1')for(int sx=0;sx<2;sx++)for(int sy=0;sy<2;sy++)
                {
                    Vector3 Point(float u,float h)
                    {Vector3 p=SmallCraftHull.SurfacePoint(vessel.Id,side,z+side*(u-.5f)*width,y+(h-.5f)*height);p.x+=side*.012f;return p;}
                    float u0=(letter*6f+column+sx*.5f)/(text.Length*6-1),u1=u0+.5f/(text.Length*6-1),v0=(6-row+sy*.5f)/7f,v1=v0+.5f/7;
                    SmallCraftHull.Face(v,t,new[]{Point(u0,v0),Point(u1,v0),Point(u1,v1),Point(u0,v1)},Vector3.right*side);
                }
            }
            Mesh m=new(){name=vessel.Pennant+" surface-conforming stencil"};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();
            MeshObject((side<0?"Port":"Starboard")+" pennant",m,new[]{white});
        }
        private void BuildExhaust()
        {
            Transform left=Group("Port exhaust outlet",new(-.37f,Attack?-.27f:-.16f,Attack?-2.4f:-2.0f));
            Transform right=Group("Starboard exhaust outlet",new(.37f,Attack?-.27f:-.16f,Attack?-2.4f:-2.0f));
            foreach(Transform outlet in new[]{left,right})Box("Low exhaust grille",outlet.localPosition,new(.23f,.035f,.28f),dark);
            root.gameObject.AddComponent<EngineExhaustSmoke>().Configure(left,right);
        }
        private void BuildUnderbody()
        {
            SmallCraftMotion motion=root.gameObject.AddComponent<SmallCraftMotion>();List<Transform> props=new(),rudders=new();
            float stern=-SmallCraftHull.HalfLength(vessel.Id);
            for(int i=0;i<vessel.EngineCount;i++)
            {
                float x=vessel.EngineLocalX(i);Transform p=Group("Propeller "+i,new(x,-1.13f,stern-.08f));props.Add(p);
                Rod("Exposed shaft",new(x,-1.13f,stern+.85f),new(x,-1.13f,stern-.10f),.045f,metal);
                Part("Propeller hub",p,Vector3.zero,new(.10f,.18f,.10f),metal,PrimitiveType.Cylinder,false).transform.localRotation=Quaternion.Euler(90,0,0);
                for(int blade=0;blade<3;blade++)
                {
                    float a=blade*120;Vector3 direction=Quaternion.Euler(0,0,a)*Vector3.up;
                    GameObject b=Part("Propeller blade",p,direction*.105f,new(.095f,.23f,.024f),metal,PrimitiveType.Sphere,false);b.transform.localRotation=Quaternion.Euler(0,22,a);
                }
            }
            foreach(int side in new[]{-1,1})
            {
                Transform r=Group("Rudder "+side,new(side*.48f,-1.17f,stern-.24f));rudders.Add(r);
                Part("Rudder blade",r,Vector3.zero,new(.025f,.32f,.25f),hull,PrimitiveType.Cube,false);
            }
            motion.Configure(props.ToArray(),rudders.ToArray());
        }
        private void Gun(string name,Vector3 position,bool twin,bool aft,float scale)
        {
            Group(name,position);Quaternion yaw=Quaternion.Euler(0,aft?180:0,0);
            Vector3 P(float x,float y,float z)=>position+yaw*(new Vector3(x,y,z)*scale);
            Cylinder(name+" circular foundation",P(0,.05f,0),new Vector3(.76f,.10f,.76f)*scale,hull);
            if(twin)
            {
                Box("Twin-gun closed rear housing",P(0,.30f,-.09f),new Vector3(.43f,.43f,.45f)*scale,dark);
                foreach(int side in new[]{-1,1})Sphere("Rounded split gun shield",P(side*.18f,.35f,0),new Vector3(.40f,.64f,.64f)*scale,upper);
            }
            else Sphere("76 mm compact turret shield",P(0,.30f,0),new Vector3(.67f,.58f,.68f)*scale,upper);
            for(int barrel=0;barrel<(twin?2:1);barrel++)
            {
                float x=twin?(barrel==0?-.075f:.075f):0;
                Rod("Gun recoil sleeve",P(x,.36f,.12f),P(x,.38f,.48f),.054f*scale,hull);
                Rod("Gun barrel",P(x,.38f,.44f),P(x,.43f,twin?1.03f:1.28f),.025f*scale,hull);
                Rod("Dark muzzle",P(x,.43f,(twin?1.03f:1.28f)-.035f),P(x,.43f,twin?1.03f:1.28f),.029f*scale,dark);
            }
            Ring("Gun mount retaining ring",P(0,.112f,0),Vector3.up,.34f*scale,.014f*scale,upper);
        }
        private void Rhib(Vector3 p,float s)
        {
            Group("Rigid inflatable boat",p);
            foreach(int side in new[]{-1,1})Rod("RHIB inflatable sponson",p+new Vector3(side*.18f,.06f,-.55f)*s,p+new Vector3(side*.18f,.06f,.48f)*s,.12f*s,rubber);
            Sphere("RHIB bow",p+Vector3.forward*.50f*s,new Vector3(.43f,.13f,.25f)*s,rubber);
            Box("RHIB cockpit",p,new Vector3(.27f,.07f,.85f)*s,dark);
            for(int row=0;row<2;row++)Box("RHIB seating",p+new Vector3(0,.10f,-.22f+row*.34f)*s,new Vector3(.28f,.08f,.10f)*s,hull);
            Box("RHIB outboard engine",p+new Vector3(0,.07f,-.62f)*s,new Vector3(.16f,.25f,.16f)*s,dark);
        }
        private void Radar(Vector3 p,float height,float width)
        {
            Rod("Radar pedestal",p,p+Vector3.up*height,.045f,hull);
            Transform scanner=Group("Rotating navigation scanner",p+Vector3.up*height);
            Part("Navigation radar array",scanner,Vector3.zero,new(width,.075f,.12f),upper,PrimitiveType.Cube,false);
            root.GetComponent<SmallCraftMotion>()?.SetRadar(scanner);
            // Motion is created with the underbody later; store the scanner by name there.
        }
        private void SmallMount(Vector3 p,int side)
        {Cylinder("Deck remote mount base",p,new(.18f,.18f,.18f),hull);Box("Remote mount housing",p+Vector3.up*.14f,new(.17f,.17f,.19f),upper);Rod("Remote mount barrel",p+new Vector3(0,.17f,.06f),p+new Vector3(side*.15f,.20f,.35f),.015f,dark);}
        private void Door(Vector3 p,Vector3 normal,float slope)
        {
            Vector3 outward=new Vector3(normal.x,-slope,0).normalized;
            Vector3 Point(float y,float z)=>p+new Vector3(normal.x*slope*y,y,z)+outward*.008f;
            Vector3[] points={Point(-.22f,-.12f),Point(-.22f,.12f),Point(.22f,.12f),Point(.22f,-.12f)};
            FaceObject("Closed weathertight door",points,outward,hull);
            for(int i=0;i<4;i++)Rod("Door gasket frame",points[i],points[(i+1)%4],.008f,dark);
            Ring("Door handwheel",p+outward*.025f+Vector3.up*.015f,outward,.041f,.006f,upper);
        }
        private void LifeRing(Vector3 p,Vector3 normal)
        {Ring("Lifebuoy",p,normal,.085f,.025f,red);for(int i=0;i<4;i++){Vector3 d=new(0,Mathf.Sin(i*Mathf.PI*.5f),Mathf.Cos(i*Mathf.PI*.5f));Rod("Lifebuoy white band",p+d*.066f,p+d*.102f,.026f,white);}}
        private void Ladder(Vector3 a,Vector3 b,float width)
        {
            Rod("Ladder stile",a-Vector3.right*width*.5f,b-Vector3.right*width*.5f,.012f,hull);Rod("Ladder stile",a+Vector3.right*width*.5f,b+Vector3.right*width*.5f,.012f,hull);
            for(int i=0;i<12;i++){Vector3 p=Vector3.Lerp(a,b,i/11f);Rod("Ladder rung",p-Vector3.right*width*.5f,p+Vector3.right*width*.5f,.010f,upper);}
        }
        private void Housing(string name,Vector3 position,Vector3 bottomSize,Vector3 topSize,Material material,bool windows=false)
        {
            Group(name,position);Vector2[] outline={new(-.72f,-1),new(.72f,-1),new(1,-.72f),new(1,.72f),new(.72f,1),new(-.72f,1),new(-1,.72f),new(-1,-.72f)};
            Vector3[] bottom=new Vector3[8],top=new Vector3[8];
            for(int i=0;i<8;i++){bottom[i]=position+new Vector3(outline[i].x*bottomSize.x*.5f,-bottomSize.y*.5f,outline[i].y*bottomSize.z*.5f);top[i]=position+new Vector3(outline[i].x*topSize.x*.5f,topSize.y*.5f,outline[i].y*topSize.z*.5f);}
            List<Vector3> v=new();List<int> t=new();SmallCraftHull.Face(v,t,top,Vector3.up);SmallCraftHull.Face(v,t,bottom,Vector3.down);
            for(int i=0;i<8;i++)
            {
                int n=(i+1)%8;Vector3 outward=(bottom[i]+bottom[n])*.5f-position;outward.y=0;
                SmallCraftHull.Face(v,t,new[]{bottom[i],bottom[n],top[n],top[i]},outward);
                if(!windows)continue;
                Vector3 normal=Vector3.Cross(top[i]-bottom[i],bottom[n]-bottom[i]).normalized;if(Vector3.Dot(normal,outward)<0)normal=-normal;
                int columns=Mathf.Clamp(Mathf.RoundToInt(Vector3.Distance(top[i],top[n])/.28f),1,6);
                for(int column=0;column<columns;column++)
                {
                    float u0=(column+.12f)/columns,u1=(column+.88f)/columns;
                    Vector3 A(float u,float h)=>Vector3.Lerp(Vector3.Lerp(bottom[i],top[i],h),Vector3.Lerp(bottom[n],top[n],h),u)+normal*.008f;
                    Vector3[] pane={A(u0,.61f),A(u1,.61f),A(u1,.90f),A(u0,.90f)};
                    FaceObject("Opaque bridge window",pane,normal,glass);
                    for(int edge=0;edge<4;edge++)Rod("Bridge window frame",pane[edge]+normal*.002f,pane[(edge+1)%4]+normal*.002f,.008f,hull);
                    Rod("Windscreen wiper",A((u0+u1)*.5f,.63f)+normal*.006f,A(u1-.02f,.86f)+normal*.006f,.006f,dark);
                    if(!Attack&&i==4&&column%2==0)Ring("Circular clear-view screen",A((u0+u1)*.5f,.755f)+normal*.006f,normal,.055f,.007f,upper);
                }
            }
            Mesh mesh=new(){name=name+" closed chamfered shell"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshObject(name+" shell",mesh,new[]{material});
        }
        private void FaceObject(string name,Vector3[] points,Vector3 normal,Material material)
        {List<Vector3> v=new();List<int> t=new();SmallCraftHull.Face(v,t,points,normal);Mesh mesh=new(){name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshObject(name,mesh,new[]{material});}
        private Material Paint(string name,Color color,float gloss=.25f)
        {Material m=owner.Own(new Material(Shader.Find("Standard")){name=vessel.Pennant+" "+name,color=color,enableInstancing=true});m.SetFloat("_Metallic",.02f);m.SetFloat("_Glossiness",gloss);return m;}
        private Transform Group(string name,Vector3 p)
        {GameObject g=new(name);g.transform.SetParent(root,false);g.transform.localPosition=p;return g.transform;}
        private GameObject Part(string name,Transform parent,Vector3 p,Vector3 size,Material material,PrimitiveType kind=PrimitiveType.Cube,bool combine=true)
        {
            GameObject g=GameObject.CreatePrimitive(kind);g.name=name;Collider c=g.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);
            g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;
            if(combine)staticParts.Add(g.GetComponent<MeshFilter>());return g;
        }
        private void Box(string name,Vector3 p,Vector3 size,Material material)=>Part(name,root,p,size,material);
        private void Sphere(string name,Vector3 p,Vector3 size,Material material)=>Part(name,root,p,size,material,PrimitiveType.Sphere);
        private void Cylinder(string name,Vector3 p,Vector3 size,Material material)=>Part(name,root,p,size,material,PrimitiveType.Cylinder);
        private void Rod(string name,Vector3 a,Vector3 b,float diameter,Material material)
        {GameObject g=Part(name,root,(a+b)*.5f,new(diameter,Vector3.Distance(a,b)*.5f,diameter),material,PrimitiveType.Cylinder);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
        private void Ring(string name,Vector3 center,Vector3 normal,float radius,float diameter,Material material)
        {
            Vector3 u=Vector3.Cross(normal,Mathf.Abs(normal.y)<.8f?Vector3.up:Vector3.forward).normalized,v=Vector3.Cross(normal,u);
            for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;Rod(name,center+(u*Mathf.Cos(a)+v*Mathf.Sin(a))*radius,center+(u*Mathf.Cos(b)+v*Mathf.Sin(b))*radius,diameter,material);}
        }
        private void MeshObject(string name,Mesh mesh,Material[] materials)
        {
            owner.Own(mesh);GameObject g=new(name);g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterials=materials;staticParts.Add(g.GetComponent<MeshFilter>());
        }
        private void Combine()
        {
            Dictionary<Material,List<CombineInstance>> batches=new();
            foreach(var part in staticParts)
            {
                Material[] materials=part.GetComponent<Renderer>().sharedMaterials;
                for(int sub=0;sub<part.sharedMesh.subMeshCount;sub++)
                {
                    Material material=materials[sub];if(!batches.ContainsKey(material))batches[material]=new();
                    batches[material].Add(new CombineInstance{mesh=part.sharedMesh,subMeshIndex=sub,transform=root.worldToLocalMatrix*part.transform.localToWorldMatrix});
                }
                part.gameObject.SetActive(false);
            }
            foreach(var batch in batches)
            {
                Mesh combined=owner.Own(new Mesh{name=vessel.Pennant+" exterior / "+batch.Key.name,indexFormat=IndexFormat.UInt32});combined.CombineMeshes(batch.Value.ToArray(),true,true);
                GameObject g=new("Exterior / "+batch.Key.name);g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=combined;g.AddComponent<MeshRenderer>().sharedMaterial=batch.Key;
            }
            foreach(var part in staticParts)Object.Destroy(part.gameObject);staticParts.Clear();
        }
    }
}
