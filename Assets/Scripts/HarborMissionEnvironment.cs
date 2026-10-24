using UnityEngine;
namespace Meridian
{
    public sealed class HarborMissionEnvironment:MonoBehaviour
    {
        private DockingMissionRun run;private EnvironmentResources resources;private Material line,grey,dark,deck;
        private readonly LineRenderer[] gates=new LineRenderer[3];private LineRenderer target;
        public void Build(DockingMissionRun mission)
        {
            run=mission;resources=gameObject.AddComponent<EnvironmentResources>();
            if(mission.Definition.IsRescue){gameObject.AddComponent<ManOverboardVisual>().Build(mission,resources);return;}
            line=resources.Own(new Material(Shader.Find("Sprites/Default")));grey=Paint(new Color(.40f,.46f,.48f));dark=Paint(new Color(.07f,.11f,.14f));deck=Paint(new Color(.57f,.62f,.64f));
            for(int i=0;i<3;i++)gates[i]=Line("Mission entry gate "+(i+1),49,.075f);
            target=Line("Mission berth outline",5,.09f);
            float separation=VesselCatalog.Get(mission.Definition.Vessel).LengthMetres/8+VesselCatalog.Get(VesselId.Kumkale).LengthMetres/8+2.3f;
            if(mission.Definition.TightBerth){Traffic(mission.Target+Vector3.forward*separation);Traffic(mission.Target-Vector3.forward*separation);}
            else Traffic(new Vector3(-20,.1f,-8));
            Draw();
        }
        private Material Paint(Color color)=>resources.Own(new Material(Shader.Find("Standard")){color=color});
        private LineRenderer Line(string name,int points,float width)
        {var go=new GameObject(name);go.transform.SetParent(transform,false);var renderer=go.AddComponent<LineRenderer>();renderer.useWorldSpace=true;renderer.positionCount=points;renderer.widthMultiplier=width;renderer.sharedMaterial=line;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return renderer;}
        private void LateUpdate()=>Draw();
        private void Draw()
        {
            if(run==null||run.Definition.IsRescue)return;
            for(int i=0;i<3;i++)
            {
                Vector3 center=DockingMissionCatalog.Gate(i);float radius=DockingMissionCatalog.GateRadius(i);Color color=i<run.GatesPassed?new Color(.1f,.5f,.35f):i==run.GatesPassed?MaritimeUiTheme.Amber:MaritimeUiTheme.Muted;
                gates[i].startColor=gates[i].endColor=color;
                for(int p=0;p<49;p++){float angle=p*Mathf.PI*2/48;Vector3 v=center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius;v.y=WaterWaves.SurfaceY(v,Time.time)+.10f;gates[i].SetPosition(p,v);}
            }
            target.startColor=target.endColor=run.Phase==DockingMissionPhase.Passed?MaritimeUiTheme.Teal:MaritimeUiTheme.Amber;
            var vessel=VesselCatalog.Get(run.Definition.Vessel);float w=vessel.BeamMetres/8+.25f,h=vessel.LengthMetres/8+.7f;
            Vector3 c=run.Target;Vector3[] corners={new(-w,0,-h),new(-w,0,h),new(w,0,h),new(w,0,-h),new(-w,0,-h)};
            for(int i=0;i<5;i++){Vector3 v=c+corners[i];v.y=WaterWaves.SurfaceY(v,Time.time)+.12f;target.SetPosition(i,v);}
        }
        private void Traffic(Vector3 p)
        {
            var go=new GameObject("Mission visible training vessel");go.transform.SetParent(transform,false);go.transform.position=p;
            var filter=go.AddComponent<MeshFilter>();filter.sharedMesh=resources.Own(SmallCraftHull.CreateMesh(VesselId.Kumkale));go.AddComponent<MeshRenderer>().sharedMaterials=new[]{grey,deck,dark};
            var box=go.AddComponent<BoxCollider>();var d=VesselCatalog.Get(VesselId.Kumkale);box.size=d.ColliderSize;box.center=d.ColliderCenter;
            Part(go.transform,"Moored deckhouse",new Vector3(0,.6f,.1f),new Vector3(1.65f,1.5f,5.5f),grey);
            Part(go.transform,"Moored bridge",new Vector3(0,1.55f,1.8f),new Vector3(1.75f,.65f,1.85f),deck);
            Part(go.transform,"Moored windows",new Vector3(0,1.6f,2.74f),new Vector3(1.50f,.3f,.03f),dark);
            Part(go.transform,"Moored mast",new Vector3(0,2.7f,.2f),new Vector3(.13f,2.2f,.13f),grey);
        }
        private void Part(Transform parent,string name,Vector3 p,Vector3 size,Material material)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);}
    }
}
