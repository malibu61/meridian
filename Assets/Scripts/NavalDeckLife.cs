using System.Collections.Generic;
using UnityEngine;

namespace Meridian
{
    /// <summary>Decorative shipboard line-handling drill, not a shore-connected mooring simulation.
    /// All coordinates are ship-local; one unit represents approximately four metres.</summary>
    public sealed class NavalDeckLife : MonoBehaviour
    {
        private enum Duty { LineHandler, Lookout, BridgePatrol }
        private enum Station { Bow, Stern, PortBridge, StarboardBridge }
        private sealed class Sailor
        {
            public Transform root, torso, hips, belt, neck, head, helmet, brim, vestLeft, vestRight, vestBack, collar;
            public Transform binoculars;
            public readonly Transform[] legs = new Transform[4];
            public readonly Transform[] arms = new Transform[4];
            public readonly Transform[] hands = new Transform[2];
            public readonly Transform[] tapes = new Transform[2];
            public readonly Transform[] boots = new Transform[2];
            public readonly Vector3[] poseFeet = new Vector3[2];
            public readonly Vector3[] poseHands = new Vector3[2];
            public Duty duty;
            public Station station;
            public Vector3 anchor;
            public Vector3[] route;
            public float routeLength, slotOffset, cycleLength, baseYaw;
            public Vector3 liveOrigin;
            public Quaternion liveHead;
            public float phase;
        }

        private readonly List<Sailor> sailors = new();
        private readonly List<Material> ownedMaterials = new();
        private readonly List<Mesh> aircraftMeshes = new();
        private Material navy, vest, black, skin, glove, reflective, ropePaint, helmetBlue, helmetBrown;
        private Mesh roundedMesh, rodMesh;
        private bool built;
        public int CrewCount => sailors.Count;
        public int BowCrewCount => CountAt(Station.Bow);
        public int SternCrewCount => CountAt(Station.Stern);
        public int PortBridgeCrewCount => CountAt(Station.PortBridge);
        public int StarboardBridgeCrewCount => CountAt(Station.StarboardBridge);
        public int DeckLookoutCount => sailors.FindAll(s => s.duty == Duty.Lookout && (s.station == Station.Bow || s.station == Station.Stern)).Count;
        public Vector3 FirstPatrolPosition => sailors.Find(s => s.duty == Duty.BridgePatrol).root.localPosition;
        public Quaternion FirstLookoutHead => sailors.Find(s => s.duty == Duty.Lookout).head.localRotation;
        public Vector3 FirstHandPosition => sailors.Count == 0 ? transform.position : sailors[0].hands[0].position;
        public float MaximumGripError { get; private set; }
        public int AnimatedFrames { get; private set; }
        public float ObservedHandTravel { get; private set; }
        public float ObservedPatrolTravel { get; private set; }
        public float ObservedLookoutTurn { get; private set; }
        private Vector3 firstLiveHand;
        internal void OwnAircraftMesh(Mesh mesh) => aircraftMeshes.Add(mesh);
        internal void OwnAircraftMaterial(Material material) => ownedMaterials.Add(material);

