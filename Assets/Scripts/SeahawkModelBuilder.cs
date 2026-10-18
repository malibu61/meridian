using System.Collections.Generic;
using UnityEngine;

namespace Meridian
{
    /// <summary>Lightweight S-70B exterior study. Folded rotors, wheels/chocks and tie-downs denote a parked aircraft.
    /// No weapons, flight behaviour or invented real-aircraft serial number are attached to this visual.</summary>
    public sealed class SeahawkModelBuilder
    {
        private readonly Transform root;
        private readonly NavalDeckLife owner;
        private readonly Material grey, panel, glass, rubber, metal, white, red, yellow;
        private readonly List<MeshFilter> parts = new();

        public SeahawkModelBuilder(Transform parent)
        {
            root = parent;
            owner = parent.GetComponentInParent<NavalDeckLife>();
            grey = Paint("Seahawk naval grey", new Color(.50f,.55f,.57f),.28f);
            panel = Paint("Seahawk panel edges", new Color(.25f,.29f,.30f),.2f);
            glass = Paint("Seahawk dark cockpit glazing", new Color(.025f,.075f,.105f),.77f);
            rubber = Paint("Aircraft tires and folded blades", new Color(.042f,.048f,.052f),.19f);
            metal = Paint("Rotor head and undercarriage", new Color(.31f,.34f,.34f),.4f);
            white = Paint("Aircraft national marking white", new Color(.87f,.88f,.85f),.15f);
            red = Paint("Aircraft national marking red", new Color(.70f,.035f,.04f),.15f);
            yellow = Paint("Wheel chocks and blade tips", new Color(.83f,.58f,.10f),.15f);
            if (owner != null) foreach (Material m in new[] { grey,panel,glass,rubber,metal,white,red,yellow }) owner.OwnAircraftMaterial(m);
        }

