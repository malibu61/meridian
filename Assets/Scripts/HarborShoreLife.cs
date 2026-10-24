using System.Collections.Generic;
using UnityEngine;
namespace Meridian
{
    /// <summary>Pedestrian routes are on pier caps, separated from the bollard/line-working strip.</summary>
    public sealed class HarborShoreLife:MonoBehaviour
    {
        private sealed class Patrol{public HarborCrewMember Person;public Vector3 A,B;public float Offset,Speed;}
        private readonly List<Patrol> patrols=new();private EnvironmentResources owner;
        public int PatrolCount=>patrols.Count;
        public static Material[] Palette(EnvironmentResources resources,bool worker)
        {
            Color[] colors={worker?new Color(.07f,.19f,.30f):new Color(.93f,.94f,.90f),new(.028f,.038f,.045f),new(.62f,.40f,.28f),new(.81f,.64f,.25f),new(.95f,.32f,.06f)};
            Material[] result=new Material[5];for(int i=0;i<5;i++)result[i]=resources.Own(new Material(Shader.Find("Standard")){name="Harbour uniform palette "+i,color=colors[i],enableInstancing=true});return result;
        }
        public void Build()
        {
            owner=gameObject.AddComponent<EnvironmentResources>();Material[] white=Palette(owner,false);
            foreach(HarborBerth berth in new[]{HarborBerth.East,HarborBerth.West})for(int i=0;i<8;i++)
            {
                float x=berth==HarborBerth.East?36.2f+(i%2)*2.1f:-34.2f-(i%2)*2.1f;
                Vector3 a=new(x,MooringRules.PierY(berth),berth==HarborBerth.East?34:13),b=new(x,a.y,berth==HarborBerth.East?103:94);
                GameObject go=new(i%3==0?"White uniform naval officer patrol":"White uniform naval NCO patrol");go.transform.SetParent(transform,false);
                var person=go.AddComponent<HarborCrewMember>();person.Build(white,false,i%3==0,i*.72f,owner);
                patrols.Add(new Patrol{Person=person,A=a,B=b,Offset=i*10.7f,Speed=.19f+(i%3)*.025f});
            }
            Sample(Time.time);
        }
        public void Sample(float time)
        {
            foreach(var p in patrols)
            {
                float length=Vector3.Distance(p.A,p.B),duration=length/p.Speed,clock=Mathf.Repeat(time+p.Offset,2*(duration+4));
                bool returning=clock>duration+4;float local=returning?clock-duration-4:clock;
                float u=Mathf.Clamp01(local/duration);Vector3 start=returning?p.B:p.A,end=returning?p.A:p.B;
                p.Person.transform.position=Vector3.Lerp(start,end,u);bool walking=local<duration;
                Quaternion facing=Quaternion.LookRotation(end-start);p.Person.transform.rotation=Quaternion.Slerp(p.Person.transform.rotation,facing,.2f);
                p.Person.SamplePose(time,walking?HarborCrewPose.Walk:HarborCrewPose.Watch,0);
            }
        }
        private void Update()=>Sample(Time.time);
    }
}
