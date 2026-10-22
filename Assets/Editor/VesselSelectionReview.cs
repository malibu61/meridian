using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Meridian.Editor
{
    public static class VesselSelectionReview
    {
        private static int runtimeStep = -1, runtimeChecks;
        private static int motionStage;
        private static double deadline;
        private static Vector3 motionStart, pausePosition;
        private const string Folder = "Temp/VesselSelectionReview";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.delayCall += RunChecks;
            EditorApplication.update -= UpdateReview; EditorApplication.update += UpdateReview;
        }

        [MenuItem("Meridian/Vessels - Check Geometry and Profiles")]
        public static void RunChecks()
        {
            int checks = 0;
            void Check(bool result,string name) { checks++; if(!result)throw new InvalidOperationException("Vessel check: " + name); }
            VesselDefinition b=VesselCatalog.Get(VesselId.Barbaros),a=VesselCatalog.Get(VesselId.Anadolu);
            Check(VesselCatalog.IsAvailable(b.Id) && VesselCatalog.IsAvailable(a.Id),"both vessels available");
            Check(!VesselCatalog.IsAvailable((VesselId)99),"invalid vessel unavailable");
            Check(b.Pennant=="F-244" && a.Pennant=="L-400","identities");
            Check(Mathf.Abs(a.LengthMetres-230.8f)<.01f && a.BeamMetres==32f,"Anadolu dimensions");
            Check(!b.UsesPods && a.UsesPods,"distinct steering types");
            Check(a.GameMass>b.GameMass && a.EngineResponse<b.EngineResponse,"heavier slower response");
            Check(a.CameraScale>b.CameraScale && a.CameraFocusHeight>b.CameraFocusHeight,"camera adapts");
            Check(a.ColliderSize.z>b.ColliderSize.z && a.ColliderSize.x>b.ColliderSize.x,"collider adapts");
            Check(a.HarborSpawn.z+a.ColliderSize.z*.5f<0f,"Anadolu clear of harbor entrance");
            Check(Mathf.Abs(a.SternWakeZ)>a.ColliderSize.z*.4f && a.BowWakeZ>a.ColliderSize.z*.4f,"wake endpoints fit");
            Check(AnadoluModelBuilder.FlightDeckHeight(-1f,28f)>AnadoluModelBuilder.FlightDeckHeight(-1f,15f)+1f,"raised port ski jump");
            Check(Mathf.Abs(AnadoluModelBuilder.FlightDeckHeight(3f,28f)-AnadoluModelBuilder.DeckLevel)<.001f,"starboard forward platform remains separate");
            Mesh mesh=AnadoluModelBuilder.CreateHullMesh();
            try
            {
                Check(Mathf.Abs(mesh.bounds.size.z-57.7f)<.01f && Mathf.Abs(mesh.bounds.size.x-8f)<.01f,"mesh real aspect ratio");
                Check(mesh.subMeshCount==3,"hull deck and underwater materials");
                Vector3[] vertices=mesh.vertices; int[] triangles=mesh.triangles;
                Dictionary<string,(int count,int direction)> edges=new();
                string Key(Vector3 p)=>$"{Mathf.RoundToInt(p.x*10000f)},{Mathf.RoundToInt(p.y*10000f)},{Mathf.RoundToInt(p.z*10000f)}";
                bool finite=true,nonDegenerate=true; double volume=0;
                foreach(Vector3 v in vertices) if(float.IsNaN(v.x)||float.IsInfinity(v.x)||float.IsNaN(v.y)||float.IsInfinity(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.z))finite=false;
                for(int i=0;i<triangles.Length;i+=3)
                {
                    Vector3 p=vertices[triangles[i]],q=vertices[triangles[i+1]],r=vertices[triangles[i+2]];
                    if(Vector3.Cross(q-p,r-p).sqrMagnitude<1e-12f)nonDegenerate=false;
                    volume+=Vector3.Dot(p,Vector3.Cross(q,r))/6.0;
                    Edge(p,q); Edge(q,r); Edge(r,p);
                }
                void Edge(Vector3 p,Vector3 q)
                {
                    string pKey=Key(p),qKey=Key(q); int sign=string.CompareOrdinal(pKey,qKey)<0?1:-1;
                    string key=sign>0?pKey+"/"+qKey:qKey+"/"+pKey;
                    edges.TryGetValue(key,out var entry);edges[key]=(entry.count+1,entry.direction+sign);
                }
                bool closed=true,oriented=true;
                foreach(var edge in edges.Values){if(edge.count!=2)closed=false;if(edge.direction!=0)oriented=false;}
                Check(finite,"finite hull vertices"); Check(nonDegenerate,"no degenerate faces");
                Check(closed,"closed hull including bow ramp and stern");
                if(!oriented)foreach(var entry in edges)if(entry.Value.direction!=0)Debug.LogWarning("VESSEL_EDGE_DIRECTION "+entry.Key+" "+entry.Value);
                Check(oriented,"consistent edge winding"); Check(volume>0,"positive outward volume "+volume);
                foreach(int side in new[]{-1,1})foreach(float z in new[]{-26f,-24f,19f,20.7f,22f})
                {
                    Vector3 p=AnadoluModelBuilder.HullSurfacePoint(side,z,2.3f);
                    Check(Mathf.Abs(p.z-z)<.001f && Mathf.Abs(p.y-2.3f)<.001f && p.x*side>0f,"surface-aligned pennant "+side+"/"+z);
                }
            }
            finally { Object.DestroyImmediate(mesh); }
            Debug.Log($"VESSEL_GEOMETRY_CHECK_PASS checks={checks}");
        }

        [MenuItem("Meridian/Vessels - Run Full Review")]
        public static void StartReview()
        {
            Directory.CreateDirectory(Folder);
            SessionState.SetBool("Meridian.VesselReview.Start",true);
            if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;
        }

        private static void Check(bool condition,string name)
        { runtimeChecks++;if(!condition)throw new InvalidOperationException("Vessel runtime: "+name); }

        private static void UpdateReview()
        {
            if(!Application.isPlaying || EditorApplication.isCompiling)return;
            GameSessionController session=Object.FindAnyObjectByType<GameSessionController>();
            if(session==null)return;
            if(SessionState.GetBool("Meridian.VesselReview.Start",false))
            {
                SessionState.SetBool("Meridian.VesselReview.Start",false);
                runtimeChecks=0;runtimeStep=0;motionStage=0;
                session.StartMode(SimulationMode.Harbor,VesselId.Barbaros);session.OpenMenu();
                ModeSelectionMenu menu=session.GetComponent<ModeSelectionMenu>();
                Check(menu.SelectVessel(VesselId.Anadolu),"Anadolu card selects");
                Check(session.ActiveVessel==VesselId.Barbaros,"pending selection does not replace active vessel");
                Check(!menu.SelectVessel((VesselId)99) && menu.SelectedVessel==VesselId.Anadolu,"invalid selection ignored");
                Check(!session.StartMode(SimulationMode.BuoyMooring,VesselId.Anadolu) && session.ActiveVessel==VesselId.Barbaros,"unavailable mode cannot change ship");
                Check(!session.StartMode(SimulationMode.Harbor,(VesselId)99),"invalid vessel cannot launch");
                session.Resume();Check(session.ActiveVessel==VesselId.Barbaros,"resume preserves active ship");session.OpenMenu();
                deadline=EditorApplication.timeSinceStartup+.3;
            }
            if(runtimeStep>=0 && EditorApplication.timeSinceStartup>=deadline)
            {
                if(runtimeStep<8)
                {
                    VesselId id=runtimeStep%4==0||runtimeStep%4==3?VesselId.Barbaros:VesselId.Anadolu;
                    SimulationMode mode=runtimeStep%2==0?SimulationMode.Harbor:SimulationMode.StraitPassage;
                    Check(session.StartMode(mode,id),"launch combination "+runtimeStep);
                    ShipController ship=session.PlayerShip;VesselDefinition definition=VesselCatalog.Get(id);
                    Check(session.ActiveVessel==id && ship.Definition==definition,"correct ship profile "+runtimeStep);
                    Check(ship.GetComponent<BoxCollider>().size==definition.ColliderSize,"correct collider "+runtimeStep);
                    Check(ship.GetComponent<Rigidbody>().mass==definition.GameMass,"correct game inertia "+runtimeStep);
                    FollowCamera camera=Object.FindAnyObjectByType<FollowCamera>();
                    Check(Mathf.Abs(camera.CurrentDistance-34f*definition.CameraScale)<.01f && Mathf.Abs(camera.FocusHeight-definition.CameraFocusHeight)<.001f,"framing "+runtimeStep);
                    Check(ship.PortThrottle==0f && ship.StarboardThrottle==0f && ship.Rudder==0f,"controls reset "+runtimeStep);
                    Check(ship.transform.Find("Visuals")!=null && ship.transform.Find("Visuals").gameObject.activeSelf,"one named active model "+runtimeStep);
                    Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedShipEmitters,"no duplicated wakes/exhaust/spray "+runtimeStep);
                    Check(ship.Definition.Pennant==(id==VesselId.Anadolu?"L-400":"F-244"),"HUD identity "+runtimeStep);
                    ship.SetRudder(1f);
                    Check(id==VesselId.Anadolu?Vector3.Dot(ship.PropulsionDirection,ship.transform.right)<-.4f:Vector3.Dot(ship.PropulsionDirection,ship.transform.forward)>.99f,"pod/rudder separation "+runtimeStep);
                    ship.SetPortThrottle(1f);ship.SetStarboardThrottle(-1f);ship.SetRudder(0f);
                    session.OpenMenu();Check(!session.CanControl && Time.timeScale==0f,"pause "+runtimeStep);
                    runtimeStep++;deadline=EditorApplication.timeSinceStartup+.35;
                }
                else
                {
                    runtimeStep=-1;
                    Check(Object.FindObjectsByType<ShipController>(FindObjectsSortMode.None).Length==1,"single player body after switches");
                    Check(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length==1,"single camera after switches");
                    session.StartMode(SimulationMode.Harbor,VesselId.Anadolu);
                    motionStart=session.PlayerShip.transform.position;
                    session.PlayerShip.SetPortThrottle(1f);session.PlayerShip.SetStarboardThrottle(1f);
                    motionStage=1;deadline=EditorApplication.timeSinceStartup+4.0;
                }
            }
            if(motionStage==0 || EditorApplication.timeSinceStartup<deadline)return;
            ShipController player=session.PlayerShip;
            if(motionStage==1)
            {
                float travel=Vector3.Distance(motionStart,player.transform.position);
                Check(travel>.07f,"Anadolu moves in real FixedUpdate");
                Debug.Log($"ANADOLU_MOTION_PASS travel={travel:F3} load={player.PortEngineLoad:F2}");
                player.SetRudder(1f);motionStage=2;deadline=EditorApplication.timeSinceStartup+4.0;
            }
            else if(motionStage==2)
            {
                float heading=Vector3.SignedAngle(Vector3.forward,player.transform.forward,Vector3.up);
                Check(heading>.05f,"limited pod steering turns bow starboard");
                Debug.Log($"ANADOLU_POD_STEERING_PASS heading={heading:F2}");
                session.OpenMenu();pausePosition=player.transform.position;motionStage=3;deadline=EditorApplication.timeSinceStartup+.6;
            }
            else
            {
                Check(Vector3.Distance(pausePosition,player.transform.position)<.001f,"Anadolu paused with menu");
                session.GetComponent<ModeSelectionMenu>().SelectVessel(VesselId.Anadolu);
                motionStage=0;
                Debug.Log($"VESSEL_RUNTIME_CHECK_PASS checks={runtimeChecks}");
                CaptureAnadolu();
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(Folder+"/menu-anadolu.png"));
            }
        }

        [MenuItem("Meridian/Vessels - Capture Anadolu")]
        public static void CaptureAnadolu()
        {
            ShipController ship=Object.FindAnyObjectByType<ShipController>();
            if(ship==null || ship.Definition.Id!=VesselId.Anadolu){Debug.LogWarning("Select and start TCG Anadolu first.");return;}
            Directory.CreateDirectory(Folder);
            Transform[] transforms=ship.GetComponentsInChildren<Transform>();int[] originalLayers=new int[transforms.Length];
            for(int i=0;i<transforms.Length;i++){originalLayers[i]=transforms[i].gameObject.layer;transforms[i].gameObject.layer=31;}
            ParticleSystemRenderer[] particles=ship.GetComponentsInChildren<ParticleSystemRenderer>();
            bool[] particleEnabled=new bool[particles.Length];
            for(int i=0;i<particles.Length;i++){particleEnabled[i]=particles[i].enabled;particles[i].enabled=false;}
            GameObject cameraObject=new("Temporary Anadolu review camera");Camera camera=cameraObject.AddComponent<Camera>();camera.enabled=false;
            camera.cullingMask=1<<31;camera.fieldOfView=36f;camera.nearClipPlane=.1f;camera.farClipPlane=350f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.21f,.27f);
            string[] names={"01-port","02-starboard","03-bow","04-stern","05-port-bow","06-starboard-stern","07-top","08-low-port"};
            float[] yaws={-90f,90f,180f,0f,-135f,45f,0f,-90f};float[] pitches={7f,7f,8f,8f,24f,24f,89f,1f};
            RenderTexture previous=RenderTexture.active;RenderTexture rt=new(1280,720,24);
            Texture2D contact=new(2560,720,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;
                for(int i=0;i<8;i++)
                {
                    Vector3 focus=ship.transform.position+Vector3.up*(i==7?1.8f:4.6f);
                    camera.transform.position=focus+ship.transform.rotation*Quaternion.Euler(pitches[i],yaws[i],0f)*Vector3.back*(i==6?103f:78f);
                    camera.transform.LookAt(focus);camera.Render();RenderTexture.active=rt;
                    Texture2D texture=new(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
                    File.WriteAllBytes(Path.GetFullPath(Folder+"/"+names[i]+".png"),texture.EncodeToPNG());
                    Color[] pixels=new Color[640*360];for(int y=0;y<360;y++)for(int x=0;x<640;x++)pixels[y*640+x]=texture.GetPixel(x*2,y*2);
                    contact.SetPixels(i%4*640,(1-i/4)*360,640,360,pixels);Object.DestroyImmediate(texture);
                }
                contact.Apply();File.WriteAllBytes(Path.GetFullPath(Folder+"/contact-sheet.png"),contact.EncodeToPNG());
                Debug.Log("ANADOLU_VISUAL_REVIEW_COMPLETE "+Path.GetFullPath(Folder));
            }
            finally
            {
                for(int i=0;i<transforms.Length;i++)if(transforms[i]!=null)transforms[i].gameObject.layer=originalLayers[i];
                for(int i=0;i<particles.Length;i++)if(particles[i]!=null)particles[i].enabled=particleEnabled[i];
                camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);
                Object.DestroyImmediate(contact);Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
