using UnityEngine;
using UnityEngine.Rendering;
namespace Meridian
{
    public sealed class WakeTrail:MonoBehaviour
    {
        public const int ExpectedShipEmitters=7;
        public static int ExpectedEmittersFor(VesselDefinition definition)=>definition.HasFourEngines?9:ExpectedShipEmitters;
        private ShipController ship;
        private ParticleSystem portWake,starboardWake,bowSpray;
        private ParticleSystem portInnerWake,starboardInnerWake;
        private readonly ParticleSystem.Particle[] particleBuffer=new ParticleSystem.Particle[320];
        private BowWaveSheet bowSheet;
        public void Configure(ShipController controller,Material foamMaterial)
        {
            ship=controller;
            if(portWake==null)
            {
                portWake=CreateEmitter("Port propeller wake",foamMaterial,false);
                starboardWake=CreateEmitter("Starboard propeller wake",foamMaterial,false);
                bowSpray=CreateEmitter("Bow surface foam",foamMaterial,true);
                GameObject sheet=new("Diverging bow wave");sheet.transform.SetParent(transform,false);bowSheet=sheet.AddComponent<BowWaveSheet>();
            }
            if(ship.Definition.HasFourEngines&&portInnerWake==null)
            {
                portInnerWake=CreateEmitter("Port inner propeller wake",foamMaterial,false);
                starboardInnerWake=CreateEmitter("Starboard inner propeller wake",foamMaterial,false);
                var p=portInnerWake.main;p.maxParticles=160;var s=starboardInnerWake.main;s.maxParticles=160;
            }
            if(portInnerWake!=null)
            {
                portInnerWake.Clear(true);starboardInnerWake.Clear(true);
                SetRate(portInnerWake,0);SetRate(starboardInnerWake,0);
                portInnerWake.gameObject.SetActive(ship.Definition.HasFourEngines);starboardInnerWake.gameObject.SetActive(ship.Definition.HasFourEngines);
                if(ship.Definition.HasFourEngines){portInnerWake.Play();starboardInnerWake.Play();}
            }
            bowSheet.Configure(ship,foamMaterial);
            HullWaveImpacts impacts=GetComponent<HullWaveImpacts>()??gameObject.AddComponent<HullWaveImpacts>();impacts.Configure(ship,foamMaterial);
            ResetForMode();
        }
        public void ResetForMode()
        {
            if(portWake==null)return;
            portWake.Clear(true);starboardWake.Clear(true);bowSpray.Clear(true);
            SetRate(portWake,0);SetRate(starboardWake,0);SetRate(bowSpray,0);
            if(portInnerWake!=null){portInnerWake.Clear(true);starboardInnerWake.Clear(true);SetRate(portInnerWake,0);SetRate(starboardInnerWake,0);}
            GetComponent<HullWaveImpacts>()?.ResetForMode();
        }
        public void EmitHullFoam(Vector3 position,float strength,int side)
        {
            if(portWake==null)return;
            position.y=WaterWaves.SurfaceY(position,Time.time)+.08f;
            ParticleSystem.EmitParams emit=new(){position=position,velocity=Vector3.zero,startSize=.5f+strength*.5f,startLifetime=2.6f,startColor=new Color(.88f,.96f,.98f,.65f)};
            (side<0?portWake:starboardWake).Emit(emit,Mathf.RoundToInt(2+strength*4));
        }
        private void Update()
        {
            if(ship==null||portWake==null)return;
            VesselDefinition definition=ship.Definition;
            float scale=definition.EffectScale;
            UpdatePropeller(portWake,-definition.WakeSpacing,ship.GetEngineSignedLoad(0),scale);
            UpdatePropeller(starboardWake,definition.WakeSpacing,ship.GetEngineSignedLoad(1),scale);
            if(definition.HasFourEngines)
            {
                UpdatePropeller(portInnerWake,definition.EngineLocalX(2),ship.GetEngineSignedLoad(2),scale);
                UpdatePropeller(starboardInnerWake,definition.EngineLocalX(3),ship.GetEngineSignedLoad(3),scale);
            }
            Position(bowSpray,new Vector3(0,0,definition.BowWakeZ+.18f),-ship.transform.forward);
            float speed=Vector3.Dot(ship.GetComponent<Rigidbody>().linearVelocity,ship.transform.forward);
            SetRate(bowSpray,WaterWaves.BowRate(speed));
        }
        private void UpdatePropeller(ParticleSystem effect,float x,float signedLoad,float scale)
        {
            Position(effect,new Vector3(x,0,ship.Definition.SternWakeZ),(signedLoad<-.02f?1:-1)*ship.PropulsionDirection);
            SetRate(effect,WaterWaves.PropellerRate(signedLoad,ship.SpeedKnots));
            var main=effect.main;main.startSize=new ParticleSystem.MinMaxCurve(.45f*scale,1.15f*scale);
            main.startSpeed=new ParticleSystem.MinMaxCurve(.25f+Mathf.Abs(signedLoad)*.35f,.65f+Mathf.Abs(signedLoad)*.8f);
        }
        private void Position(ParticleSystem effect,Vector3 local,Vector3 direction)
        {
            Vector3 p=ship.transform.TransformPoint(local);p.y=WaterWaves.SurfaceY(p,Time.time)+.08f;
            effect.transform.SetPositionAndRotation(p,Quaternion.LookRotation(direction,Vector3.up));
        }
        private void LateUpdate()
        {
            if(portWake==null)return;
            FloatOnWaves(portWake);FloatOnWaves(starboardWake);FloatOnWaves(bowSpray);
            if(ship.Definition.HasFourEngines){FloatOnWaves(portInnerWake);FloatOnWaves(starboardInnerWake);}
        }
        private void FloatOnWaves(ParticleSystem effect)
        {
            int count=effect.GetParticles(particleBuffer);
            for(int i=0;i<count;i++)
            {
                Vector3 p=particleBuffer[i].position;p.y=WaterWaves.SurfaceY(p,Time.time)+.08f;
                particleBuffer[i].position=p;
            }
            effect.SetParticles(particleBuffer,count);
        }
        private ParticleSystem CreateEmitter(string name,Material material,bool bow)
        {
            GameObject go=new(name);go.transform.SetParent(transform,false);ParticleSystem effect=go.AddComponent<ParticleSystem>();effect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=effect.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=bow?160:320;
            main.startLifetime=bow?new ParticleSystem.MinMaxCurve(.8f,1.6f):new ParticleSystem.MinMaxCurve(3.2f,5.2f);
            main.startSize=bow?new ParticleSystem.MinMaxCurve(.25f,.65f):new ParticleSystem.MinMaxCurve(.45f,1.15f);
            main.startSpeed=new ParticleSystem.MinMaxCurve(.3f,.8f);main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
            main.startColor=new Color(.91f,.98f,1f,bow?.55f:.70f);
            var emission=effect.emission;emission.rateOverTime=0;
            var shape=effect.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=bow?48:16;shape.radius=bow?.24f:.24f;
            var color=effect.colorOverLifetime;color.enabled=true;Gradient gradient=new();
            gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.66f,.84f,.86f),1)},
                new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.12f),new GradientAlphaKey(.55f,.55f),new GradientAlphaKey(0,1)});color.color=gradient;
            var size=effect.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.45f,1,2f));
            var rotation=effect.rotationOverLifetime;rotation.enabled=true;rotation.z=new ParticleSystem.MinMaxCurve(-.18f,.18f);
            ParticleSystemRenderer renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.HorizontalBillboard;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.sortMode=ParticleSystemSortMode.Distance;
            effect.Play();return effect;
        }
        private static void SetRate(ParticleSystem effect,float rate){var emission=effect.emission;emission.rateOverTime=rate;}
    }
}
