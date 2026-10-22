using System;
using UnityEngine;

namespace Meridian
{
    public enum VesselId { Barbaros, Anadolu, Ruzgar, Kumkale }

    /// <summary>Public dimensions plus explicitly game-tuned handling; not naval engineering coefficients.</summary>
    public sealed class VesselDefinition
    {
        public VesselId Id { get; }
        public string Name { get; }
        public string Pennant { get; }
        public float LengthMetres { get; }
        public float BeamMetres { get; }
        public bool UsesPods => Id == VesselId.Anadolu;
        public bool IsSmallCraft=>Id==VesselId.Ruzgar||Id==VesselId.Kumkale;
        public bool HasFourEngines=>Id==VesselId.Ruzgar;
        public int EngineCount=>HasFourEngines?4:2;
        private float Profile(float barbaros,float anadolu,float ruzgar,float kumkale)=>Id switch{VesselId.Anadolu=>anadolu,VesselId.Ruzgar=>ruzgar,VesselId.Kumkale=>kumkale,_=>barbaros};
        public float CameraScale=>Profile(1,1.85f,.52f,.56f);
        public float CameraFocusHeight=>Profile(2.45f,4.8f,.85f,1.05f);
        public Vector3 ColliderSize=>IsSmallCraft?new Vector3(BeamMetres/4,Id==VesselId.Ruzgar?1f:1.25f,LengthMetres/4):UsesPods?new Vector3(8,6.9f,57.7f):new Vector3(3.7f,1.85f,29.3f);
        public Vector3 ColliderCenter=>IsSmallCraft?new Vector3(0,Id==VesselId.Ruzgar?-.45f:-.35f,0):UsesPods?new Vector3(0,.95f,0):new Vector3(0,.12f,.4f);
        public Vector3 HarborSpawn=>new(0,.1f,Profile(-38,-52,-30,-30));
        public float SternWakeZ=>Profile(-13.2f,-27.4f,-7.05f,-6.95f);
        public float BowWakeZ=>Profile(14.4f,27.5f,7.1f,6.98f);
        public float WakeSpacing=>Profile(1.05f,2.05f,.64f,.67f);
        public float EngineSpacing=>Profile(3.2f,5.8f,1.48f,1.45f);
        public float EngineOffset=>Profile(12.2f,25.5f,6.2f,6.1f);
        public float GameMass=>Profile(40000,200000,6500,8500);
        // Public maximum-speed targets; turning/acceleration/roll values are game tuning, not trials data.
        public float MaximumAheadKnots=>Profile(32,21,41,25);
        public float MaximumAsternKnots=>MaximumAheadKnots*Profile(.38f,.30f,.34f,.40f);
        public float MaximumWorldSpeed=>MaximumAheadKnots/(1.9438445f*VesselCatalog.ModelMetresPerUnit);
        public float AccelerationSeconds=>Profile(24,38,14,18);
        public float GameThrust=>GameMass*MaximumWorldSpeed/(AccelerationSeconds*EngineCount);
        public float ReverseThrustFactor{get{float q=MaximumAsternKnots/MaximumAheadKnots;return .15f*q+.85f*q*q;}}
        public float TurnDiameterLengths=>Profile(3.8f,4.2f,3.4f,3.6f);
        public float YawResponseSeconds=>Profile(7.5f,12,4.5f,5.5f);
        public float RollPeriod=>Profile(11,16,7.7f,8.1f);
        public float RollDamping=>Profile(.25f,.38f,.17f,.20f);
        public float WaveRollResponse=>Profile(.68f,.34f,1.45f,1.50f);
        public float MaximumGameRoll=>Profile(30,20,48,44);
        public float MaximumTurnHeel=>Profile(10,6,17,13);
        public float EngineResponse=>Profile(.45f,.22f,.7f,.55f);
        public float LongitudinalResistance=>Profile(1100,5800,380,400);
        public float LateralResistance=>Profile(5200,32000,1700,2100);
        public float AngularResistance=>Profile(5200,26000,2500,3000);
        public float GameRudderTorque=>Profile(950,950,160,260);
        public float EffectScale=>Profile(1,1.5f,.50f,.55f);
        public float EngineLocalX(int index)
        {
            if(index<0||index>=EngineCount)throw new ArgumentOutOfRangeException(nameof(index));
            return (index%2==0?-1:1)*EngineSpacing*(index<2?.5f:.17f);
        }
        public string ClassLabel=>Id switch{VesselId.Anadolu=>"AMFİBİ HÜCUM GEMİSİ",VesselId.Ruzgar=>"RÜZGAR SINIFI HÜCUMBOT",VesselId.Kumkale=>"TUZLA SINIFI KARAKOL GEMİSİ",_=>"BARBAROS SINIFI FIRKATEYN"};
        public string EngineCaption=>HasFourEngines?"4 BAĞIMSIZ MAKİNE + DÜMEN":UsesPods?"ÇİFT POD":"ÇİFT MAKİNE + DÜMEN";
        public string Description=>Id switch
        {
            VesselId.Anadolu=>"Uzun uçuş güvertesi, çift pod ve daha ağır makine tepkisi. Pod kumandası bu sürümde sınırlıdır.",
            VesselId.Ruzgar=>"Alçak hücumbot gövdesi. Dört bağımsız makine, kompakt kumandalar. Oyun azami sürat hedefi: 41 kn.",
            VesselId.Kumkale=>"Tuzla sınıfı kapalı üst yapı, çift baş topu ve kıç çalışma güvertesi. Çift makine. Sürat hedefi: 25 kn.",
            _=>"Bağımsız çift makine ve dümen. Hassas liman manevraları için mevcut fırkateyn platformu."
        };

        internal VesselDefinition(VesselId id, string name, string pennant, float length, float beam)
        { Id = id; Name = name; Pennant = pennant; LengthMetres = length; BeamMetres = beam; }
    }

    public static class VesselCatalog
    {
        public const float ModelMetresPerUnit = 4f;
        private static readonly VesselDefinition Barbaros = new(VesselId.Barbaros, "TCG BARBAROS", "F-244", 118f, 14.8f);
        private static readonly VesselDefinition Anadolu = new(VesselId.Anadolu, "TCG ANADOLU", "L-400", 230.8f, 32f);
        private static readonly VesselDefinition Ruzgar=new(VesselId.Ruzgar,"TCG RÜZGAR","P-344",57.84f,7.62f);
        private static readonly VesselDefinition Kumkale=new(VesselId.Kumkale,"TCG KUMKALE","P-1203",56.9f,8.9f);
        public static readonly System.Collections.Generic.IReadOnlyList<VesselId> All=Array.AsReadOnly(new[]{VesselId.Barbaros,VesselId.Anadolu,VesselId.Ruzgar,VesselId.Kumkale});
        public static bool IsAvailable(VesselId id) => id==VesselId.Barbaros||id==VesselId.Anadolu||id==VesselId.Ruzgar||id==VesselId.Kumkale;
        public static VesselDefinition Get(VesselId id) => id switch
        {
            VesselId.Barbaros => Barbaros,
            VesselId.Anadolu => Anadolu,
            VesselId.Ruzgar=>Ruzgar,
            VesselId.Kumkale=>Kumkale,
            _ => throw new ArgumentOutOfRangeException(nameof(id))
        };
    }
}
