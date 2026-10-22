using UnityEngine;
namespace Meridian
{
    /// <summary>Small-craft-sized deck crew; never reuses the frigate's out-of-bounds walking routes.</summary>
    public sealed class SmallCraftDeckLife:MonoBehaviour
    {
        private sealed class Sailor
        {
            public Transform Root,Head,LeftArm,RightArm,LeftLeg,RightLeg;
            public Vector3 Station;public int Role;public float Phase;
        }
        private readonly Sailor[] crew=new Sailor[6];
        private VesselDefinition vessel;
        private Material uniform,skin,vest,dark;
        public int CrewCount=>crew.Length;
        public void Build(VesselDefinition definition,Material blue,Material skinPaint,Material safety,Material black)
        {
            vessel=definition;uniform=blue;skin=skinPaint;vest=safety;dark=black;
            float half=SmallCraftHull.HalfLength(vessel.Id);
            for(int i=0;i<6;i++)
            {
                int side=i%2==0?-1:1;float z=i<2?half-1.4f:i<4?-half+.70f:vessel.HasFourEngines?1.1f:-.50f;
                float x=i<4?side*SmallCraftHull.HalfWidth(vessel.Id,z)*.56f:side*(vessel.HasFourEngines?.80f:1.02f);
                float y=i>=4&&!vessel.HasFourEngines?.67f:SmallCraftHull.DeckHeight(vessel.Id,z)+.025f;
                Transform root=Group(i<2?"Bow deck crew":i<4?"Stern deck crew":"Bridge watch",transform,new(x,y,z));
                Sailor s=new(){Root=root,Station=new(x,y,z),Role=i/2,Phase=i*1.83f};crew[i]=s;
                s.LeftLeg=Limb("Left leg",root,new(-.026f,.20f,0),new(.034f,.16f,.037f),uniform);
                s.RightLeg=Limb("Right leg",root,new(.026f,.20f,0),new(.034f,.16f,.037f),uniform);
                Part("Boot",s.LeftLeg,new(0,-.17f,.018f),new(.038f,.025f,.072f),dark,PrimitiveType.Cube);
                Part("Boot",s.RightLeg,new(0,-.17f,.018f),new(.038f,.025f,.072f),dark,PrimitiveType.Cube);
                Part("Working uniform",root,new(0,.28f,0),new(.108f,.17f,.073f),uniform,PrimitiveType.Capsule);
                Part("Deck safety vest",root,new(0,.285f,.006f),new(.103f,.115f,.079f),i<4?vest:uniform,PrimitiveType.Cube);
                s.LeftArm=Limb("Left arm",root,new(-.062f,.34f,0),new(.031f,.13f,.033f),uniform);
                s.RightArm=Limb("Right arm",root,new(.062f,.34f,0),new(.031f,.13f,.033f),uniform);
                Part("Left hand",s.LeftArm,new(0,-.136f,0),Vector3.one*.026f,skin,PrimitiveType.Sphere);
                Part("Right hand",s.RightArm,new(0,-.136f,0),Vector3.one*.026f,skin,PrimitiveType.Sphere);
                s.Head=Group("Lookout head",root,new(0,.39f,0));
                Part("Head",s.Head,Vector3.zero,new(.064f,.075f,.063f),skin,PrimitiveType.Sphere);
                Part("Navy cap",s.Head,new(0,.035f,0),new(.069f,.023f,.072f),uniform,PrimitiveType.Sphere);
                Part("Cap peak",s.Head,new(0,.032f,.030f),new(.064f,.008f,.035f),dark,PrimitiveType.Cube);
            }
        }
        private void Update()
        {
            if(vessel==null)return;
            for(int i=0;i<crew.Length;i++)
            {
                Sailor s=crew[i];if(s==null)continue;float phase=Time.time*.8f+s.Phase;
                Vector3 p=s.Station;
                bool walking=s.Role==2&&vessel.HasFourEngines;
                if(walking){p.z+=Mathf.Sin(phase*.55f)*.30f;p.y=SmallCraftHull.DeckHeight(vessel.Id,p.z)+.025f;}
                s.Root.localPosition=p;s.Root.localRotation=Quaternion.Euler(0,s.Role==1?180:s.Role==2?(i%2==0?90:-90):0,0);
                s.Head.localRotation=Quaternion.Euler(Mathf.Sin(phase*.7f)*4,Mathf.Sin(phase*.37f)*36,0);
                float step=walking?Mathf.Sin(phase*3)*15:0;
                s.LeftLeg.localRotation=Quaternion.Euler(step,0,0);s.RightLeg.localRotation=Quaternion.Euler(-step,0,0);
                float handling=s.Role<2?26+Mathf.Sin(phase*1.6f)*7:0;
                s.LeftArm.localRotation=Quaternion.Euler(handling-step*.5f,0,9);s.RightArm.localRotation=Quaternion.Euler(handling+step*.5f,0,-9);
            }
        }
        private Transform Limb(string name,Transform parent,Vector3 p,Vector3 size,Material material)
        {Transform joint=Group(name,parent,p);Part(name+" sleeve",joint,Vector3.down*size.y*.5f,size,material,PrimitiveType.Capsule);return joint;}
        private static Transform Group(string name,Transform parent,Vector3 p)
        {GameObject g=new(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;return g.transform;}
        private static void Part(string name,Transform parent,Vector3 p,Vector3 size,Material material,PrimitiveType primitive)
        {GameObject g=GameObject.CreatePrimitive(primitive);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;Collider c=g.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);g.GetComponent<Renderer>().sharedMaterial=material;}
    }
}
