using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    public enum WhistleSignal{None=0,Starboard=1,Port=2,Astern=3,Prolonged=4,Attention=5}
    /// <summary>International Rule 34(a) training context: visible vessels, not every motion everywhere.</summary>
    public sealed class ManeuverSignalMonitor
    {
        private sealed class Requirement{public WhistleSignal Signal;public float Deadline;}
        private readonly Queue<Requirement> pending=new();
        private bool asternEpisode,turnEpisode;private float asternClear,turnClear,headingAtTurn;
        private WhistleSignal recent;private float recentAt=-100;private int turnSign;
        private bool hasHeading;private float previousHeading;
        public int Complied{get;private set;}public int WrongSignals{get;private set;}
        public bool Violated{get;private set;}public WhistleSignal Expected=>pending.Count>0?pending.Peek().Signal:WhistleSignal.None;
        public int PendingCount=>pending.Count;
        public float SecondsLeft(float now)=>pending.Count==0?0:Mathf.Max(0,pending.Peek().Deadline-now);
        public static string Meaning(WhistleSignal signal)=>signal switch{WhistleSignal.Starboard=>"SANCAĞA ROTA DEĞİŞİKLİĞİ · 1 KISA",WhistleSignal.Port=>"İSKELEYE ROTA DEĞİŞİKLİĞİ · 2 KISA",WhistleSignal.Astern=>"TORNİSTAN SEVK · 3 KISA",WhistleSignal.Prolonged=>"UZUN DÜDÜK · 5 SANİYE",WhistleSignal.Attention=>"ŞÜPHE / UYARI · 5 KISA",_=>"Sinyal gerektiğinde doğru düdüğü seç"};
        public void Tick(float dt,float now,bool asternPropulsion,float rudder,float heading,float forwardKnots,bool visibleTraffic,float differential=0)
        {
            if(!float.IsFinite(dt)||dt<=0||!visibleTraffic||Violated)return;
            if(asternPropulsion){asternClear=0;if(!asternEpisode){asternEpisode=true;Require(WhistleSignal.Astern,now);}}
            else{asternClear+=dt;if(asternClear>=2)asternEpisode=false;}
            float yawRate=hasHeading?Mathf.DeltaAngle(previousHeading,heading)/dt:0;previousHeading=heading;hasHeading=true;
            bool turning=!asternPropulsion&&forwardKnots>.3f&&(Mathf.Abs(rudder)>.18f||Mathf.Abs(differential)>.12f);
            if(turning)
            {
                float direction=Mathf.Abs(yawRate)>.15f?yawRate:Mathf.Abs(rudder)>.18f?rudder:differential;int sign=direction>0?1:-1;turnClear=0;
                if(turnSign!=sign){turnSign=sign;headingAtTurn=heading;turnEpisode=false;}
                if(!turnEpisode&&Mathf.Abs(Mathf.DeltaAngle(headingAtTurn,heading))>=3){turnEpisode=true;Require(sign>0?WhistleSignal.Starboard:WhistleSignal.Port,now);}
            }
            else{turnClear+=dt;if(turnClear>=2){turnEpisode=false;turnSign=0;headingAtTurn=heading;}}
            if(pending.Count>0&&now>pending.Peek().Deadline)Violated=true;
        }
        private void Require(WhistleSignal signal,float now)
        {
            if(recent==signal&&now>=recentAt&&now-recentAt<=8){recent=WhistleSignal.None;Complied++;return;}
            foreach(var requirement in pending)if(requirement.Signal==signal)return;
            // Multiple different manoeuvres retain their independent obligations; no unbounded queue.
            if(pending.Count<4)pending.Enqueue(new Requirement{Signal=signal,Deadline=now+15+pending.Count*6});
        }
        public void SoundCompleted(WhistleSignal signal,float now)
        {
            if(Violated)return;
            if(pending.Count>0)
            {
                if(now>pending.Peek().Deadline){Violated=true;return;}
                if(pending.Peek().Signal==signal){pending.Dequeue();Complied++;return;}
                WrongSignals++;return;
            }
            recent=signal;recentAt=now;
        }
    }
}