        public void Build()
        {
            Loft("Closed sculpted Seahawk fuselage",
                new[]{-.95f,-.70f,-.2f,.35f,.70f,1.04f,1.26f,1.38f},
                new[]{.52f,.52f,.53f,.53f,.49f,.41f,.34f,.31f},
                new[]{.16f,.28f,.30f,.31f,.29f,.25f,.14f,.01f},
                new[]{.25f,.31f,.32f,.32f,.29f,.22f,.13f,.02f}, grey);
            Loft("Tapered tail boom",new[]{-2.36f,-2.12f,-1.55f,-.78f},new[]{.63f,.60f,.55f,.52f},
                new[]{.035f,.065f,.10f,.17f},new[]{.055f,.08f,.12f,.18f},grey);
            // The S-70B's characteristic under-nose maritime radar fairing, not a skid-equipped utility Black Hawk.
            Sphere("Maritime radar radome",new Vector3(0f,.235f,.96f),new Vector3(.49f,.18f,.46f),grey);
            Sphere("Nose sensor housing",new Vector3(.18f,.47f,1.21f),new Vector3(.13f,.13f,.12f),panel);
            Sphere("Sensor window",new Vector3(.18f,.47f,1.268f),new Vector3(.072f,.070f,.013f),glass);
            Rod("Nose pitot",new Vector3(-.21f,.39f,1.17f),new Vector3(-.23f,.40f,1.47f),.008f,metal);
            for (int side = -1; side <= 1; side += 2)
            {
                float s = side;
                CockpitGlazing(side);
                Rod("Windscreen outer frame",new Vector3(s*.21f,.72f,.78f),new Vector3(s*.16f,.51f,1.23f),.011f,grey);
                Face("Pilot side window",new[]{new Vector3(s*.26f,.677f,.70f),new Vector3(s*.293f,.745f,.36f),new Vector3(s*.314f,.57f,.31f),new Vector3(s*.281f,.50f,.81f)},glass);
                Face("Pilot aft window",new[]{new Vector3(s*.304f,.737f,.27f),new Vector3(s*.301f,.737f,.03f),new Vector3(s*.311f,.54f,.03f),new Vector3(s*.318f,.54f,.27f)},glass);
                Rod("Pilot door pillar",new Vector3(s*.316f,.54f,.29f),new Vector3(s*.303f,.74f,.30f),.010f,grey);
                // Closed sliding cabin doors with visible rail, gasket and inset window.
                Face("Cabin door panel",new[]{new Vector3(s*.297f,.36f,-.64f),new Vector3(s*.318f,.36f,-.05f),new Vector3(s*.310f,.72f,-.05f),new Vector3(s*.284f,.72f,-.64f)},grey);
                Face("Cabin door window",new[]{new Vector3(s*.306f,.53f,-.52f),new Vector3(s*.323f,.53f,-.16f),new Vector3(s*.315f,.682f,-.16f),new Vector3(s*.294f,.682f,-.52f)},glass);
                Rod("Sliding door upper rail",new Vector3(s*.292f,.74f,-.74f),new Vector3(s*.321f,.74f,-.03f),.008f,panel);
                Rod("Door lower seam",new Vector3(s*.300f,.36f,-.64f),new Vector3(s*.322f,.36f,-.05f),.005f,panel);
                Rod("Door rear seam",new Vector3(s*.297f,.36f,-.64f),new Vector3(s*.285f,.72f,-.64f),.005f,panel);
                Rod("Cabin door handle",new Vector3(s*.327f,.48f,-.10f),new Vector3(s*.327f,.48f,-.19f),.006f,panel);
                Rod("Cabin step",new Vector3(s*.38f,.25f,-.50f),new Vector3(s*.38f,.25f,-.16f),.014f,metal);
                Rod("Step bracket",new Vector3(s*.29f,.35f,-.43f),new Vector3(s*.38f,.25f,-.43f),.011f,metal);
                Sphere("Twin engine nacelle",new Vector3(s*.185f,.855f,-.22f),new Vector3(.27f,.25f,.91f),grey);
                Sphere("Dark engine intake",new Vector3(s*.185f,.89f,.214f),new Vector3(.19f,.16f,.020f),panel);
                Rod("Exhaust pipe",new Vector3(s*.18f,.86f,-.49f),new Vector3(s*.24f,.87f,-.79f),.089f,metal);
                Sphere("Exhaust aperture",new Vector3(s*.242f,.873f,-.80f),new Vector3(.15f,.14f,.020f),rubber);
                // Main gear are forward; the Seahawk's compact rear gear is under the aft fuselage.
                Rod("Main gear shock strut",new Vector3(s*.265f,.40f,.47f),new Vector3(s*.47f,.105f,.45f),.018f,metal);
                Rod("Main gear brace",new Vector3(s*.24f,.27f,.05f),new Vector3(s*.47f,.105f,.45f),.013f,metal);
                Wheel("Main wheel",new Vector3(s*.47f,.105f,.45f),.106f,.070f);
                Box("Wheel chock",new Vector3(s*.47f,.028f,.585f),new Vector3(.12f,.052f,.07f),yellow);
                Box("Wheel chock",new Vector3(s*.47f,.028f,.315f),new Vector3(.12f,.052f,.07f),yellow);
                Rod("Aircraft securing chain",new Vector3(s*.28f,.30f,-.53f),new Vector3(s*.77f,.012f,-.73f),.008f,metal);
                Roundel(side,new Vector3(s*.185f,.59f,-.86f),.063f);
                // Small rectangular Turkish flag on the fin, repeated on both sides.
                Face("Turkish fin flag",new[]{new Vector3(s*.033f,.97f,-2.25f),new Vector3(s*.033f,.97f,-2.08f),new Vector3(s*.033f,1.07f,-2.08f),new Vector3(s*.033f,1.07f,-2.25f)},red);
                Crescent(side,new Vector3(s*.035f,1.022f,-2.19f),.032f);
            }
            Rod("Central windscreen frame",new Vector3(0f,.794f,.65f),new Vector3(0f,.568f,1.245f),.010f,grey);
            Rod("Left windscreen wiper",new Vector3(-.08f,.585f,1.211f),new Vector3(-.12f,.655f,1.055f),.004f,panel);
            Rod("Right windscreen wiper",new Vector3(.08f,.585f,1.211f),new Vector3(.12f,.655f,1.055f),.004f,panel);
            Rod("Rear landing strut",new Vector3(0f,.43f,-.88f),new Vector3(0f,.085f,-.94f),.019f,metal);
            Wheel("Rear twin wheel port",new Vector3(-.048f,.080f,-.94f),.079f,.040f);
            Wheel("Rear twin wheel starboard",new Vector3(.048f,.080f,-.94f),.079f,.040f);
            Face("Tail fin",new[]{new Vector3(0f,.59f,-2.38f),new Vector3(0f,.65f,-1.98f),new Vector3(0f,1.29f,-2.15f),new Vector3(0f,1.32f,-2.34f)},grey,.045f);
            Face("Tail stabilator",new[]{new Vector3(-.55f,.62f,-2.03f),new Vector3(.55f,.62f,-2.03f),new Vector3(.48f,.62f,-2.31f),new Vector3(-.48f,.62f,-2.31f)},grey,.024f);
            Rod("Tail rotor shaft",new Vector3(0f,1.13f,-2.24f),new Vector3(-.15f,1.13f,-2.24f),.025f,metal);
            for (int blade = 0; blade < 4; blade++)
            {
                float a = (blade * 90f + 24f) * Mathf.Deg2Rad;
                Vector3 direction = new(0f,Mathf.Cos(a),Mathf.Sin(a));
                Blade("Four-blade tail rotor",new Vector3(-.16f,1.13f,-2.24f)+direction*.05f,
                    new Vector3(-.16f,1.13f,-2.24f)+direction*.34f,.049f,rubber,Vector3.right);
            }
            Rod("Main rotor mast",new Vector3(0f,.84f,-.02f),new Vector3(0f,1.13f,-.02f),.031f,metal);
            Sphere("Rotor hub",new Vector3(0f,1.115f,-.02f),new Vector3(.24f,.085f,.24f),metal);
            for (int i = 0; i < 4; i++)
            {
                float x = (i-1.5f)*.12f;
                Vector3 hinge = new(x,1.12f,-.11f + Mathf.Abs(x)*.25f);
                Rod("Fold hinge linkage",new Vector3(0f,1.10f,-.02f),hinge,.017f,metal);
                Vector3 tip = new(x*2.1f,1.02f,-1.98f + i*.045f);
                Blade("Folded main rotor blade",hinge,tip,.092f,rubber,Vector3.up);
                Blade("Rotor blade tip stripe",Vector3.Lerp(hinge,tip,.95f),tip,.094f,yellow,Vector3.up);
            }
            Rod("Rotor folding support",new Vector3(0f,.60f,-1.63f),new Vector3(0f,1.04f,-1.63f),.014f,metal);
            Rod("Dorsal antenna",new Vector3(0f,.76f,-.78f),new Vector3(0f,1.06f,-.90f),.006f,panel);
            Rod("Nose aerial",new Vector3(.09f,.70f,.75f),new Vector3(.09f,.97f,.70f),.005f,panel);
            Combine();
        }

