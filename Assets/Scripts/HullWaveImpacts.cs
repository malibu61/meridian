using UnityEngine;
using UnityEngine.Rendering;
namespace Meridian
{
    public sealed class HullWaveImpacts:MonoBehaviour
    {
        private ShipController ship;
        private Rigidbody body;
        private WakeTrail wake;
        private ParticleSystem port,starboard;
        private readonly float[] nextImpact=new float[8];
        private readonly System.Random random=new(74401);
        public int TotalBursts { get; private set; }
        public int PortBursts { get; private set; }
        public int StarboardBursts { get; private set; }
        public const int SprayBudget=448;
        public void Configure(ShipController controller,Material foam)
        {
            ship=controller;body=ship.GetComponent<Rigidbody>();wake=ship.GetComponent<WakeTrail>();
            if(port==null){port=Create("Port hull spray",foam);starboard=Create("Starboard hull spray",foam);}
            ResetForMode();
        }
        public void ResetForMode()
        {
            if(port!=null)port.Clear(true);if(starboard!=null)starboard.Clear(true);
            for(int i=0;i<nextImpact.Length;i++)nextImpact[i]=Time.time+.15f;
            TotalBursts=PortBursts=StarboardBursts=0;
        }
        public static float Strength(float heightAboveWaterline,float relativeRise,float facing)
            =>Mathf.Clamp((Mathf.Max(0,heightAboveWaterline-.025f)*.8f+Mathf.Max(0,relativeRise-.06f)*.75f)*Mathf.Clamp01(facing),0,1.5f);
        private void Update()
        {
            if(ship==null||port==null||Time.deltaTime<=0)return;
            for(int side=-1;side<=1;side+=2)for(int sample=0;sample<4;sample++)
            {
                int index=(side<0?0:4)+sample;if(Time.time<nextImpact[index])continue;
                float z=ship.Definition.ColliderSize.z*(sample/3f-.5f)*.72f;
                float width=ship.Definition.IsSmallCraft?SmallCraftHull.HalfWidth(ship.Definition.Id,z)*.86f:ship.Definition.UsesPods?AnadoluModelBuilder.WaterlineHalfWidth(z):BarbarosModelBuilder.HalfWidth(z)*.85f;
                Vector3 p=ship.transform.TransformPoint(new Vector3(side*(width+.06f),-.82f,z));
                Vector3 outward=ship.RightOnWater*side;
                Vector3 fluid=WaterWaves.OrbitalVelocity(p,Time.time);
                float incoming=Vector3.Dot(Vector3.ProjectOnPlane(fluid,Vector3.up),-outward);
                float facing=Mathf.Clamp01(incoming/.16f);
                float water=WaterWaves.SurfaceY(p,Time.time),rise=fluid.y-body.GetPointVelocity(p).y;
                float intensity=Strength(water-p.y,rise,facing);
                if(intensity<.09f)continue;
                // Emit outside the flared hull and on the crest, never hidden below opaque water.
                float outer=ship.Definition.IsSmallCraft?SmallCraftHull.HalfWidth(ship.Definition.Id,z):ship.Definition.UsesPods?AnadoluModelBuilder.DeckHalfWidth(z):BarbarosModelBuilder.HalfWidth(z);
                p=ship.transform.TransformPoint(new Vector3(side*(outer+.10f),0,z));p.y=WaterWaves.SurfaceY(p,Time.time)+.08f;
                Burst(side<0?port:starboard,p,outward,intensity);
                wake.EmitHullFoam(p,intensity,side);
                nextImpact[index]=Time.time+.65f+(float)random.NextDouble()*.4f;
                TotalBursts++;if(side<0)PortBursts++;else StarboardBursts++;
            }
        }
        private void Burst(ParticleSystem effect,Vector3 position,Vector3 outward,float strength)
        {
            int count=Mathf.RoundToInt(7+strength*18);
            for(int i=0;i<count;i++)
            {
                float r=(float)random.NextDouble(),jitter=(float)random.NextDouble()-.5f;
                ParticleSystem.EmitParams emit=new()
                {
                    position=position+ship.ForwardOnWater*jitter*.65f,
                    velocity=outward*(.5f+r*.7f+strength*.8f)+Vector3.up*(1.1f+strength*2.5f+r*.7f)+ship.ForwardOnWater*jitter+body.linearVelocity*.15f,
                    startSize=.12f+r*.18f,startLifetime=.8f+r*.6f,startColor=new Color(.90f,.97f,1f,.9f)
                };
                if(i%7==0){emit.startSize=.45f+r*.35f;emit.velocity*=.5f;emit.startColor=new Color(.85f,.94f,.97f,.45f);}
                effect.Emit(emit,1);
            }
        }
        private ParticleSystem Create(string name,Material material)
        {
            GameObject go=new(name);go.transform.SetParent(transform,false);ParticleSystem effect=go.AddComponent<ParticleSystem>();effect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=effect.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=SprayBudget/2;
            main.startLifetime=1;main.startSpeed=0;main.startSize=.14f;main.gravityModifier=.55f;
            var emission=effect.emission;emission.enabled=false;var shape=effect.shape;shape.enabled=false;
            var color=effect.colorOverLifetime;color.enabled=true;Gradient gradient=new();
            gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.72f,.85f,.9f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.85f,.08f),new GradientAlphaKey(.4f,.55f),new GradientAlphaKey(0,1)});color.color=gradient;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=1.35f;renderer.velocityScale=.07f;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.sortMode=ParticleSystemSortMode.Distance;effect.Play();return effect;
        }
    }
}
