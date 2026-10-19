using UnityEngine;

namespace Meridian
{
    /// <summary>Visual load response, not a physical emissions model of the real ship.</summary>
    public sealed class EngineExhaustResponse
    {
        private float previousCommand;
        public float Load { get; private set; }
        public float Surge { get; private set; }
        public float EmissionRate => .8f + Load * 14f + Surge * 65f;
        public float Opacity => .035f + Load * .19f + Surge * .32f;
        public float Darkness => .10f + Load * .14f + Surge * .68f;
        public float UpwardSpeed => .38f + Load * .32f + Surge * .85f;

        public void Reset(float command=0f,float actualLoad=0f)
        {
            previousCommand=Finite(command)?Mathf.Clamp(command,-1f,1f):0f;
            Load=Finite(actualLoad)?Mathf.Clamp01(Mathf.Abs(actualLoad)):0f;Surge=0f;
        }

        public void Step(float command,float actualLoad,float deltaTime)
        {
            if(!Finite(command)||!Finite(actualLoad)||!Finite(deltaTime)||deltaTime<=0f)return;
            float dt=Mathf.Min(deltaTime,.25f);
            command=Mathf.Clamp(command,-1f,1f);actualLoad=Mathf.Clamp01(Mathf.Abs(actualLoad));
            float increase=Mathf.Max(0f,Mathf.Abs(command)-Mathf.Abs(previousCommand));
            if(command*previousCommand<-.20f)increase=Mathf.Max(increase,Mathf.Abs(command)*.65f);
            float rate=increase/dt;
            Surge=Mathf.MoveTowards(Surge,0f,dt*.42f);
            if(rate>.8f)Surge=Mathf.Max(Surge,Mathf.Clamp01(increase*.9f+(rate-.8f)*.10f));
            Load=Mathf.Lerp(Load,actualLoad,1f-Mathf.Exp(-4f*dt));
            previousCommand=command;
        }

        private static bool Finite(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f);
    }
}