        public void Build()
        {
            if (built) return;
            built = true;
            navy = Paint("TDK navy blue working dress", new Color(.025f, .045f, .085f));
            vest = Paint("Orange line-handler lifejacket", new Color(.96f, .22f, .035f));
            black = Paint("Boots and lifejacket webbing", new Color(.022f, .027f, .031f));
            skin = Paint("Crew skin", new Color(.62f, .40f, .28f));
            glove = Paint("Heavy work gloves", new Color(.56f, .50f, .35f));
            reflective = Paint("Lifejacket reflective patches", new Color(.82f, .83f, .69f));
            ropePaint = Paint("Manila coloured training line", new Color(.56f, .43f, .24f));
            helmetBlue = Paint("Blue safety helmet", new Color(.035f, .41f, .67f));
            helmetBrown = Paint("Brown safety helmet", new Color(.39f, .15f, .08f));
            roundedMesh = RoundedMesh(); rodMesh = CylinderMesh();

            Transform aircraft = new GameObject("S-70B Seahawk - parked and secured").transform;
            aircraft.SetParent(transform, false);
            aircraft.localPosition = new Vector3(-.10f, 1.055f, -11.0f);
            new SeahawkModelBuilder(aircraft).Build();
            BuildTeam("Bow port line handlers", -1.01f, 9.10f, 1f, Station.Bow);
            BuildTeam("Bow starboard line handlers", 1.01f, 9.10f, 1f, Station.Bow);
            BuildTeam("Stern starboard line handlers", 1.10f, -12.15f, -1f, Station.Stern);
            BuildTeam("Stern port line handlers", -1.10f, -12.15f, -1f, Station.Stern);
            BuildLookout(new Vector3(1.37f,DeckY(9.08f),9.08f),55f,Station.Bow,0f,true);
            BuildLookout(new Vector3(1.25f,DeckY(10.28f),10.28f),110f,Station.Bow,2.1f,true);
            BuildLookout(new Vector3(1.43f,DeckY(-12.38f),-12.38f),105f,Station.Stern,1.2f,true);
            BuildLookout(new Vector3(1.35f,DeckY(-13.46f),-13.46f),165f,Station.Stern,3.4f,true);
            BuildBridgeWatch();
            SampleMotion(0f);
        }

        private int CountAt(Station station) => sailors.FindAll(s => s.station == station).Count;

        private static float DeckY(float z) => 1.035f + Mathf.SmoothStep(0f, .43f, Mathf.InverseLerp(7f, 15.1f, z));

        private void BuildTeam(string name, float x, float firstZ, float facing, Station station)
        {
            Transform team = new GameObject(name).transform; team.SetParent(transform, false);
            // Feet stay clear of both the helicopter envelope and the bow gun/VLS.
            for (int i = 0; i < 3; i++)
            {
                float z = firstZ + facing * i * .62f;
                Sailor person = BuildSailor(team, i == 0 ? helmetBlue : helmetBrown);
                person.root.localPosition = new Vector3(x, DeckY(z), z);
                person.root.localRotation = Quaternion.Euler(0f, facing > 0 ? 0f : 180f, 0f);
                person.phase = i * .27f + (facing < 0 ? 1.1f : 0f);
                person.station = station;
                sailors.Add(person);
            }
            float ropeX = x - facing * .070f;
            float startZ = firstZ - facing * .48f, endZ = firstZ + facing * 1.95f;
            Vector3[] points = new Vector3[33];
            for (int i = 0; i < points.Length; i++)
            {
                float z = Mathf.Lerp(startZ, endZ, i / 32f);
                points[i] = new Vector3(ropeX, DeckY(z) + .255f, z);
            }
            Line("Taut shared working line", team, points, .012f, ropePaint);
            Vector3 bollard = new(ropeX, DeckY(endZ), endZ);
            Segment("Bollard upright", team, bollard, bollard + Vector3.up * .23f, .065f, black);
            Segment("Bollard crosspiece", team, bollard + new Vector3(-.12f,.20f,0f), bollard + new Vector3(.12f,.20f,0f), .035f, black);
            Line("Lead down to bollard", team, new[] { points[32], bollard + new Vector3(.055f,.17f,0f), bollard + new Vector3(-.06f,.12f,.03f) }, .012f, ropePaint);
            // Flaked rope lies outside the handlers' feet, never around their ankles.
            Vector3 coilCenter = new(x + facing * .25f, DeckY(firstZ) + .012f, firstZ - facing * .45f);
            Vector3[] coil = new Vector3[121];
            for (int i = 0; i < coil.Length; i++)
            {
                float a = i / 120f * Mathf.PI * 8f, r = Mathf.Lerp(.075f,.17f,i / 120f);
                coil[i] = coilCenter + new Vector3(Mathf.Cos(a) * r, .004f * i / 120f, Mathf.Sin(a) * r * 1.5f);
            }
            Line("Neatly flaked spare line", team, coil, .010f, ropePaint);
            Vector3[] lead = new Vector3[21];
            Vector3 p0=points[0],p1=p0+new Vector3(0f,-.16f,-facing*.16f),p2=coil[0]+new Vector3(0f,0f,-facing*.10f);
            for(int i=0;i<lead.Length;i++)
            { float u=i/20f,v=1f-u; lead[i]=v*v*v*p0+3f*v*v*u*p1+3f*v*u*u*p2+u*u*u*coil[0]; }
            Line("Working end leads to spare coil", team, lead,.012f,ropePaint);
        }

