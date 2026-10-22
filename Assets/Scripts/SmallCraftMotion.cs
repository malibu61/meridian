using UnityEngine;
namespace Meridian
{
    public sealed class SmallCraftMotion:MonoBehaviour
    {
        private ShipController ship;
        private Transform[] propellers,rudders;
        private Transform radar;
        public void Configure(Transform[] screws,Transform[] steering)
        {ship=GetComponentInParent<ShipController>();propellers=screws;rudders=steering;radar=transform.Find("Rotating navigation scanner");}
        public void SetRadar(Transform scanner)=>radar=scanner;
        private void Update()
        {
            if(ship==null||Time.deltaTime<=0)return;
            if(radar!=null)radar.Rotate(0,Time.deltaTime*36,0,Space.Self);
            for(int i=0;i<propellers.Length;i++)if(propellers[i]!=null)propellers[i].Rotate(0,0,ship.GetEngineSignedLoad(i)*Time.deltaTime*1000*(i%2==0?1:-1),Space.Self);
            foreach(var rudder in rudders)if(rudder!=null)rudder.localRotation=Quaternion.Euler(0,-ship.ActualRudder*35,0);
        }
    }
}
