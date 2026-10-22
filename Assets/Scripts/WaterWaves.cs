using UnityEngine;
namespace Meridian
{
    public static class WaterWaves
    {
        public const float SeaLevel=-.72f,TransitionSeconds=4f;
        private static SeaStateSettings target=SeaStateSettings.Default;
        private readonly struct Spectrum
        {
            public readonly Vector2 A,B,C;
            public readonly float Amplitude,W;
            public Spectrum(SeaStateSettings s)
            {
                Vector2 d=s.TravelDirection;float k=Mathf.PI*2/s.Wavelength;
                A=d*k;B=new Vector2(d.x*.883f-d.y*.469f,d.x*.469f+d.y*.883f)*k*1.47f;
                C=new Vector2(d.x*.423f+d.y*.906f,-d.x*.906f+d.y*.423f)*k*2.1f;
                Amplitude=s.Amplitude;W=Mathf.Sqrt(2.4f*k);
            }
        }
        private static Spectrum targetSpectrum=new(SeaStateSettings.Default);
        private static readonly Spectrum[] sourceMix=new Spectrum[8];
        private static readonly float[] sourceWeights=new float[8];
        private static int sourceCount;
        private static Vector2 sourceDirection=SeaStateSettings.Default.TravelDirection;
        private static float sourceLevel=2;
        private static float changedAt=-100;
        public static SeaStateSettings State=>target;
        public static Vector2 DirectionAt(float time)=>VisualDirection(time);
        public static float AmplitudeBound{get{float maximum=target.Amplitude;for(int i=0;i<sourceCount;i++)maximum=Mathf.Max(maximum,sourceMix[i].Amplitude);return maximum;}}
        public static float CurrentAmplitude(float time)
        {float blend=Blend(time);if(blend>=1)return target.Amplitude;float old=0;for(int i=0;i<sourceCount;i++)old+=sourceWeights[i]*sourceMix[i].Amplitude;return Mathf.Lerp(old,target.Amplitude,blend);}
        public static void Reset(SeaStateSettings settings,float time)
        {if(!settings.IsValid)return;target=settings;targetSpectrum=new Spectrum(settings);sourceCount=1;sourceMix[0]=targetSpectrum;sourceWeights[0]=1;sourceDirection=settings.TravelDirection;sourceLevel=settings.Level;changedAt=time-TransitionSeconds;ApplyShader(time);}
        public static void SetState(SeaStateSettings settings,float time)
        {
            if(!settings.IsValid||target.Equals(settings))return;
            float blend=Blend(time);sourceDirection=VisualDirection(time);sourceLevel=Mathf.Lerp(sourceLevel,target.Level,blend);
            int retained=0;
            for(int i=0;i<sourceCount;i++)
            {float weight=sourceWeights[i]*(1-blend);if(weight>.00001f){sourceMix[retained]=sourceMix[i];sourceWeights[retained++]=weight;}}
            sourceCount=retained;
            if(blend>.00001f)
            {
                int slot=sourceCount;
                if(slot==sourceMix.Length){slot=0;for(int i=1;i<sourceCount;i++)if(sourceWeights[i]<sourceWeights[slot])slot=i;}
                else sourceCount++;
                sourceMix[slot]=targetSpectrum;sourceWeights[slot]=blend;
            }
            float total=0;for(int i=0;i<sourceCount;i++)total+=sourceWeights[i];
            if(total>.00001f)for(int i=0;i<sourceCount;i++)sourceWeights[i]/=total;
            target=settings;targetSpectrum=new Spectrum(settings);changedAt=time;ApplyShader(time);
        }
        private static float Blend(float time)=>Mathf.SmoothStep(0,1,Mathf.Clamp01((time-changedAt)/TransitionSeconds));
        public static float Height(float x,float z,float time)
        {
            float blend=Blend(time),next=Sample(targetSpectrum,x,z,time);if(blend>=1)return next;
            float previous=0;for(int i=0;i<sourceCount;i++)previous+=sourceWeights[i]*Sample(sourceMix[i],x,z,time);
            return Mathf.Lerp(previous,next,blend);
        }
        public static Vector3 Normal(float x,float z,float time)
        {
            float blend=Blend(time);Vector2 slope=Slope(targetSpectrum,x,z,time);
            if(blend<1){Vector2 previous=Vector2.zero;for(int i=0;i<sourceCount;i++)previous+=sourceWeights[i]*Slope(sourceMix[i],x,z,time);slope=Vector2.Lerp(previous,slope,blend);}
            return new Vector3(-slope.x,1,-slope.y).normalized;
        }
        public static float SampleHeight(SeaStateSettings settings,float x,float z,float time)
        {
            return Sample(new Spectrum(settings),x,z,time);
        }
        private static float Sample(Spectrum s,float x,float z,float time)
        {
            float phase=s.A.x*x+s.A.y*z-time*s.W;
            return s.Amplitude*(.64f*Mathf.Sin(phase)+.12f*Mathf.Sin(phase*2-.25f)+.19f*Mathf.Sin(s.B.x*x+s.B.y*z-time*s.W*1.21f+.8f)+.05f*Mathf.Sin(s.C.x*x+s.C.y*z-time*s.W*1.45f+1.7f));
        }
        private static Vector2 Slope(Spectrum s,float x,float z,float time)
        {
            float phase=s.A.x*x+s.A.y*z-time*s.W;
            return s.Amplitude*(s.A*(.64f*Mathf.Cos(phase)+.24f*Mathf.Cos(phase*2-.25f))+s.B*(.19f*Mathf.Cos(s.B.x*x+s.B.y*z-time*s.W*1.21f+.8f))+s.C*(.05f*Mathf.Cos(s.C.x*x+s.C.y*z-time*s.W*1.45f+1.7f)));
        }
        private static Vector3 Velocity(Spectrum s,float x,float z,float time)
        {
            float phase=s.A.x*x+s.A.y*z-time*s.W,b=s.B.x*x+s.B.y*z-time*s.W*1.21f+.8f,c=s.C.x*x+s.C.y*z-time*s.W*1.45f+1.7f;
            Vector2 horizontal=s.A.normalized*(.64f*Mathf.Sin(phase)+.24f*Mathf.Sin(phase*2-.25f))+s.B.normalized*(.19f*1.21f*Mathf.Sin(b))+s.C.normalized*(.05f*1.45f*Mathf.Sin(c));
            float vertical=-(.64f*Mathf.Cos(phase)+.24f*Mathf.Cos(phase*2-.25f)+.19f*1.21f*Mathf.Cos(b)+.05f*1.45f*Mathf.Cos(c));
            return new Vector3(horizontal.x,vertical,horizontal.y)*(s.Amplitude*s.W);
        }
        public static Vector3 OrbitalVelocity(Vector3 point,float time)
        {
            float blend=Blend(time);Vector3 next=Velocity(targetSpectrum,point.x,point.z,time);if(blend>=1)return next;
            Vector3 previous=Vector3.zero;for(int i=0;i<sourceCount;i++)previous+=sourceWeights[i]*Velocity(sourceMix[i],point.x,point.z,time);
            return Vector3.Lerp(previous,next,blend);
        }
        public static void ApplyShader(float time)
        {
            float blend=Blend(time);Vector2 direction=VisualDirection(time);
            float level=Mathf.Lerp(sourceLevel,target.Level,blend);
            Shader.SetGlobalVector("_MeridianSea",new Vector4(direction.x,direction.y,Mathf.Lerp(.35f,1.5f,(level-1)/4f),Mathf.Lerp(0,.20f,(level-1)/4f)));
            Shader.SetGlobalFloat("_MeridianAmplitude",CurrentAmplitude(time));
        }
        private static Vector2 VisualDirection(float time)
        {
            Vector2 next=target.TravelDirection;float a=Mathf.Atan2(sourceDirection.y,sourceDirection.x)*Mathf.Rad2Deg,b=Mathf.Atan2(next.y,next.x)*Mathf.Rad2Deg;
            float angle=Mathf.LerpAngle(a,b,Blend(time))*Mathf.Deg2Rad;return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
        }
        public static float SurfaceY(Vector3 position,float time)=>SeaLevel+Height(position.x,position.z,time);
        public static float PropellerRate(float load,float speed)=>Mathf.Clamp(Mathf.Abs(load)*38f+Mathf.Clamp(speed,0,14)*1.8f,0,64);
        public static float BowRate(float forwardSpeed)=>Mathf.Clamp((forwardSpeed-.25f)*8f,0,42);
    }
}
