using UnityEngine;
using UnityEngine.Rendering;

namespace Meridian
{
    /// <summary>Independent, world-space exhaust for the two visible funnels.</summary>
    public sealed class EngineExhaustSmoke : MonoBehaviour
    {
        private ShipController ship;
        private readonly EngineExhaustResponse port=new(),starboard=new();
        private ParticleSystem portSmoke,starboardSmoke;
        private Material smokeMaterial;
        private Texture2D smokeTexture;
        private bool configured;
        public EngineExhaustResponse Port => port;
        public EngineExhaustResponse Starboard => starboard;
        public int ParticleCount => (portSmoke==null?0:portSmoke.particleCount)+(starboardSmoke==null?0:starboardSmoke.particleCount);
        public bool IsBoundToShip => ship!=null;

        public void Configure(Transform portOutlet,Transform starboardOutlet)
        {
            if(configured)return;
            configured=true;
            smokeTexture=CreateCloudTexture();
            Shader shader=Resources.Load<Shader>("FunnelSmoke")??Shader.Find("Meridian/Funnel Smoke");
            if(shader==null)shader=Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            smokeMaterial=new Material(shader){name="Soft grey engine exhaust",mainTexture=smokeTexture};
            portSmoke=CreateEmitter(portOutlet,"Port funnel smoke",7831);
            starboardSmoke=CreateEmitter(starboardOutlet,"Starboard funnel smoke",9539);
            Apply(portSmoke,port,0f);Apply(starboardSmoke,starboard,0f);
        }

        private void Start()
        {
            ship=GetComponentInParent<ShipController>();
            if(ship!=null){port.Reset(ship.PortThrottle,ship.PortEngineLoad);starboard.Reset(ship.StarboardThrottle,ship.StarboardEngineLoad);}
        }

        private void Update()
        {
            if(!configured || ship==null)return;
            Tick(ship.PortThrottle,ship.PortEngineLoad,ship.StarboardThrottle,ship.StarboardEngineLoad,Time.deltaTime);
        }

        private void Tick(float portCommand,float portActual,float starboardCommand,float starboardActual,float dt)
        {
            float oldPort=port.Surge,oldStarboard=starboard.Surge;
            port.Step(portCommand,portActual,dt);starboard.Step(starboardCommand,starboardActual,dt);
            Apply(portSmoke,port,port.Surge-oldPort);Apply(starboardSmoke,starboard,starboard.Surge-oldStarboard);
        }

        private ParticleSystem CreateEmitter(Transform outlet,string name,uint seed)
        {
            GameObject go=new(name);go.transform.SetParent(outlet,false);
            go.transform.localRotation=Quaternion.Euler(-90f,0f,0f);
            ParticleSystem ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.useAutoRandomSeed=false;ps.randomSeed=seed;
            var main=ps.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.emitterVelocityMode=ParticleSystemEmitterVelocityMode.Transform;
            main.startLifetime=new ParticleSystem.MinMaxCurve(2.8f,4.3f);main.startRotation=new ParticleSystem.MinMaxCurve(0f,Mathf.PI*2f);
            main.maxParticles=240;main.gravityModifier=0f;main.cullingMode=ParticleSystemCullingMode.Automatic;
            var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.15f;shape.angle=14f;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;
            velocity.x=.075f;velocity.y=0f;velocity.z=-.09f;
            var inherit=ps.inheritVelocity;inherit.enabled=true;inherit.mode=ParticleSystemInheritVelocityMode.Initial;inherit.curveMultiplier=.75f;
            var noise=ps.noise;noise.enabled=true;noise.quality=ParticleSystemNoiseQuality.Low;noise.frequency=.65f;noise.strength=.14f;noise.scrollSpeed=.15f;
            var size=ps.sizeOverLifetime;size.enabled=true;
            size.size=new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new Keyframe(0f,.6f),new Keyframe(.25f,1.7f),new Keyframe(1f,5.2f)));
            Gradient fade=new();fade.SetKeys(new[]{new GradientColorKey(Color.white,0f),new GradientColorKey(new Color(.88f,.90f,.93f),1f)},
                new[]{new GradientAlphaKey(0f,0f),new GradientAlphaKey(1f,.06f),new GradientAlphaKey(.45f,.55f),new GradientAlphaKey(0f,1f)});
            var color=ps.colorOverLifetime;color.enabled=true;color.color=fade;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=smokeMaterial;renderer.renderMode=ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            ps.Play(true);return ps;
        }

        private static void Apply(ParticleSystem ps,EngineExhaustResponse state,float surgeIncrease)
        {
            var main=ps.main;
            float grey=Mathf.Lerp(.51f,.075f,state.Darkness);
            main.startColor=new ParticleSystem.MinMaxGradient(new Color(grey,grey*1.02f,grey*1.06f,state.Opacity),new Color(grey*.83f,grey*.86f,grey*.90f,state.Opacity*.80f));
            main.startSpeed=new ParticleSystem.MinMaxCurve(state.UpwardSpeed*.80f,state.UpwardSpeed*1.16f);
            main.startSize=new ParticleSystem.MinMaxCurve(Mathf.Lerp(.22f,.16f,state.Surge),Mathf.Lerp(.34f,.26f,state.Surge));
            var emission=ps.emission;emission.rateOverTime=state.EmissionRate;
            var shape=ps.shape;shape.angle=Mathf.Lerp(14f,5f,state.Surge);
            if(surgeIncrease>.25f)ps.Emit(Mathf.RoundToInt(18f*surgeIncrease));
        }

        private static Texture2D CreateCloudTexture()
        {
            const int size=64;
            Texture2D texture=new(size,size,TextureFormat.RGBA32,false){name="Procedural soft smoke cloud",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            Color[] pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+.5f)/size,v=(y+.5f)/size,r=new Vector2((u-.5f)*2f,(v-.5f)*2f).magnitude;
                float edge=1f-Mathf.SmoothStep(.10f,1f,r);
                float detail=Mathf.PerlinNoise(u*5.3f+7.1f,v*5.3f+2.3f)*.65f+Mathf.PerlinNoise(u*11f+3f,v*11f)*.35f;
                pixels[y*size+x]=new Color(1f,1f,1f,Mathf.Pow(edge,1.25f)*(.55f+detail*.45f));
            }
            texture.SetPixels(pixels);texture.Apply(false,true);return texture;
        }

#if UNITY_EDITOR
        public void ResetStudy(float portCommand,float portActual,float starboardCommand,float starboardActual)
        {
            port.Reset(portCommand,portActual);starboard.Reset(starboardCommand,starboardActual);
            portSmoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);starboardSmoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            portSmoke.Play(true);starboardSmoke.Play(true);Apply(portSmoke,port,0f);Apply(starboardSmoke,starboard,0f);
        }
        public void StepStudy(float portCommand,float portActual,float starboardCommand,float starboardActual,float dt)
        {
            Tick(portCommand,portActual,starboardCommand,starboardActual,dt);
            // Simulate pauses the system; resume it before the next manual review step.
            portSmoke.Play(true);starboardSmoke.Play(true);
            portSmoke.Simulate(dt,true,false,false);starboardSmoke.Simulate(dt,true,false,false);
        }
#endif

        private void OnDestroy()
        {
            if(portSmoke!=null)Destroy(portSmoke.gameObject);if(starboardSmoke!=null)Destroy(starboardSmoke.gameObject);
            if(smokeMaterial!=null)Destroy(smokeMaterial);if(smokeTexture!=null)Destroy(smokeTexture);
        }
    }
}