        private static Material Paint(string name,Color c,float smooth)
        { Material m=new(Shader.Find("Standard")){name=name,color=c,enableInstancing=true}; m.SetFloat("_Glossiness",smooth); return m; }

        private void CockpitGlazing(int side)
        {
            // Conform the panes to the loft; a single flat quad would sink into the rounded nose.
            for (int row=0;row<6;row++) for (int column=0;column<5;column++)
            {
                float z0=Mathf.Lerp(.67f,1.17f,row/6f),z1=Mathf.Lerp(.67f,1.17f,(row+1)/6f);
                float a0=Mathf.Lerp(.075f,.86f,column/5f),a1=Mathf.Lerp(.075f,.86f,(column+1)/5f);
                Face("Curved front windshield",new[]{CockpitPoint(side,z0,a0),CockpitPoint(side,z0,a1),CockpitPoint(side,z1,a1),CockpitPoint(side,z1,a0)},glass);
            }
        }
        private static Vector3 CockpitPoint(int side,float z,float a)
        {
            bool forward=z>1.04f;
            float u=forward?Mathf.InverseLerp(1.04f,1.26f,z):Mathf.InverseLerp(.70f,1.04f,z);
            float center=forward?Mathf.Lerp(.41f,.34f,u):Mathf.Lerp(.49f,.41f,u);
            float width=forward?Mathf.Lerp(.25f,.14f,u):Mathf.Lerp(.29f,.25f,u);
            float height=forward?Mathf.Lerp(.22f,.13f,u):Mathf.Lerp(.29f,.22f,u);
            return new Vector3(side*Mathf.Sin(a)*(width+.008f),center+Mathf.Cos(a)*(height+.008f),z);
        }