        private void BuildLookout(Vector3 position,float yaw,Station station,float phase,bool safetyGear)
        {
            Sailor s=BuildSailor(transform,safetyGear?helmetBlue:navy,safetyGear);
            s.duty=Duty.Lookout; s.station=station; s.anchor=position; s.baseYaw=yaw; s.phase=phase;
            s.root.name=station+" - scanning lookout"; s.root.localPosition=position;
            s.root.localRotation=Quaternion.Euler(0f,yaw,0f);
            s.binoculars=new GameObject("Lookout binoculars").transform; s.binoculars.SetParent(s.root,false);
            for(int side=-1;side<=1;side+=2)
            {
                Transform tube=Shape("Binocular barrel",s.binoculars,new Vector3(.025f,.024f,.055f),black);
                tube.localPosition=new Vector3(side*.017f,0f,0f);
                Transform lens=Shape("Binocular objective",s.binoculars,new Vector3(.021f,.020f,.004f),reflective);
                lens.localPosition=new Vector3(side*.017f,0f,.027f);
            }
            sailors.Add(s);
        }

        private void BuildBridgeWatch()
        {
            // Assigned complement: eight port, two starboard. Some are inside while the others stand watch outside.
            for(int i=0;i<4;i++)
                BuildLookout(new Vector3(-1.81f,2.465f,2.77f+i*.255f),-90f,Station.PortBridge,i*1.7f,false);
            BuildLookout(new Vector3(1.81f,2.465f,3.54f),90f,Station.StarboardBridge,.8f,false);
            for(int side=-1;side<=1;side+=2)
            {
                int walkers=side<0?4:1;
                for(int i=0;i<walkers;i++)
                {
                    Sailor s=BuildSailor(transform,navy,false);
                    s.duty=Duty.BridgePatrol; s.station=side<0?Station.PortBridge:Station.StarboardBridge;
                    s.root.name=s.station+" - walking through bridge door";
                    s.anchor=new Vector3(side*.88f,2.465f,3.02f+i*.14f);
                    s.route=new[]{s.anchor,new Vector3(side*1.25f,2.465f,3.25f),new Vector3(side*1.65f,2.465f,3.25f),new Vector3(side*1.65f,2.465f,2.82f)};
                    for(int j=1;j<s.route.Length;j++)s.routeLength+=Vector3.Distance(s.route[j-1],s.route[j]);
                    s.slotOffset=i*10f; s.cycleLength=walkers*10f; s.phase=i*.63f+side;
                    sailors.Add(s);
                }
            }
        }

