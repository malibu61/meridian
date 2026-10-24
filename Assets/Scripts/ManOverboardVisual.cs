using UnityEngine;
namespace Meridian
{
    /// <summary>Life-jacketed training casualty and simplified recovery sling; no casualty collider or teleporting ship.</summary>
    public sealed class ManOverboardVisual:MonoBehaviour
    {
        private DockingMissionRun run;private ShipController ship;private Transform person,armLeft,armRight,sling;private LineRenderer marker,rope;private HarborCrewMember rescuer;
        private ParticleSystem splash;private bool enteredWater;
        public Vector3 PersonPosition=>person.position;
        public int SplashCount{get;private set;}
        public void Build(DockingMissionRun mission,EnvironmentResources resources)
        {
            run=mission;ship=Object.FindAnyObjectByType<GameSessionController>().PlayerShip;
            Material skin=Paint(resources,new Color(.65f,.43f,.29f)),navy=Paint(resources,new Color(.035f,.075f,.13f)),orange=Paint(resources,new Color(1,.32f,.035f)),white=Paint(resources,new Color(.95f,.94f,.8f));
            person=new GameObject("Life-jacketed training casualty 1.76 metres").transform;person.SetParent(transform,false);
            Part("Body",person,PrimitiveType.Capsule,new(0,-.04f,0),new(.115f,.13f,.07f),navy);
            Part("Life jacket",person,PrimitiveType.Cube,new(0,.07f,0),new(.135f,.14f,.09f),orange);
            Part("Head above water",person,PrimitiveType.Sphere,new(0,.19f,0),new(.072f,.08f,.07f),skin);
            Part("Left reflective strip",person,PrimitiveType.Cube,new(-.04f,.09f,.047f),new(.02f,.06f,.005f),white);
            Part("Right reflective strip",person,PrimitiveType.Cube,new(.04f,.09f,.047f),new(.02f,.06f,.005f),white);
            armLeft=Part("Signal arm left",person,PrimitiveType.Capsule,new(-.105f,.12f,0),new(.035f,.09f,.035f),navy);
            armRight=Part("Signal arm right",person,PrimitiveType.Capsule,new(.105f,.12f,0),new(.035f,.09f,.035f),navy);
            Part("Leg left",person,PrimitiveType.Capsule,new(-.032f,-.15f,0),new(.044f,.10f,.044f),navy);
            Part("Leg right",person,PrimitiveType.Capsule,new(.032f,-.15f,0),new(.044f,.10f,.044f),navy);
            Material line=resources.Own(new Material(Shader.Find("Sprites/Default")));
            marker=Line("MOB navigation marker (not equipment scale)",49,.04f,line,MaritimeUiTheme.Amber);
            rope=Line("Recovery strop and hoist line",2,.014f,line,new Color(.8f,.67f,.4f));
            sling=Part("Orange rescue sling",person,PrimitiveType.Cube,new(0,.02f,.052f),new(.15f,.025f,.012f),orange);
            var crew=new GameObject("Recovery station line handler");crew.transform.SetParent(transform,false);rescuer=crew.AddComponent<HarborCrewMember>();rescuer.Build(HarborShoreLife.Palette(resources,true),true,false,0,resources);
            CreateSplash(resources);
            UpdateVisuals();
        }
        private void CreateSplash(EnvironmentResources resources)
        {
            var texture=resources.Own(new Texture2D(32,32,TextureFormat.RGBA32,false));texture.name="Soft MOB spray droplet";
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {float radius=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2)));}
            texture.Apply();var material=resources.Own(new Material(Shader.Find("Sprites/Default")){mainTexture=texture});
            var go=new GameObject("MOB water entry splash");go.transform.SetParent(transform,false);splash=go.AddComponent<ParticleSystem>();
            splash.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=splash.main;main.playOnAwake=false;main.loop=false;main.duration=2;main.maxParticles=64;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=.25f;main.startSpeed=0;
            var emission=splash.emission;emission.enabled=false;var shape=splash.shape;shape.enabled=false;
            var fade=splash.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();
            gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.64f,.84f,.92f),1)},new[]{new GradientAlphaKey(.9f,0),new GradientAlphaKey(0,1)});fade.color=gradient;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
        private void EnterWater(Vector3 point)
        {
            enteredWater=true;SplashCount++;splash.Play();
            for(int i=0;i<48;i++)
            {
                float a=i*2.399963f,r=.35f+(i%7)*.06f;
                splash.Emit(new ParticleSystem.EmitParams{position=point+new Vector3(Mathf.Cos(a),.04f,Mathf.Sin(a))*.07f,
                    velocity=new Vector3(Mathf.Cos(a)*r,.65f+(i%9)*.1f,Mathf.Sin(a)*r),startSize=.05f+(i%5)*.025f,startLifetime=.65f+(i%7)*.09f,startColor=new Color(.86f,.96f,1,.85f)},1);
            }
        }
        private Material Paint(EnvironmentResources resources,Color color)=>resources.Own(new Material(Shader.Find("Standard")){color=color});
        private Transform Part(string name,Transform parent,PrimitiveType primitive,Vector3 position,Vector3 scale,Material material)
        {var p=GameObject.CreatePrimitive(primitive);p.name=name;p.transform.SetParent(parent,false);p.transform.localPosition=position;p.transform.localScale=scale;p.GetComponent<Renderer>().sharedMaterial=material;var c=p.GetComponent<Collider>();c.enabled=false;Destroy(c);return p.transform;}
        private LineRenderer Line(string name,int count,float width,Material material,Color color)
        {var p=new GameObject(name);p.transform.SetParent(transform,false);var line=p.AddComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=count;line.widthMultiplier=width;line.startColor=line.endColor=color;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return line;}
        private void LateUpdate()=>UpdateVisuals();
        public void SampleVisuals()=>UpdateVisuals();
        private void UpdateVisuals()
        {
            if(run?.Rescue==null||ship==null)return;var r=run.Rescue;Vector3 sea=r.Casualty;sea.y=WaterWaves.SurfaceY(sea,Time.time)+.025f;
            if(!r.InWater)
            {
                person.position=r.FallPosition(sea.y);
                person.rotation=Quaternion.Euler(Mathf.Sin(r.FallProgress*Mathf.PI)*22,0,-r.Side*r.FallProgress*85);
                armLeft.localRotation=Quaternion.Euler(0,0,-65-Mathf.Sin(r.FallProgress*15)*25);
                armRight.localRotation=Quaternion.Euler(0,0,65+Mathf.Sin(r.FallProgress*15)*25);
                marker.enabled=rope.enabled=false;sling.gameObject.SetActive(false);rescuer.gameObject.SetActive(false);return;
            }
            if(!enteredWater)EnterWater(sea);
            rescuer.gameObject.SetActive(true);
            Vector3 anchor=ship.transform.TransformPoint(r.LocalStation);bool lifting=r.Stage==RescueStage.Recovering||r.Completed;
            // Stay outside the hull while lifting, then settle onto the deck only above rail height.
            Vector3 raised=new(anchor.x,anchor.y+.7f,anchor.z);Vector3 aboard=ship.transform.TransformPoint(new Vector3(r.LocalStation.x-r.Side*.65f,r.DeckHeight,0));
            float t=r.Completed?1:r.RecoverySeconds/8;
            person.position=lifting?(t<.72f?Vector3.Lerp(sea,raised,Mathf.SmoothStep(0,1,t/.72f)):Vector3.Lerp(raised,aboard+ship.transform.up*.25f,Mathf.SmoothStep(0,1,(t-.72f)/.28f))):sea;
            Quaternion floating=Quaternion.Euler(Mathf.Sin(Time.time*1.7f)*5,0,Mathf.Sin(Time.time*1.3f)*9);
            person.rotation=lifting?ship.transform.rotation:Quaternion.Slerp(Quaternion.Euler(0,0,-r.Side*85),floating,Mathf.Clamp01((r.Elapsed-r.FallSeconds)*2));
            armLeft.localRotation=Quaternion.Euler(0,0,-45-Mathf.Sin(Time.time*2)*25);armRight.localRotation=Quaternion.Euler(0,0,45+Mathf.Sin(Time.time*2+1)*25);
            sling.gameObject.SetActive(lifting);rope.enabled=lifting&&!r.Completed;rope.SetPosition(0,raised);rope.SetPosition(1,person.position+Vector3.up*.12f);
            marker.enabled=r.Marked&&!r.Completed;
            for(int i=0;i<49;i++){float a=i*Mathf.PI*2/48;Vector3 p=r.Casualty+new Vector3(Mathf.Cos(a)*.8f,0,Mathf.Sin(a)*.8f);p.y=WaterWaves.SurfaceY(p,Time.time)+.09f;marker.SetPosition(i,p);}
            rescuer.transform.position=aboard;rescuer.transform.rotation=ship.transform.rotation*Quaternion.Euler(0,r.Side*90,0);rescuer.SamplePose(Time.time,lifting&&!r.Completed?HarborCrewPose.Haul:HarborCrewPose.Watch,t);
        }
    }
}