        private GameObject Part(string name,Vector3 p,Vector3 size,Material paint,PrimitiveType kind=PrimitiveType.Cube)
        {
            GameObject go=GameObject.CreatePrimitive(kind); go.name=name;
            Collider c=go.GetComponent<Collider>(); c.enabled=false; Object.Destroy(c);
            go.transform.SetParent(root,false); go.transform.localPosition=p; go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=paint; parts.Add(go.GetComponent<MeshFilter>()); return go;
        }
        private void Box(string name,Vector3 p,Vector3 size,Material m)=>Part(name,p,size,m);
        private void Sphere(string name,Vector3 p,Vector3 size,Material m)=>Part(name,p,size,m,PrimitiveType.Sphere);
        private void Rod(string name,Vector3 a,Vector3 b,float r,Material m)
        { GameObject go=Part(name,(a+b)*.5f,new Vector3(r*2f,(b-a).magnitude*.5f,r*2f),m,PrimitiveType.Cylinder); go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a); }
        private void Wheel(string name,Vector3 p,float radius,float width)
        {
            GameObject tire=Part(name,p,new Vector3(radius*2f,width*.5f,radius*2f),rubber,PrimitiveType.Cylinder);
            tire.transform.localRotation=Quaternion.Euler(0f,0f,90f);
            GameObject hub=Part(name+" hub",p,new Vector3(radius*.86f,width*.52f,radius*.86f),metal,PrimitiveType.Cylinder);
            hub.transform.localRotation=Quaternion.Euler(0f,0f,90f);
        }
        private void Blade(string name,Vector3 a,Vector3 b,float width,Material m,Vector3 normal)
        {
            Vector3 across=Vector3.Cross(b-a,normal).normalized*width*.5f;
            Face(name,new[]{a-across,a+across,b+across,b-across},m,.009f);
        }
        private void Face(string name,Vector3[] points,Material m,float thickness=0f)
        {
            // Explicit back faces keep windows and fins visible from both quarters.
            List<Vector3> v=new(); List<int> t=new();
            Vector3 normal=Vector3.Cross(points[1]-points[0],points[2]-points[0]).normalized;
            for(int side=0;side<2;side++)
            {
                int start=v.Count; foreach(Vector3 p in points) v.Add(p+normal*(side==0?thickness*.5f:-thickness*.5f));
                for(int i=1;i<points.Length-1;i++) if(side==0)t.AddRange(new[]{start,start+i,start+i+1});else t.AddRange(new[]{start,start+i+1,start+i});
            }
            MeshObject(name,v,t,m);
        }
        private void Loft(string name,float[] zs,float[] ys,float[] widths,float[] heights,Material m)
        {
            const int sides=20; List<Vector3> v=new(); List<int> t=new();
            for(int station=0;station<zs.Length;station++) for(int i=0;i<sides;i++)
            {float a=i*Mathf.PI*2f/sides;v.Add(new Vector3(Mathf.Sin(a)*widths[station],ys[station]+Mathf.Cos(a)*heights[station],zs[station]));}
            for(int station=0;station<zs.Length-1;station++)for(int i=0;i<sides;i++)
            {int a=station*sides+i,b=station*sides+(i+1)%sides,c=b+sides,d=a+sides;t.AddRange(new[]{a,c,b,a,d,c});}
            for(int i=1;i<sides-1;i++){t.AddRange(new[]{0,i,i+1});int a=(zs.Length-1)*sides;t.AddRange(new[]{a,a+i+1,a+i});}
            MeshObject(name,v,t,m);
        }
        private void MeshObject(string name,List<Vector3> v,List<int> t,Material m)
        {
            Mesh mesh=new(){name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            if(owner!=null)owner.OwnAircraftMesh(mesh);
            GameObject go=new(name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=m;
            parts.Add(go.GetComponent<MeshFilter>());
        }
        private void Roundel(int side,Vector3 p,float r)
        {
            // Turkish military aircraft use a red/white/red national roundel.
            for(int ring=0;ring<3;ring++)
            {
                float radius=r*(ring==0?1f:ring==1?.70f:.40f); List<Vector3> v=new(){RoundelPoint(side,p.y,p.z,ring)}; List<int> t=new();
                for(int i=0;i<=24;i++){float a=i*Mathf.PI/12f;v.Add(RoundelPoint(side,p.y+Mathf.Cos(a)*radius,p.z+Mathf.Sin(a)*radius,ring));}
                for(int i=1;i<=24;i++)t.AddRange(side>0?new[]{0,i,i+1}:new[]{0,i+1,i});
                MeshObject("Turkish naval aviation roundel",v,t,ring==1?white:red);
            }
        }
        private static Vector3 RoundelPoint(int side,float y,float z,int ring)
        {
            float u=Mathf.InverseLerp(-.95f,-.70f,z);
            float width=Mathf.Lerp(.16f,.28f,u),height=Mathf.Lerp(.25f,.31f,u);
            float dy=(y-.52f)/height;
            return new Vector3(side*(width*Mathf.Sqrt(Mathf.Max(0f,1f-dy*dy))+.003f+ring*.0015f),y,z);
        }
        private void Crescent(int side,Vector3 p,float r)
        {
            List<Vector3> v=new();List<int> t=new();
            for(int i=0;i<=20;i++)
            {
                float a=Mathf.Lerp(.65f,Mathf.PI*2f-.65f,i/20f);
                v.Add(p+new Vector3(0f,Mathf.Sin(a)*r,Mathf.Cos(a)*r));
                v.Add(p+new Vector3(0f,Mathf.Sin(a)*r*.82f,Mathf.Cos(a)*r*.82f+r*.30f));
            }
            for(int i=0;i<20;i++){int a=i*2;t.AddRange(new[]{a,a+1,a+2,a+2,a+1,a+3,a+2,a+1,a,a+3,a+1,a+2});}
            MeshObject("White crescent",v,t,white);
            List<Vector3> star=new(){p+new Vector3(0f,0f,r*1.05f)};List<int> st=new();
            for(int i=0;i<10;i++){float a=i*Mathf.PI/5f;float radius=r*(i%2==0?.35f:.15f);star.Add(star[0]+new Vector3(0f,Mathf.Sin(a)*radius,Mathf.Cos(a)*radius));}
            for(int i=1;i<=10;i++)st.AddRange(new[]{0,i,i%10+1,0,i%10+1,i});MeshObject("White star",star,st,white);
        }
        private void Combine()
        {
            Dictionary<Material,List<CombineInstance>> batches=new();
            foreach(MeshFilter filter in parts)
            {
                Material m=filter.GetComponent<Renderer>().sharedMaterial;
                if(!batches.TryGetValue(m,out List<CombineInstance> batch)){batch=new();batches.Add(m,batch);}
                batch.Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
            }
            foreach(var entry in batches)
            {
                Mesh mesh=new(){name="Seahawk combined "+entry.Key.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(entry.Value.ToArray());
                if(owner!=null)owner.OwnAircraftMesh(mesh);
                GameObject go=new(mesh.name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=entry.Key;
            }
            foreach(MeshFilter part in parts){part.gameObject.SetActive(false);Object.Destroy(part.gameObject);}
        }
    }
}