        private Sailor BuildSailor(Transform parent, Material helmetPaint, bool safetyGear=true)
        {
            Sailor s = new();
            s.root = new GameObject("Animated Turkish naval line handler").transform;
            s.root.SetParent(parent, false);
            s.torso = Shape("Working jacket", s.root, new Vector3(.098f,.143f,.067f), navy);
            s.hips = Shape("Trouser seat", s.root, new Vector3(.088f,.069f,.069f), navy);
            s.belt = Shape("Black working belt", s.root, new Vector3(.098f,.018f,.074f), black);
            s.head = Shape("Head", s.root, new Vector3(.055f,.065f,.054f), skin);
            s.neck = Shape("Neck", s.root, new Vector3(.027f,.045f,.029f), skin);
            Transform nose=Shape("Nose gives the scanning head a visible facing",s.head,new Vector3(.20f,.22f,.28f),skin);
            nose.localPosition=new Vector3(0f,-.015f,.48f);
            for(int side=-1;side<=1;side+=2)
            {
                Transform eye=Shape("Eye",s.head,new Vector3(.075f,.055f,.07f),black);
                eye.localPosition=new Vector3(side*.22f,.12f,.443f);
            }
            s.helmet = Shape("Safety helmet shell", s.root, new Vector3(.070f,.048f,.071f), helmetPaint);
            s.brim = Shape("Helmet brim", s.root, new Vector3(.080f,.009f,.078f), helmetPaint);
            s.collar = Shape("Lifejacket collar", s.root, new Vector3(.116f,.040f,.080f), vest);
            s.vestLeft = Shape("Left lifejacket panel", s.root, new Vector3(.048f,.127f,.040f), vest);
            s.vestRight = Shape("Right lifejacket panel", s.root, new Vector3(.048f,.127f,.040f), vest);
            s.vestBack = Shape("Lifejacket back", s.root, new Vector3(.105f,.116f,.025f), vest);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Transform boot = Shape("Black safety boot", s.root, new Vector3(.046f,.037f,.083f), black);
                s.boots[i]=boot;
                boot.localPosition = new Vector3(side * .042f,.022f, i == 0 ? .055f : -.055f);
                s.legs[i * 2] = Shape("Trouser thigh", s.root, Vector3.one, navy);
                s.legs[i * 2 + 1] = Shape("Trouser shin", s.root, Vector3.one, navy);
                s.arms[i * 2] = Shape("Jacket upper sleeve", s.root, Vector3.one, navy);
                s.arms[i * 2 + 1] = Shape("Jacket forearm", s.root, Vector3.one, navy);
                s.hands[i] = Shape(safetyGear?"Gloved hand":"Hand", s.root, new Vector3(.033f,.027f,.040f), safetyGear?glove:skin);
                s.tapes[i] = Shape("Reflective chest patch", s.root, new Vector3(.026f,.014f,.006f), reflective);
            }
            if(!safetyGear)
            {
                s.collar.gameObject.SetActive(false);s.vestLeft.gameObject.SetActive(false);s.vestRight.gameObject.SetActive(false);s.vestBack.gameObject.SetActive(false);
                foreach(Transform tape in s.tapes)tape.gameObject.SetActive(false);
                s.helmet.localScale=new Vector3(.065f,.030f,.061f);
                s.brim.localScale=new Vector3(.068f,.008f,.079f);
            }
            return s;
        }

        private void Update()
        {
            SampleMotion(Time.time);
            if (sailors.Count == 0) return;
            Vector3 hand = sailors[0].hands[0].localPosition;
            if (AnimatedFrames == 0) firstLiveHand = hand;
            ObservedHandTravel = Mathf.Max(ObservedHandTravel,Vector3.Distance(firstLiveHand,hand));
            foreach(Sailor s in sailors)
            {
                if(AnimatedFrames==0){s.liveOrigin=s.root.localPosition;s.liveHead=s.head.localRotation;}
                if(s.duty==Duty.BridgePatrol)ObservedPatrolTravel=Mathf.Max(ObservedPatrolTravel,Vector3.Distance(s.liveOrigin,s.root.localPosition));
                if(s.duty==Duty.Lookout)ObservedLookoutTurn=Mathf.Max(ObservedLookoutTurn,Quaternion.Angle(s.liveHead,s.head.localRotation));
            }
            AnimatedFrames++;
        }

