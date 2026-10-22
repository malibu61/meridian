using UnityEngine;
namespace Meridian
{
    public readonly struct SeaStateSettings:System.IEquatable<SeaStateSettings>
    {
        public readonly int Level;
        public readonly float FromDegrees;
        public bool IsValid=>Level>=1&&Level<=5&&!float.IsNaN(FromDegrees)&&!float.IsInfinity(FromDegrees);
        public static SeaStateSettings Default=>new(2,315);
        public SeaStateSettings(int level,float fromDegrees){Level=level;FromDegrees=float.IsNaN(fromDegrees)||float.IsInfinity(fromDegrees)?fromDegrees:Mathf.Repeat(fromDegrees,360);}
        public Vector2 TravelDirection=>new(-Mathf.Sin(FromDegrees*Mathf.Deg2Rad),-Mathf.Cos(FromDegrees*Mathf.Deg2Rad));
        public float Amplitude=>Level switch{1=>.08f,2=>.22f,3=>.48f,4=>.85f,_=>1.25f};
        public float Wavelength=>Level switch{1=>26,2=>30,3=>32,4=>34,_=>36};
        public string LevelName=>Level switch{1=>"Dingin",2=>"Hafif",3=>"Orta",4=>"Dalgalı",_=>"Sert"};
        public string DirectionName=>DirectionNames[Mathf.RoundToInt(FromDegrees/45f)%8];
        public static readonly string[] DirectionNames={"Kuzey","Kuzeydoğu","Doğu","Güneydoğu","Güney","Güneybatı","Batı","Kuzeybatı"};
        public static readonly string[] DirectionShort={"K","KD","D","GD","G","GB","B","KB"};
        public bool Equals(SeaStateSettings other)=>Level==other.Level&&FromDegrees.Equals(other.FromDegrees);
        public override bool Equals(object obj)=>obj is SeaStateSettings other&&Equals(other);
        public override int GetHashCode()=>Level.GetHashCode()^FromDegrees.GetHashCode();
    }
}
