using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class SmallCraftReview
    {
        private const string Folder="Temp/SmallCraftReview";
        private static int stage=-1,checks,cycle;
        private static double deadline;
        private static Vector3 origin;
        private static float motionStartedAt;
        [InitializeOnLoadMethod] private static void Register(){EditorApplication.update-=Update;EditorApplication.update+=Update;}
        private static void Check(bool ok,string name){checks++;if(!ok){stage=-1;Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/runtime-result.txt","FAILED: "+name);throw new InvalidOperationException("Small craft: "+name);}}
        [MenuItem("Meridian/Small craft - Check Hull Geometry")]
        public static void RunChecks()
        {
            checks=0;Directory.CreateDirectory(Folder);
            foreach(var id in new[]{VesselId.Ruzgar,VesselId.Kumkale})
            {
                VesselDefinition definition=VesselCatalog.Get(id);
                Check(VesselCatalog.IsAvailable(id)&&definition.IsSmallCraft,"selectable identity "+id);
                Check(definition.EngineCount==(id==VesselId.Ruzgar?4:2),"distinct machinery "+id);
                Mesh mesh=SmallCraftHull.CreateMesh(id);
                try
                {
                    Check(Mathf.Abs(mesh.bounds.size.z-definition.LengthMetres/4)<.001f&&Mathf.Abs(mesh.bounds.size.x-definition.BeamMetres/4)<.001f,"measured hull proportions "+id);
                    var vertices=mesh.vertices;var indices=mesh.triangles;Dictionary<string,(int count,int direction)> edges=new();double volume=0;
                    string Key(Vector3 p)=>$"{Mathf.RoundToInt(p.x*100000)},{Mathf.RoundToInt(p.y*100000)},{Mathf.RoundToInt(p.z*100000)}";
                    void Edge(Vector3 p,Vector3 q)
                    {string a=Key(p),b=Key(q);int sign=string.CompareOrdinal(a,b)<0?1:-1;string key=sign>0?a+"/"+b:b+"/"+a;edges.TryGetValue(key,out var old);edges[key]=(old.count+1,old.direction+sign);}
                    foreach(Vector3 p in vertices)Check(float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z),"finite vertex "+id);
                    for(int i=0;i<indices.Length;i+=3)
                    {
                        Vector3 p=vertices[indices[i]],q=vertices[indices[i+1]],r=vertices[indices[i+2]];
                        Check(Vector3.Cross(q-p,r-p).sqrMagnitude>1e-12f,"nondegenerate hull triangle "+id);
                        volume+=Vector3.Dot(p,Vector3.Cross(q,r))/6.0;Edge(p,q);Edge(q,r);Edge(r,p);
                    }
                    foreach(var edge in edges.Values)Check(edge.count==2&&edge.direction==0,"closed consistently outward hull "+id);
                    Check(volume>0,"positive hull volume "+id);
                    for(int side=-1;side<=1;side+=2)for(int i=0;i<25;i++)
                    {
                        float z=3.8f+i*.065f;Vector3 p=SmallCraftHull.SurfacePoint(id,side,z,-.38f);
                        Check(p.x*side>0&&Mathf.Abs(p.z-z)<.0001f&&Mathf.Abs(p.y+.38f)<.0001f,"surface-aligned port/starboard pennants "+id);
                    }
                }
                finally{Object.DestroyImmediate(mesh);}
            }
            Check(!VesselCatalog.IsAvailable((VesselId)99)&&VesselCatalog.All.Count==4,"four actual platforms, invalid rejected");
            File.WriteAllText(Folder+"/logic-result.txt",$"SMALLCRAFT_GEOMETRY_PASS checks={checks}\n");Debug.Log($"SMALLCRAFT_GEOMETRY_PASS checks={checks}");
        }
        [MenuItem("Meridian/Small craft - Run Model Review")]
        public static void StartReview()
        {RunChecks();SessionState.SetBool("Meridian.SmallCraftReview",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;}
        private static void Begin(GameSessionController session,VesselId id)
        {
            Check(session.StartMode(SimulationMode.Harbor,id,new SeaStateSettings(2,315)),"launch "+id);
            deadline=EditorApplication.timeSinceStartup+1.2;
        }
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            var session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.SmallCraftReview",false))
            {SessionState.SetBool("Meridian.SmallCraftReview",false);checks=0;stage=0;cycle=0;File.WriteAllText(Folder+"/measurements.txt","");Begin(session,VesselId.Ruzgar);return;}
            if(stage<0||EditorApplication.timeSinceStartup<deadline)return;
            var ship=session.PlayerShip;var definition=ship.Definition;
            if(stage<2)
            {
                Transform visuals=ship.transform.Find("Visuals");
                Check(visuals.Find("Bridge")!=null,"closed bridge present "+definition.Id);
                Check(visuals.Find(definition.HasFourEngines?"Lattice mast":"Solid sensor mast")!=null,"distinct mast "+definition.Id);
                Check(visuals.Find(definition.HasFourEngines?"76 mm forward mount":"Twin 40 mm forward mount")!=null,"correct forward mount "+definition.Id);
                Check(visuals.GetComponent<SmallCraftDeckLife>().CrewCount==6,"small craft deck crew "+definition.Id);
                Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedEmittersFor(definition),"pooled effect count "+definition.Id);
                Check(visuals.GetComponentsInChildren<MeshRenderer>().Length<160,"bounded renderer count "+definition.Id);
                for(int engine=0;engine<definition.EngineCount;engine++)Check(visuals.Find("Propeller "+engine)!=null,"individual shaft "+definition.Id+"/"+engine);
                int vertices=0;foreach(var m in visuals.GetComponentsInChildren<MeshFilter>())vertices+=m.sharedMesh.vertexCount;
                File.AppendAllText(Folder+"/measurements.txt",$"{definition.Id} activeRenderers={visuals.GetComponentsInChildren<MeshRenderer>().Length} vertices={vertices} effects={ship.GetComponentsInChildren<ParticleSystem>().Length}\n");
                session.OpenMenu();CaptureViews(session);
                stage++;if(stage==1){Begin(session,VesselId.Kumkale);return;}
                StartMotion(session,VesselId.Ruzgar,1);return;
            }
            if(stage>=2&&stage<=5)
            {
                if(Time.time<motionStartedAt+6)return;
                bool forward=stage%2==0;float travel=Vector3.Dot(ship.transform.position-origin,Vector3.forward);
                Check(float.IsFinite(travel)&&(forward?travel>.15f:travel<-.15f),"real "+(forward?"ahead":"astern")+" physics "+definition.Id+" travel="+travel);
                Check(Mathf.Abs(ship.GetComponent<ShipBuoyancy>().RollDegrees)<30,"stable small craft motion "+definition.Id);
                File.AppendAllText(Folder+"/measurements.txt",$"stage={stage} {definition.Id} forward={forward} travel={travel:F3} speed={ship.SpeedKnots:F2}\n");
                stage++;
                if(stage==3){StartMotion(session,VesselId.Ruzgar,-1);return;}
                if(stage==4){StartMotion(session,VesselId.Kumkale,1);return;}
                if(stage==5){StartMotion(session,VesselId.Kumkale,-1);return;}
                deadline=EditorApplication.timeSinceStartup+.1;return;
            }
            if(stage==6)
            {
                if(cycle<16)
                {
                    var id=VesselCatalog.All[cycle%4];var mode=(cycle/4)%2==0?SimulationMode.Harbor:SimulationMode.StraitPassage;
                    Check(session.StartMode(mode,id),"switch cycle "+cycle);
                    cycle++;deadline=EditorApplication.timeSinceStartup+.3;stage=7;return;
                }
                Check(Object.FindObjectsByType<ShipController>(FindObjectsSortMode.None).Length==1,"single physics body after switches");
                Check(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length==1,"single player camera after switches");
                session.StartMode(SimulationMode.Harbor,VesselId.Ruzgar);session.OpenMenu();session.GetComponent<ModeSelectionMenu>().SelectVessel(VesselId.Ruzgar);
                File.WriteAllText(Folder+"/runtime-result.txt",$"SMALLCRAFT_RUNTIME_PASS checks={checks}\n");Debug.Log($"SMALLCRAFT_RUNTIME_PASS checks={checks}");stage=-1;return;
            }
            if(stage==7)
            {
                Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedEmittersFor(definition),"effect count after switch "+cycle);
                Check(ship.GetComponent<BoxCollider>().size==definition.ColliderSize,"matching collider "+cycle);
                Check(ship.EngineCount==definition.EngineCount&&ship.AllEnginesSynchronized,"matching propulsion and reset "+cycle);
                Check(ship.GetComponentsInChildren<SmallCraftMotion>().Length==(definition.IsSmallCraft?1:0),"one correct model motion "+cycle);
                for(int i=0;i<ship.EngineCount;i++)Check(ship.GetEngineThrottle(i)==0&&ship.GetEngineSignedLoad(i)==0,"all engines reset "+cycle+"/"+i);
                stage=6;deadline=EditorApplication.timeSinceStartup+.1;
            }
        }
        private static void StartMotion(GameSessionController session,VesselId id,float command)
        {session.StartMode(SimulationMode.Harbor,id,new SeaStateSettings(1,0));origin=session.PlayerShip.transform.position;session.PlayerShip.SetCombinedThrottle(command);motionStartedAt=Time.time;deadline=EditorApplication.timeSinceStartup+1;}
        private static void CaptureViews(GameSessionController session)
        {
            for(int view=0;view<9;view++)
            {
                var ship=session.PlayerShip;GameObject go=new("Small craft review camera");Camera camera=go.AddComponent<Camera>();camera.CopyFrom(Object.FindAnyObjectByType<FollowCamera>().GetComponent<Camera>());camera.enabled=false;
                float azimuth=view*45,pitch=view==8?78:13,distance=(view==8?42:view==0||view==4?34:29)*ship.Definition.CameraScale;
                Vector3 focus=ship.transform.position+Vector3.up*ship.Definition.CameraFocusHeight;
                camera.transform.position=focus+Quaternion.Euler(pitch,azimuth,0)*Vector3.back*distance;camera.transform.LookAt(focus);
                RenderTexture previous=RenderTexture.active;RenderTexture rt=new(1280,720,24);Texture2D image=new(1280,720,TextureFormat.RGB24,false);
                try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Folder+$"/{ship.Definition.Id}-{view:00}.png",image.EncodeToPNG());}
                finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(go);}
            }
        }
    }
}