        /// <summary>Deterministic pose evaluation also used by the editor's visual regression check.</summary>
        public void SampleMotion(float seconds)
        {
            MaximumGripError = 0f;
            foreach (Sailor s in sailors)
            {
                if(s.duty!=Duty.LineHandler){AnimateWatch(s,seconds);continue;}
                // Reach, haul, release/reset. Small phase offsets keep the team coordinated, not identical.
                float phase = Mathf.Repeat(seconds / 3.6f + s.phase * .12f, 1f);
                float reach = phase < .65f ? Mathf.SmoothStep(.070f, -.040f, phase / .65f)
                    : Mathf.SmoothStep(-.040f, .070f, (phase - .65f) / .35f);
                float bend = .012f * Mathf.Sin(phase * Mathf.PI * 2f);
                Vector3 hip = new(0f,.215f + bend, -.025f - reach * .12f);
                Vector3 shoulder = hip + new Vector3(0f,.125f,reach * .33f);
                Vector3 middle = (hip + shoulder) * .5f;
                Quaternion lean = Quaternion.FromToRotation(Vector3.up, shoulder - hip);
                s.torso.localPosition = middle; s.torso.localRotation = lean;
                s.hips.localPosition = hip - Vector3.up * .014f;
                s.belt.localPosition = hip + Vector3.up * .019f;
                s.head.localPosition = shoulder + new Vector3(0f,.061f,.012f);
                s.neck.localPosition = shoulder + new Vector3(0f,.025f,.005f);
                s.head.localRotation = Quaternion.Euler(12f, -18f, 0f);
                s.helmet.localPosition = s.head.localPosition + new Vector3(0f,.027f,-.004f);
                s.brim.localPosition = s.helmet.localPosition + new Vector3(0f,-.012f,.008f);
                s.collar.localPosition = shoulder + new Vector3(0f,-.002f,0f); s.collar.localRotation = lean;
                s.vestLeft.localPosition = middle + lean * new Vector3(-.026f,0f,.038f);
                s.vestRight.localPosition = middle + lean * new Vector3(.026f,0f,.038f);
                s.vestBack.localPosition = middle + lean * new Vector3(0f,0f,-.034f);
                s.vestLeft.localRotation = s.vestRight.localRotation = s.vestBack.localRotation = lean;
                for (int i = 0; i < 2; i++)
                {
                    float side = i == 0 ? -1f : 1f;
                    Vector3 foot = new(side * .042f,.046f,i == 0 ? .055f : -.055f);
                    Vector3 knee = new(side * .047f,.118f,foot.z + .025f + reach * .15f);
                    Bone(s.legs[i * 2], hip + Vector3.right * side * .027f, knee, .046f);
                    Bone(s.legs[i * 2 + 1], knee, foot, .037f);
                    Vector3 hand = new(-.070f,.255f,-.015f + i * .065f + reach);
                    // The forward line follows the camber of the forecastle.
                    float handShipZ = s.root.localPosition.z + (s.root.localRotation * hand).z;
                    hand.y += DeckY(handShipZ) - s.root.localPosition.y;
                    Vector3 armStart = shoulder + new Vector3(side * .060f,-.012f,0f);
                    Vector3 elbow = Vector3.Lerp(armStart, hand, .5f) + new Vector3(side * .014f,-.041f,-.018f);
                    Bone(s.arms[i * 2], armStart, elbow, .033f);
                    Bone(s.arms[i * 2 + 1], elbow, hand, .029f);
                    s.hands[i].localPosition = hand;
                    s.tapes[i].localPosition = middle + lean * new Vector3(side * .026f,.035f,.061f);
                    s.tapes[i].localRotation = lean;
                    Vector3 shipHand = s.root.localPosition + s.root.localRotation * hand;
                    float lineX = s.root.localPosition.x + (s.root.localRotation * new Vector3(-.070f,0f,0f)).x;
                    float error = Vector2.Distance(new Vector2(shipHand.x, shipHand.y), new Vector2(lineX, DeckY(shipHand.z) + .255f));
                    MaximumGripError = Mathf.Max(MaximumGripError, error);
                }
            }
        }

