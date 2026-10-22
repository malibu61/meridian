using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Meridian.Editor
{
    public static class WaterVisualReview
    {
        private const string Folder="Temp/WaterVisualReview";
        private static int step=-1,checks;
        private static double deadline;
        private static Vector3 start;
        private static bool awaitingPause;
        private static float pausedTime;
        private static Vector3 pausedPosition;
        private static int pausedParticles;
        [InitializeOnLoadMethod]
        private static void Register(){EditorApplication.update-=Update;EditorApplication.update+=Update;}
        [MenuItem("Meridian/Water - Check Waves and Materials")]
        public static void RunChecks()
        {
            int count=0;void Check(bool result,string name){count++;if(!result)throw new InvalidOperationException("Water check: "+name);}
            Shader ocean=Resources.Load<Shader>("MaritimeOcean"),foam=Resources.Load<Shader>("MaritimeFoam");
            Check(ocean!=null&&foam!=null,"included shader resources");Check(ocean.isSupported&&foam.isSupported,"supported shaders");
            Check(!ShaderUtil.ShaderHasError(ocean)&&!ShaderUtil.ShaderHasError(foam),"shader compilation");
            for(int x=-50;x<=50;x+=5)for(int z=-50;z<=50;z+=5)
            {
                float h=WaterWaves.Height(x,z,1.25f);Vector3 n=WaterWaves.Normal(x,z,1.25f);
                Check(!float.IsNaN(h)&&Mathf.Abs(h)<=WaterWaves.AmplitudeBound+.0001f&&Mathf.Abs(n.magnitude-1)<.001f,"bounded finite wave "+x+"/"+z);
                const float d=.005f;float dx=(WaterWaves.Height(x+d,z,1.25f)-WaterWaves.Height(x-d,z,1.25f))/(2*d);
                float dz=(WaterWaves.Height(x,z+d,1.25f)-WaterWaves.Height(x,z-d,1.25f))/(2*d);
                Check(Vector3.Dot(n,new Vector3(-dx,1,-dz).normalized)>.9999f,"analytic normal "+x+"/"+z);
            }
            Check(Mathf.Abs(WaterWaves.Height(3,8,0)-WaterWaves.Height(3,8,1))>.001f,"animated waves");
            Check(WaterWaves.PropellerRate(0,0)==0&&WaterWaves.BowRate(0)==0,"no idle foam");
            Check(WaterWaves.PropellerRate(1,0)>WaterWaves.PropellerRate(.2f,0),"load response");
            Check(WaterWaves.BowRate(-3)==0&&WaterWaves.BowRate(3)>0,"no bow foam when reversing");
            Texture2D texture=WaterVisuals.CreateFoamTexture();
            try
            {
                for(int p=0;p<texture.width;p++)Check(texture.GetPixel(p,0).a==0&&texture.GetPixel(p,texture.height-1).a==0&&texture.GetPixel(0,p).a==0&&texture.GetPixel(texture.width-1,p).a==0,"transparent edges "+p);
                Check(texture.GetPixel(64,64).a>.05f,"foam center visible");Check(texture.mipmapCount>1,"foam mips");
            }
            finally{Object.DestroyImmediate(texture);}
            Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"/logic-result.txt",$"WATER_LOGIC_CHECK_PASS checks={count}\n");Debug.Log($"WATER_LOGIC_CHECK_PASS checks={count}");
        }
        [MenuItem("Meridian/Water - Run Live Review")]
        public static void StartReview()
        {
            RunChecks();SessionState.SetBool("Meridian.WaterReview",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;
        }
        private static void Check(bool condition,string name){checks++;if(!condition){step=-1;throw new InvalidOperationException("Water runtime: "+name);}}
        private static void Begin(GameSessionController session)
        {
            VesselId vessel=step<2?VesselId.Barbaros:VesselId.Anadolu;SimulationMode mode=step%2==0?SimulationMode.Harbor:SimulationMode.StraitPassage;
            session.StartMode(mode,vessel);ShipController ship=session.PlayerShip;
            Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedShipEmitters,"three wake, two spray and two smoke emitters "+step);
            WakeTrail wake=ship.GetComponent<WakeTrail>();Material material=ship.GetComponentInChildren<ParticleSystemRenderer>().sharedMaterial;
            // Find the actual wake material rather than a smoke material.
            foreach(var particle in ship.GetComponentsInChildren<ParticleSystem>())if(particle.name=="Port propeller wake")material=particle.GetComponent<ParticleSystemRenderer>().sharedMaterial;
            wake.Configure(ship,material);wake.Configure(ship,material);
            Check(ship.GetComponentsInChildren<ParticleSystem>().Length==WakeTrail.ExpectedShipEmitters,"Configure idempotent "+step);
            Check(ship.GetComponentsInChildren<BowWaveSheet>().Length==1,"one bow sheet "+step);
            OceanSurface water=Object.FindAnyObjectByType<OceanSurface>();
            Check(water!=null&&water.VertexCount<10000,"bounded water patch "+step);
            Check(water.GetComponent<MeshRenderer>().sharedMaterial.shader.name=="Meridian/Layered Ocean","ocean material "+step);
            Check(mode==SimulationMode.Harbor?Object.FindObjectsByType<ShorelineWash>(FindObjectsSortMode.None).Length==3:true,"harbor wash "+step);
            ship.SetCombinedThrottle(1);start=ship.transform.position;deadline=EditorApplication.timeSinceStartup+5;
        }
        private static void Update()
        {
            if(!Application.isPlaying||EditorApplication.isCompiling)return;
            GameSessionController session=Object.FindAnyObjectByType<GameSessionController>();if(session==null)return;
            if(SessionState.GetBool("Meridian.WaterReview",false)){SessionState.SetBool("Meridian.WaterReview",false);step=0;checks=0;awaitingPause=false;Begin(session);return;}
            if(step<0||EditorApplication.timeSinceStartup<deadline)return;
            ShipController ship=session.PlayerShip;
            if(awaitingPause)
            {
                awaitingPause=false;
                Check(Time.time==pausedTime,"scaled wave time remains frozen across frames "+step);
                Check(Vector3.Distance(ship.transform.position,pausedPosition)<.001f,"ship remains paused "+step);
                int count=0;foreach(ParticleSystem p in ship.GetComponentsInChildren<ParticleSystem>())count+=p.particleCount;
                Check(count==pausedParticles,"foam and smoke remain paused "+step);
                ship.ResetSimulation(ship.transform.position,ship.transform.rotation);
                foreach(ParticleSystem particle in ship.GetComponentsInChildren<ParticleSystem>())Check(particle.particleCount==0,"reset clears "+particle.name);
                step++;
                if(step<4){Begin(session);return;}
                step=-1;session.StartMode(SimulationMode.Harbor,VesselId.Barbaros);session.OpenMenu();
                File.WriteAllText(Folder+"/runtime-result.txt",$"WATER_RUNTIME_CHECK_PASS checks={checks}\n");Debug.Log($"WATER_RUNTIME_CHECK_PASS checks={checks}");Debug.Log("WATER_VISUAL_REVIEW_READY "+Path.GetFullPath(Folder));return;
            }
            Check(Vector3.Distance(ship.transform.position,start)>.1f,"real movement "+step);
            int budget=0;
            foreach(ParticleSystem particle in ship.GetComponentsInChildren<ParticleSystem>())
            {
                if(particle.name.Contains("exhaust")||particle.name.Contains("smoke"))continue;
                if(particle.name!="Port propeller wake"&&particle.name!="Starboard propeller wake"&&particle.name!="Bow surface foam")continue;
                budget+=particle.main.maxParticles;
                Check(particle.GetComponent<ParticleSystemRenderer>().renderMode==ParticleSystemRenderMode.HorizontalBillboard,"flat foam "+particle.name);
                Check(particle.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.name=="Meridian/Soft Surface Foam","alpha foam "+particle.name);
                if(particle.name.Contains("propeller"))Check(particle.particleCount>0,"actual foam emission "+particle.name);
            }
            Check(budget==800,"bounded foam particle budget "+step);
            Capture(session,$"{step:00}-{session.ActiveVessel}-{session.ActiveMode}");
            session.OpenMenu();Check(Time.timeScale==0,"pause "+step);
            pausedTime=Time.time;pausedPosition=ship.transform.position;pausedParticles=0;
            foreach(ParticleSystem p in ship.GetComponentsInChildren<ParticleSystem>())pausedParticles+=p.particleCount;
            awaitingPause=true;deadline=EditorApplication.timeSinceStartup+.6;
        }
        private static void Capture(GameSessionController session,string name)
        {
            ShipController ship=session.PlayerShip;GameObject go=new("Water review camera");Camera camera=go.AddComponent<Camera>();camera.CopyFrom(Object.FindAnyObjectByType<FollowCamera>().GetComponent<Camera>());camera.enabled=false;
            float scale=ship.Definition.CameraScale;Vector3 focus=ship.transform.position+Vector3.up*ship.Definition.CameraFocusHeight;
            camera.transform.position=focus+ship.transform.rotation*(Quaternion.Euler(22,54,0)*Vector3.back*(38*scale));camera.transform.LookAt(focus);
            RenderTexture previous=RenderTexture.active;RenderTexture rt=new(1280,720,24);Texture2D texture=new(1280,720,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Folder+"/"+name+".png",texture.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(go);}
        }
    }
}