        private void AnimateWatch(Sailor s,float seconds)
        {
            bool walking=false;
            if(s.duty==Duty.BridgePatrol)
            {
                PatrolPose(s,seconds,out Vector3 p,out Vector3 forward,out walking);
                s.root.localPosition=p;
                s.root.localRotation=Quaternion.LookRotation(forward,Vector3.up);
            }
            else
            {
                s.root.localPosition=s.anchor;
                s.root.localRotation=Quaternion.Euler(0f,s.baseYaw+Mathf.Sin(seconds*.21f+s.phase)*8f,0f);
            }
            float gait=walking?Mathf.Sin(seconds*8f+s.phase):0f;
            Vector3 hip=new(0f,.218f+Mathf.Abs(gait)*.004f,0f);
            Vector3 shoulder=hip+new Vector3(0f,.124f,walking?.012f:0f);
            Vector3[] feet=s.poseFeet,hands=s.poseHands;
            feet[0]=new Vector3(-.042f,.022f+Mathf.Max(0f,gait)*.021f,gait*.052f);
            feet[1]=new Vector3(.042f,.022f+Mathf.Max(0f,-gait)*.021f,-gait*.052f);
            hands[0]=new Vector3(-.073f,.207f,-gait*.05f);hands[1]=new Vector3(.073f,.207f,gait*.05f);
            float headYaw=Mathf.Sin(seconds*.38f+s.phase)*54f;
            float headPitch=4f+Mathf.Sin(seconds*.23f+s.phase)*6f;
            Quaternion headRotation=Quaternion.Euler(headPitch,headYaw,0f);
            if(s.duty==Duty.Lookout)
            {
                float raised=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(-.3f,.4f,Mathf.Sin(seconds*.24f+s.phase)));
                Vector3 binocularPosition=shoulder+new Vector3(0f,.055f,0f)+headRotation*new Vector3(0f,-.009f,.065f);
                s.binoculars.localPosition=binocularPosition;s.binoculars.localRotation=headRotation;
                s.binoculars.gameObject.SetActive(raised>.82f);
                for(int i=0;i<2;i++)hands[i]=Vector3.Lerp(hands[i],binocularPosition+headRotation*new Vector3(i==0?-.030f:.030f,-.013f,0f),raised);
            }
            ApplyWatchPose(s,hip,shoulder,feet,hands,headRotation);
        }

        private static void PatrolPose(Sailor s,float seconds,out Vector3 position,out Vector3 forward,out bool walking)
        {
            const float speed=.36f, pause=1.2f;
            float t=Mathf.Repeat(seconds-s.slotOffset,s.cycleLength),travelTime=s.routeLength/speed;
            bool returning=t>travelTime+pause;
            walking=t<travelTime || (returning && t<travelTime*2f+pause);
            float distance=t<travelTime?t*speed:returning?Mathf.Max(0f,s.routeLength-(t-travelTime-pause)*speed):s.routeLength;
            position=RoutePoint(s,distance);
            forward=RoutePoint(s,Mathf.Min(s.routeLength,distance+.025f))-RoutePoint(s,Mathf.Max(0f,distance-.025f));
            if(returning)forward=-forward;
            if(!walking)forward=new Vector3(s.station==Station.PortBridge?-1f:1f,0f,0f);
            if(forward.sqrMagnitude<.00001f)forward=Vector3.forward;
        }

        private static Vector3 RoutePoint(Sailor s,float distance)
        {
            for(int i=1;i<s.route.Length;i++)
            {
                float length=Vector3.Distance(s.route[i-1],s.route[i]);
                if(distance<=length)return Vector3.Lerp(s.route[i-1],s.route[i],distance/length);
                distance-=length;
            }
            return s.route[s.route.Length-1];
        }

        private static void ApplyWatchPose(Sailor s,Vector3 hip,Vector3 shoulder,Vector3[] feet,Vector3[] hands,Quaternion headRotation)
        {
            Vector3 middle=(hip+shoulder)*.5f;
            Quaternion lean=Quaternion.FromToRotation(Vector3.up,shoulder-hip);
            s.torso.localPosition=middle;s.torso.localRotation=lean;
            s.hips.localPosition=hip-Vector3.up*.014f;s.belt.localPosition=hip+Vector3.up*.019f;
            s.head.localPosition=shoulder+new Vector3(0f,.061f,.006f);s.head.localRotation=headRotation;
            s.neck.localPosition=shoulder+new Vector3(0f,.025f,.003f);s.neck.localRotation=headRotation;
            s.helmet.localPosition=s.head.localPosition+headRotation*new Vector3(0f,.027f,-.004f);s.helmet.localRotation=headRotation;
            s.brim.localPosition=s.head.localPosition+headRotation*new Vector3(0f,.015f,.008f);s.brim.localRotation=headRotation;
            s.collar.localPosition=shoulder;s.collar.localRotation=lean;
            s.vestLeft.localPosition=middle+lean*new Vector3(-.026f,0f,.038f);
            s.vestRight.localPosition=middle+lean*new Vector3(.026f,0f,.038f);
            s.vestBack.localPosition=middle+lean*new Vector3(0f,0f,-.034f);
            s.vestLeft.localRotation=s.vestRight.localRotation=s.vestBack.localRotation=lean;
            for(int i=0;i<2;i++)
            {
                float side=i==0?-1f:1f;
                s.boots[i].localPosition=feet[i];
                Vector3 ankle=feet[i]+Vector3.up*.024f;
                Vector3 knee=new(side*.044f,.121f,feet[i].z*.55f+.018f);
                Bone(s.legs[i*2],hip+Vector3.right*side*.028f,knee,.046f);
                Bone(s.legs[i*2+1],knee,ankle,.037f);
                Vector3 armStart=shoulder+new Vector3(side*.059f,-.013f,0f);
                Vector3 elbow=(armStart+hands[i])*.5f+new Vector3(side*.016f,-.032f,.016f);
                Bone(s.arms[i*2],armStart,elbow,.033f);Bone(s.arms[i*2+1],elbow,hands[i],.029f);
                s.hands[i].localPosition=hands[i];
                s.tapes[i].localPosition=middle+lean*new Vector3(side*.026f,.035f,.061f);s.tapes[i].localRotation=lean;
            }
        }

        public bool ValidateBridgeRoutes(out string result)
        {
            int routes=0;
            foreach(Sailor s in sailors)
            {
                if(s.duty!=Duty.BridgePatrol)continue;
                bool inside=false,outside=false;
                for(float time=0f;time<=s.cycleLength;time+=.125f)
                {
                    PatrolPose(s,time,out Vector3 p,out _,out _);
                    float x=Mathf.Abs(p.x);inside|=x<1.35f;outside|=x>1.60f;
                    if((x>1.40f && x<1.55f && (p.z<3.15f || p.z>3.35f)) || x>1.84f || p.z<2.70f || p.z>3.65f || Mathf.Abs(p.y-2.465f)>.001f)
                    {result=$"Invalid bridge route at {p}";return false;}
                }
                if(!inside || !outside){result="A bridge patrol never crosses its doorway.";return false;}
                routes++;
            }
            result=$"routes={routes}, bow={BowCrewCount}, stern={SternCrewCount}, port={PortBridgeCrewCount}, starboard={StarboardBridgeCrewCount}, deckLookouts={DeckLookoutCount}";
            return routes==5 && BowCrewCount==8 && SternCrewCount==8 && PortBridgeCrewCount==8 && StarboardBridgeCrewCount==2 && DeckLookoutCount==4;
        }

        private static void Bone(Transform part, Vector3 a, Vector3 b, float diameter)
        {
            part.localPosition = (a + b) * .5f;
            part.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
            part.localScale = new Vector3(diameter, Vector3.Distance(a,b) + diameter * .35f, diameter);
        }

        private Material Paint(string name, Color color)
        {
            Material m = new(Shader.Find("Standard")) { name = name, color = color, enableInstancing = true };
            m.SetFloat("_Glossiness", .16f); ownedMaterials.Add(m); return m;
        }

        private Transform Shape(string name, Transform parent, Vector3 scale, Material paint)
        {
            GameObject go = new(name); go.transform.SetParent(parent, false); go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = roundedMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = paint; return go.transform;
        }

        private void Segment(string name, Transform parent, Vector3 a, Vector3 b, float radius, Material paint)
        {
            GameObject go = new(name); go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = rodMesh; go.AddComponent<MeshRenderer>().sharedMaterial = paint;
            go.transform.localPosition = (a+b)*.5f; go.transform.localRotation = Quaternion.FromToRotation(Vector3.up,b-a);
            go.transform.localScale = new Vector3(radius * 2f,Vector3.Distance(a,b),radius * 2f);
        }

        private static void Line(string name, Transform parent, Vector3[] points, float width, Material paint)
        {
            GameObject go = new(name); go.transform.SetParent(parent,false);
            LineRenderer line = go.AddComponent<LineRenderer>(); line.useWorldSpace = false;
            line.sharedMaterial = paint; line.widthMultiplier = width; line.numCapVertices = 3;
            line.positionCount = points.Length; line.SetPositions(points); line.alignment = LineAlignment.View;
        }

        private static Mesh RoundedMesh()
        {
            const int rings = 8, sides = 12;
            List<Vector3> v = new(); List<int> t = new();
            for (int y = 0; y <= rings; y++)
            for (int x = 0; x <= sides; x++)
            {
                float lat = Mathf.PI * y / rings, lon = 2f * Mathf.PI * x / sides;
                v.Add(new Vector3(Mathf.Sin(lat) * Mathf.Cos(lon), Mathf.Cos(lat), Mathf.Sin(lat) * Mathf.Sin(lon)) * .5f);
            }
            for (int y = 0; y < rings; y++) for (int x = 0; x < sides; x++)
            { int a = y*(sides+1)+x,b=a+sides+1; t.AddRange(new[]{a,a+1,b,b,a+1,b+1}); }
            Mesh mesh = new() { name = "Shared low-poly rounded crew mesh" }; mesh.SetVertices(v); mesh.SetTriangles(t,0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        private static Mesh CylinderMesh()
        {
            List<Vector3> v = new(); List<int> t = new();
            for (int i = 0; i < 12; i++)
            { float a=i*Mathf.PI/6f; v.Add(new Vector3(Mathf.Cos(a)*.5f,-.5f,Mathf.Sin(a)*.5f)); v.Add(new Vector3(Mathf.Cos(a)*.5f,.5f,Mathf.Sin(a)*.5f)); }
            for (int i = 0; i < 12; i++) { int a=i*2,b=(i*2+2)%24; t.AddRange(new[]{a,a+1,b,b,a+1,b+1}); }
            Mesh mesh = new() { name = "Deck bollard cylinder" }; mesh.SetVertices(v); mesh.SetTriangles(t,0); mesh.RecalculateNormals(); return mesh;
        }

        private void OnDestroy()
        {
            foreach (Material m in ownedMaterials) if (m != null) Destroy(m);
            foreach (Mesh mesh in aircraftMeshes) if (mesh != null) Destroy(mesh);
            if (roundedMesh != null) Destroy(roundedMesh);
            if (rodMesh != null) Destroy(rodMesh);
        }
    }
}
