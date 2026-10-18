using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meridian
{
    /// <summary>Exterior study of F244's pre-MLU configuration, using public 2009–2022 photographs.
    /// One model unit is approximately four metres. Positive Z is the bow, positive X starboard.</summary>
    public sealed class BarbarosModelBuilder
    {
        private readonly Transform root;
        private readonly Material hull, upper, deck, dark, glass, white, rubber, red;
        private readonly List<MeshFilter> mergeCandidates = new();
        private static readonly float[] Stations = { -14.25f, -13f, -10f, -6f, -2f, 2f, 6f, 9f, 11f, 12.8f, 14.2f, 15.1f };
        private static readonly float[] Widths = { 1.43f, 1.65f, 1.79f, 1.84f, 1.85f, 1.85f, 1.79f, 1.62f, 1.33f, 0.94f, 0.45f, 0.025f };

        public BarbarosModelBuilder(Transform parent)
        {
            root = parent;
            hull = Paint("Naval hull grey", new Color(0.46f, 0.51f, 0.53f), 0.28f);
            upper = Paint("Superstructure grey", new Color(0.58f, 0.62f, 0.63f), 0.26f);
            deck = Paint("Non-slip deck", new Color(0.24f, 0.28f, 0.29f), 0.13f);
            dark = Paint("Machinery and louvers", new Color(0.16f, 0.19f, 0.20f), 0.2f);
            glass = Paint("Bridge glazing", new Color(0.055f, 0.10f, 0.13f), 0.72f);
            white = Paint("Deck paint", new Color(0.82f, 0.84f, 0.80f), 0.16f);
            rubber = Paint("RHIB tubes", new Color(0.11f, 0.13f, 0.14f), 0.12f);
            red = Paint("Lifebuoys", new Color(0.67f, 0.12f, 0.07f), 0.2f);
        }

        public void Build()
        {
            BuildHull();
            BuildSuperstructure();
            BuildFunnels();
            BuildMasts();
            BuildDeckEquipment();
            BuildAviationDeck();
            BuildRails();
            BuildHullNumbers();
            CombineStaticParts();
            GameObject activity = new("Deck life");
            activity.transform.SetParent(root, false);
            activity.AddComponent<NavalDeckLife>().Build();
        }

        public static float HalfWidth(float z)
        {
            for (int i = 1; i < Stations.Length; i++)
                if (z <= Stations[i]) return Mathf.Lerp(Widths[i - 1], Widths[i], Mathf.InverseLerp(Stations[i - 1], Stations[i], z));
            return Widths[Widths.Length - 1];
        }

        private static float DeckHeight(float z) => 1.02f + Mathf.SmoothStep(0f, 0.43f, Mathf.InverseLerp(7f, 15.1f, z));

        private static Vector3 HullPoint(float side, float z, float y)
        {
            // Match the actual triangle diagonal on each side, not an approximate interpolated width.
            const int sections = 70;
            float section = Mathf.Clamp((z + 14.25f) / 29.35f * sections, 0f, sections - 0.0001f);
            int index = Mathf.FloorToInt(section);
            float z0 = Mathf.Lerp(-14.25f, 15.1f, index / (float)sections);
            float z1 = Mathf.Lerp(-14.25f, 15.1f, (index + 1) / (float)sections);
            Vector3 top0 = new(side * HalfWidth(z0), DeckHeight(z0), z0);
            Vector3 top1 = new(side * HalfWidth(z1), DeckHeight(z1), z1);
            Vector3 bottom0 = new(side * HalfWidth(z0) * 0.85f, -0.63f, z0);
            Vector3 bottom1 = new(side * HalfWidth(z1) * 0.85f, -0.63f, z1);
            if (side > 0f)
            {
                if (TriangleSurface(top0, top1, bottom1, y, z, out Vector3 point)) return point;
                TriangleSurface(top0, bottom1, bottom0, y, z, out point); return point;
            }
            if (TriangleSurface(bottom0, bottom1, top1, y, z, out Vector3 portPoint)) return portPoint;
            TriangleSurface(bottom0, top1, top0, y, z, out portPoint); return portPoint;
        }

        private static bool TriangleSurface(Vector3 a, Vector3 b, Vector3 c, float y, float z, out Vector3 point)
        {
            Vector2 u = new(b.y - a.y, b.z - a.z), v = new(c.y - a.y, c.z - a.z), p = new(y - a.y, z - a.z);
            float determinant = u.x * v.y - u.y * v.x;
            float beta = (p.x * v.y - p.y * v.x) / determinant;
            float gamma = (u.x * p.y - u.y * p.x) / determinant;
            point = new Vector3(a.x + beta * (b.x - a.x) + gamma * (c.x - a.x), y, z);
            return beta >= -0.0001f && gamma >= -0.0001f && beta + gamma <= 1.0001f;
        }

        private void BuildHull()
        {
            const int sections = 70;
            List<Vector3> vertices = new();
            List<int> triangles = new();
            for (int i = 0; i <= sections; i++)
            {
                float z = Mathf.Lerp(-14.25f, 15.1f, i / (float)sections);
                float w = HalfWidth(z);
                float y = DeckHeight(z);
                float keel = Mathf.Lerp(-1.12f, -0.28f, Mathf.InverseLerp(11f, 15.1f, z));
                float bowRake = Mathf.SmoothStep(0f, 0.72f, Mathf.InverseLerp(13.2f, 15.1f, z));
                vertices.Add(new Vector3(-w, y, z));
                vertices.Add(new Vector3(w, y, z));
                vertices.Add(new Vector3(w * 0.85f, -0.63f, z - bowRake));
                vertices.Add(new Vector3(w * 0.60f, keel, z - bowRake * 1.25f));
                vertices.Add(new Vector3(-w * 0.60f, keel, z - bowRake * 1.25f));
                vertices.Add(new Vector3(-w * 0.85f, -0.63f, z - bowRake));
            }
            for (int i = 0; i < sections; i++)
            for (int edge = 0; edge < 6; edge++)
            {
                int a = i * 6 + edge, b = i * 6 + (edge + 1) % 6;
                Quad(triangles, a, a + 6, b + 6, b);
            }
            for (int j = 1; j < 5; j++) { triangles.Add(0); triangles.Add(j); triangles.Add(j + 1); }
            int last = sections * 6;
            for (int j = 1; j < 5; j++) { triangles.Add(last); triangles.Add(last + j + 1); triangles.Add(last + j); }
            Mesh mesh = new() { name = "F244 closed flared hull" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            MeshObject("Hull", mesh, hull);

            // Follow the hull outline rather than laying rectangular slabs over the bow.
            List<Vector3> deckVertices = new(); List<int> deckTriangles = new();
            for (int i = 0; i <= sections; i++)
            {
                float z = Mathf.Lerp(-14.20f, 15.02f, i / (float)sections);
                float w = Mathf.Max(0.01f, HalfWidth(z) - 0.055f);
                deckVertices.Add(new Vector3(-w, DeckHeight(z) + 0.009f, z));
                deckVertices.Add(new Vector3(w, DeckHeight(z) + 0.009f, z));
                if (i < sections) Quad(deckTriangles, i * 2, i * 2 + 2, i * 2 + 3, i * 2 + 1);
            }
            Mesh top = new() { name = "Fitted non-slip deck" };
            top.SetVertices(deckVertices); top.SetTriangles(deckTriangles, 0); top.RecalculateNormals(); top.RecalculateBounds();
            MeshObject("Non-slip deck", top, deck);

            for (int side = -1; side <= 1; side += 2)
            {
                // Stern bulwarks: three real openings with horizontal safety bars.
                for (int bay = 0; bay < 3; bay++)
                {
                    float z = -13.45f + bay * 1.2f;
                    float x = side * (HalfWidth(z) - 0.055f);
                    Box("Flight deck bulwark sill", new Vector3(x, 1.13f, z), new Vector3(0.11f, 0.22f, 1.22f), hull);
                    Box("Flight deck bulwark cap", new Vector3(x, 1.72f, z), new Vector3(0.11f, 0.15f, 1.22f), hull);
                    Box("Bulwark frame", new Vector3(x, 1.43f, z - 0.56f), new Vector3(0.13f, 0.50f, 0.14f), hull);
                    for (int bar = 0; bar < 2; bar++)
                        Rod("Opening safety bar", new Vector3(x, 1.30f + bar * 0.16f, z - 0.5f), new Vector3(x, 1.30f + bar * 0.16f, z + 0.5f), 0.012f, hull);
                }
                // Dark anchor pockets and hawse pipes near the bow.
                Vector3 anchor = HullPoint(side, 12.55f, 0.27f);
                Box("Anchor recess", anchor + new Vector3(side * 0.018f, 0f, 0f), new Vector3(0.055f, 0.23f, 0.42f), dark);
                Rod("Anchor shank", anchor + new Vector3(side * 0.055f, -0.13f, 0f), anchor + new Vector3(side * 0.055f, 0.13f, 0f), 0.035f, hull);
            }
        }

        private void BuildSuperstructure()
        {
            Block("Forward deckhouse", new Vector3(0f, 1.62f, 3.48f), 3.25f, 2.92f, 1.18f, 6.0f, 0.26f, upper);
            Block("Bridge", new Vector3(0f, 2.69f, 4.22f), 3.02f, 2.85f, 0.96f, 2.75f, -0.12f, upper);
            Block("Bridge roof", new Vector3(0f, 3.21f, 4.18f), 3.14f, 3.10f, 0.12f, 2.94f, 0f, upper);
            // Individual glazing, mullions and bridge wings on both sides.
            for (int i = -3; i <= 3; i++)
                Box("Bridge forward pane", new Vector3(i * 0.38f, 2.85f, 5.72f), new Vector3(0.31f, 0.28f, 0.035f), glass);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 4; i++)
                    Box("Bridge side pane", new Vector3(side * 1.47f, 2.85f, 3.52f + i * 0.43f), new Vector3(0.035f, 0.28f, 0.33f), glass);
                Box("Bridge wing", new Vector3(side * 1.59f, 2.37f, 3.18f), new Vector3(0.7f, 0.18f, 1.2f), upper);
                Rail(new Vector3(side * 1.91f, 2.48f, 2.64f), new Vector3(side * 1.91f, 2.48f, 3.70f), 0.34f, 4);
                // Door apertures are cut into the bridge shell below, rather than painted over a solid wall.
                for (int jamb = 0; jamb < 2; jamb++)
                    Box("Bridge wing door jamb", new Vector3(side * 1.47f, 2.735f, 3.035f + jamb * .43f), new Vector3(.055f,.55f,.025f), hull);
                Box("Bridge wing door lintel", new Vector3(side * 1.45f, 3.02f, 3.25f), new Vector3(.055f,.045f,.46f), hull);
                Box("Bridge wing door threshold", new Vector3(side * 1.47f, 2.455f, 3.25f), new Vector3(.18f,.012f,.40f), dark);
                Box("Open inward bridge door", new Vector3(side * 1.27f, 2.735f, 3.455f), new Vector3(.37f,.53f,.027f), hull);
                Box("Open door handle", new Vector3(side * 1.13f, 2.71f, 3.43f), new Vector3(.055f,.018f,.025f), dark);
                for (int i = 0; i < 5; i++)
                {
                    Box("Deckhouse window", new Vector3(side * 1.58f, 1.83f, 1.3f + i * 0.7f), new Vector3(0.028f, 0.17f, 0.24f), glass);
                    Box("Access door", new Vector3(side * 1.55f, 1.45f, 0.95f + i * 0.83f), new Vector3(0.035f, 0.54f, 0.25f), hull);
                }
            }
            Box("Bridge interior walking floor", new Vector3(0f, 2.43f, 3.25f), new Vector3(2.88f,.06f,1.00f), deck);
            Box("Bridge interior aft lining", new Vector3(0f,2.73f,2.88f), new Vector3(2.75f,.54f,.035f), dark);
            Box("Bridge interior forward partition", new Vector3(0f,2.73f,3.77f), new Vector3(2.68f,.54f,.035f), dark);
            Block("Engine enclosure", new Vector3(0f, 1.72f, -0.75f), 2.66f, 2.42f, 1.36f, 3.1f, 0.08f, upper);
            Block("Hangar", new Vector3(0f, 1.91f, -6.55f), 3.10f, 2.86f, 1.75f, 4.55f, 0.13f, upper);
            Box("Closed hangar door", new Vector3(0f, 1.76f, -8.84f), new Vector3(2.30f, 1.40f, 0.035f), hull);
            for (int i = 0; i < 12; i++)
                Box("Hangar horizontal door rib", new Vector3(0f, 1.15f + i * 0.109f, -8.868f), new Vector3(2.26f, 0.018f, 0.018f), dark);
            Box("Hangar door lintel", new Vector3(0f, 2.57f, -8.89f), new Vector3(2.55f, 0.13f, 0.12f), upper);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int vent = 0; vent < 3; vent++)
                    Louver(new Vector3(side * 1.49f, 2.0f, -5.0f - vent * 0.92f), side);
                Box("Hangar side door", new Vector3(side * 1.52f, 1.58f, -7.9f), new Vector3(0.03f, 0.65f, 0.32f), hull);
                Rod("Side pipe", new Vector3(side * 1.55f, 1.21f, -8.2f), new Vector3(side * 1.55f, 1.21f, -4.4f), 0.016f, upper);
            }
        }

        private void BuildFunnels()
        {
            // Broad oval uptakes with dark exhaust bands, not black rectangular towers.
            for (int side = -1; side <= 1; side += 2)
            {
                Transform funnel = Group("Oval funnel", new Vector3(side * 0.68f, 2.22f, -1.05f));
                funnel.localRotation = Quaternion.Euler(-7f, 0f, side * -7f);
                OvalCylinder("Funnel body", funnel, new Vector3(0f, 0.68f, 0f), 0.52f, 0.75f, 1.36f, upper);
                OvalCylinder("Exhaust black band", funnel, new Vector3(0f, 1.40f, 0f), 0.53f, 0.76f, 0.21f, dark);
                OvalCylinder("Exhaust grille", funnel, new Vector3(0f, 1.52f, 0f), 0.44f, 0.64f, 0.025f, rubber);
            }
        }

        private void BuildMasts()
        {
            Block("Mast foundation", new Vector3(0f, 3.0f, 1.85f), 1.95f, 1.15f, 1.1f, 1.75f, 0.15f, upper);
            Vector3[] feet = { new(-0.66f, 3.32f, 1.30f), new(0.66f, 3.32f, 1.30f), new(0.66f, 3.32f, 2.42f), new(-0.66f, 3.32f, 2.42f) };
            for (int level = 0; level < 4; level++)
            {
                float t0 = level / 4f, t1 = (level + 1) / 4f;
                for (int edge = 0; edge < 4; edge++)
                {
                    Vector3 top = new(feet[edge].x * 0.36f, 6.45f, 1.86f + (feet[edge].z - 1.86f) * 0.36f);
                    Vector3 nextTop = new(feet[(edge + 1) % 4].x * 0.36f, 6.45f, 1.86f + (feet[(edge + 1) % 4].z - 1.86f) * 0.36f);
                    Vector3 a = Vector3.Lerp(feet[edge], top, t0), b = Vector3.Lerp(feet[edge], top, t1);
                    Vector3 c = Vector3.Lerp(feet[(edge + 1) % 4], nextTop, t1);
                    Rod("Lattice upright", a, b, 0.043f, hull);
                    Rod("Lattice diagonal", a, c, 0.027f, hull);
                    Rod("Lattice cross member", b, c, 0.028f, hull);
                }
            }
            Box("Mast upper platform", new Vector3(0f, 6.46f, 1.86f), new Vector3(1.15f, 0.09f, 1.12f), upper);
            Rod("Mast top pole", new Vector3(0f, 6.5f, 1.86f), new Vector3(0f, 7.65f, 1.86f), 0.035f, hull);
            for (int i = 0; i < 3; i++)
                Rod("Mast yardarm", new Vector3(-1.18f + i * 0.2f, 5.0f + i * 0.8f, 1.86f), new Vector3(1.18f - i * 0.2f, 5.0f + i * 0.8f, 1.86f), 0.035f, hull);
            Transform forwardRadar = Group("Forward search radar", new Vector3(0f, 6.94f, 1.86f));
            Part("Radar scanner", forwardRadar, new Vector3(0f, 0f, 0f), new Vector3(1.05f, 0.12f, 0.24f), dark);
            forwardRadar.gameObject.AddComponent<RotatingRadar>();
            Block("Aft radar pedestal", new Vector3(0f, 3.45f, -3.65f), 0.92f, 0.55f, 1.40f, 0.85f, 0.08f, upper);
            Transform search = Group("Aft SMART-S radar", new Vector3(0f, 4.35f, -3.65f));
            Part("Radar array", search, new Vector3(0f, 0f, 0f), new Vector3(1.72f, 0.42f, 0.20f), hull);
            Part("Radar aperture", search, new Vector3(0f, 0f, 0.115f), new Vector3(1.57f, 0.31f, 0.018f), dark);
            search.gameObject.AddComponent<RotatingRadar>();
            Sphere("Forward fire control radome", new Vector3(0f, 3.84f, 3.45f), new Vector3(0.64f, 0.66f, 0.64f), upper);
            Sphere("Aft fire control radome", new Vector3(0f, 3.14f, -5.0f), new Vector3(0.57f, 0.62f, 0.57f), upper);
            for (int side = -1; side <= 1; side += 2)
            {
                Sphere("SATCOM", new Vector3(side * 1.08f, 3.14f, -6.1f), new Vector3(0.32f, 0.46f, 0.32f), white);
                Rod("Whip antenna", new Vector3(side * 0.95f, 2.85f, -7.05f), new Vector3(side * 1.02f, 5.14f, -7.1f), 0.015f, dark);
                Rod("Bridge whip", new Vector3(side * 1.1f, 3.3f, 4.22f), new Vector3(side * 1.1f, 4.4f, 4.22f), 0.013f, hull);
                Rod("Signal halyard", new Vector3(side * 1.07f, 5.0f, 1.86f), new Vector3(side * 1.35f, 2.52f, 3.1f), 0.006f, dark);
            }
        }

        private void BuildDeckEquipment()
        {
            Block("Mk45 gun housing", new Vector3(0f, 1.77f, 11.15f), 1.10f, 0.82f, 1.08f, 1.35f, 0.21f, upper);
            Rod("Gun barrel", new Vector3(0f, 2.04f, 11.6f), new Vector3(0f, 2.18f, 13.75f), 0.065f, hull);
            Rod("Muzzle", new Vector3(0f, 2.17f, 13.59f), new Vector3(0f, 2.18f, 13.77f), 0.081f, dark);
            Box("VLS coaming", new Vector3(0f, 1.17f, 8.05f), new Vector3(1.65f, 0.16f, 1.65f), hull);
            for (int row = 0; row < 4; row++)
            for (int col = 0; col < 2; col++)
            {
                Vector3 cell = new(-0.38f + col * 0.76f, 1.266f, 7.45f + row * 0.39f);
                Box("VLS hatch", cell, new Vector3(0.67f, 0.025f, 0.31f), deck);
                Box("VLS hatch hinge", cell + new Vector3(0f, 0.017f, -0.12f), new Vector3(0.46f, 0.02f, 0.03f), hull);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                // Quad packs with eight visually separate canisters overall.
                Transform pack = Group("Harpoon quad pack", new Vector3(side * 0.85f, 2.46f, 0.0f));
                pack.localRotation = Quaternion.Euler(-18f, side * 55f, 0f);
                for (int row = 0; row < 2; row++)
                for (int col = 0; col < 2; col++)
                    Part("Missile canister", pack, new Vector3(col * 0.31f - 0.155f, row * 0.30f, 0f), new Vector3(0.27f, 0.27f, 1.42f), hull);
                Box("Canister platform", new Vector3(side * 0.88f, 2.22f, 0f), new Vector3(1.38f, 0.12f, 1.32f), upper);
                // RHIB and davits in the amidships working recess.
                Sphere("RHIB flotation tube", new Vector3(side * 1.48f, 1.42f, -3.07f), new Vector3(0.47f, 0.27f, 1.52f), rubber);
                Box("RHIB interior", new Vector3(side * 1.48f, 1.51f, -3.07f), new Vector3(0.27f, 0.08f, 1.16f), hull);
                for (int i = 0; i < 2; i++)
                {
                    float z = -3.62f + i * 1.05f;
                    Rod("Boat davit upright", new Vector3(side * 1.17f, 1.1f, z), new Vector3(side * 1.17f, 2.32f, z), 0.045f, upper);
                    Rod("Boat davit arm", new Vector3(side * 1.17f, 2.32f, z), new Vector3(side * 1.70f, 2.15f, z), 0.045f, upper);
                    Rod("Davit cable", new Vector3(side * 1.70f, 2.15f, z), new Vector3(side * 1.70f, 1.56f, z), 0.008f, dark);
                }
                // Life raft canisters, vents, hatches, lifebuoys and bollards.
                for (int i = 0; i < 4; i++)
                    Sphere("Life raft canister", new Vector3(side * 1.25f, 2.91f, -6.45f + i * 0.43f), new Vector3(0.37f, 0.26f, 0.27f), white);
                for (int i = 0; i < 3; i++)
                {
                    float z = 3f + i * 1.2f;
                    Sphere("Lifebuoy", new Vector3(side * 1.64f, 1.7f, z), new Vector3(0.05f, 0.21f, 0.21f), red);
                }
                for (int i = 0; i < 2; i++)
                {
                    float z = 12.9f + i * 0.65f;
                    Rod("Bow bollard", new Vector3(side * 0.42f, 1.25f, z), new Vector3(side * 0.42f, 1.45f, z), 0.07f, dark);
                }
                Box("Sea Zenith pedestal", new Vector3(side * 1.10f, 2.95f, -7.42f), new Vector3(0.36f, 0.47f, 0.40f), hull);
                Sphere("Sea Zenith mount", new Vector3(side * 1.10f, 3.25f, -7.42f), new Vector3(0.42f, 0.35f, 0.42f), upper);
                for (int barrel = 0; barrel < 2; barrel++)
                    Rod("CIWS barrel", new Vector3(side * 1.10f + barrel * 0.07f, 3.32f, -7.5f), new Vector3(side * 1.1f + barrel * 0.07f, 3.39f, -8.0f), 0.019f, dark);
            }
            for (int i = 0; i < 3; i++)
                Box("Foredeck access hatch", new Vector3(0.6f, DeckHeight(9.4f) + 0.035f, 9.35f + i * 0.36f), new Vector3(0.32f, 0.04f, 0.25f), hull);
        }

        private void BuildAviationDeck()
        {
            // Circular landing boundary, H, centerline, traversing rails and deck-edge nets.
            const float y = 1.042f;
            Vector3 center = new(0f, y, -11.15f);
            for (int i = 0; i < 48; i++)
            {
                float a = i * Mathf.PI * 2f / 48f, b = (i + 1) * Mathf.PI * 2f / 48f;
                Rod("Landing circle", center + new Vector3(Mathf.Cos(a) * 1.13f, 0f, Mathf.Sin(a) * 1.13f),
                    center + new Vector3(Mathf.Cos(b) * 1.13f, 0f, Mathf.Sin(b) * 1.13f), 0.018f, white);
            }
            Box("H left", center + new Vector3(-0.32f, 0f, 0f), new Vector3(0.09f, 0.015f, 0.96f), white);
            Box("H right", center + new Vector3(0.32f, 0f, 0f), new Vector3(0.09f, 0.015f, 0.96f), white);
            Box("H middle", center, new Vector3(0.64f, 0.015f, 0.09f), white);
            for (int i = 0; i < 6; i++)
                Box("Landing centerline", new Vector3(0f, y, -13.6f + i * 0.35f), new Vector3(0.055f, 0.015f, 0.20f), white);
            Box("Helicopter traversing rail", new Vector3(0f, 1.04f, -9.36f), new Vector3(0.035f, 0.015f, 1.0f), hull);
            Rod("Stern flag pole", new Vector3(0f, 1.06f, -14f), new Vector3(0f, 2.54f, -14f), 0.024f, hull);
            for (int i = -3; i <= 3; i++)
            {
                Rod("Stern net support", new Vector3(i * 0.40f, 1.05f, -14.20f), new Vector3(i * 0.40f, 0.94f, -14.54f), 0.012f, hull);
                if (i < 3) Rod("Stern net edge", new Vector3(i * 0.40f, 0.94f, -14.54f), new Vector3((i + 1) * 0.40f, 0.94f, -14.54f), 0.013f, hull);
            }
        }

        private void BuildRails()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 43; i++)
                {
                    float z = Mathf.Lerp(-10.2f, 14.75f, i / 42f);
                    float nextZ = Mathf.Lerp(-10.2f, 14.75f, (i + 1) / 42f);
                    Vector3 a = new(side * (HalfWidth(z) - 0.045f), DeckHeight(z) + 0.015f, z);
                    Vector3 b = new(side * (HalfWidth(nextZ) - 0.045f), DeckHeight(nextZ) + 0.015f, nextZ);
                    Rod("Deck stanchion", a, a + Vector3.up * 0.27f, 0.012f, hull);
                    if (i < 42)
                    {
                        Rod("Upper lifeline", a + Vector3.up * 0.27f, b + Vector3.up * 0.27f, 0.008f, hull);
                        Rod("Lower lifeline", a + Vector3.up * 0.13f, b + Vector3.up * 0.13f, 0.006f, hull);
                    }
                }
                Rail(new Vector3(side * 1.40f, 2.83f, -8.63f), new Vector3(side * 1.40f, 2.83f, -4.53f), 0.27f, 9);
            }
        }

        private void BuildHullNumbers()
        {
            Shader shader = Resources.Load<Shader>("HullMarking") ?? Shader.Find("Meridian/Hull Marking") ?? Shader.Find("Unlit/Transparent Cutout");
            Material marking = new(shader) { name = "Depth-tested F244 paint", mainTexture = Texture2D.whiteTexture, color = new Color(0.88f, 0.89f, 0.85f) };
            marking.SetFloat("_Cutoff", 0.45f);
            for (int side = -1; side <= 1; side += 2)
            {
                Pennant("Bow pennant F244", side, 11.1f, 0.20f, 1.78f, 0.42f, marking);
                Pennant("Aft pennant F244", side, -9.35f, 0.25f, 1.65f, 0.42f, marking);
            }
        }

        private void Pennant(string name, int side, float z, float y, float width, float height, Material material)
        {
            // Solid stencil strokes avoid alpha mipmaps breaking up the small port-side marking.
            string[] glyphs = { "11111/10000/10000/11110/10000/10000/10000", "01110/10001/00001/00010/00100/01000/11111", "10010/10010/10010/11111/00010/00010/00010", "10010/10010/10010/11111/00010/00010/00010" };
            List<Vector3> vertices = new(); List<int> triangles = new();
            for (int letter = 0; letter < glyphs.Length; letter++)
            {
                string[] rows = glyphs[letter].Split('/');
                for (int row = 0; row < 7; row++)
                for (int column = 0; column < 5; column++)
                {
                    if (rows[row][column] != '1') continue;
                    float u0 = (letter * 6f + column) / 23f;
                    float u1 = (letter * 6f + column + 1f) / 23f;
                    float v0 = (6f - row) / 7f;
                    float v1 = (7f - row) / 7f;
                    // Subdivide strokes in both axes to follow the hull's triangle changes.
                    for (int subY = 0; subY < 2; subY++)
                    for (int subX = 0; subX < 2; subX++)
                    {
                        float left = Mathf.Lerp(u0, u1, subX * 0.5f);
                        float right = Mathf.Lerp(u0, u1, (subX + 1) * 0.5f);
                        float bottom = Mathf.Lerp(v0, v1, subY * 0.5f);
                        float top = Mathf.Lerp(v0, v1, (subY + 1) * 0.5f);
                        int first = vertices.Count;
                        vertices.Add(PennantPoint(side, z, y, width, height, left, bottom));
                        vertices.Add(PennantPoint(side, z, y, width, height, left, top));
                        vertices.Add(PennantPoint(side, z, y, width, height, right, top));
                        vertices.Add(PennantPoint(side, z, y, width, height, right, bottom));
                        Quad(triangles, first, first + 1, first + 2, first + 3);
                    }
                }
            }
            Mesh mesh = new() { name = name };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            MeshObject(name + (side < 0 ? " port" : " starboard"), mesh, material, false);
        }

        private static Vector3 PennantPoint(int side, float z, float y, float width, float height, float u, float v)
        {
            Vector3 point = HullPoint(side, z + side * (u - 0.5f) * width, y + (v - 0.5f) * height);
            point.x += side * 0.020f;
            return point;
        }

        private Material Paint(string name, Color color, float smoothness)
        {
            Material material = new(Shader.Find("Standard")) { name = name, color = color };
            material.SetFloat("_Metallic", 0.02f); material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private Transform Group(string name, Vector3 position)
        {
            GameObject group = new(name); group.transform.SetParent(root, false); group.transform.localPosition = position; return group.transform;
        }

        private void Box(string name, Vector3 position, Vector3 size, Material material) => Part(name, root, position, size, material);

        private GameObject Part(string name, Transform parent, Vector3 position, Vector3 size, Material material, PrimitiveType type = PrimitiveType.Cube)
        {
            GameObject part = GameObject.CreatePrimitive(type); part.name = name;
            Collider collider = part.GetComponent<Collider>(); collider.enabled = false; Object.Destroy(collider);
            part.transform.SetParent(parent, false); part.transform.localPosition = position; part.transform.localScale = size;
            part.GetComponent<Renderer>().sharedMaterial = material;
            if (parent == root) mergeCandidates.Add(part.GetComponent<MeshFilter>());
            return part;
        }

        private void Sphere(string name, Vector3 position, Vector3 size, Material material) => Part(name, root, position, size, material, PrimitiveType.Sphere);

        private void Rod(string name, Vector3 a, Vector3 b, float radius, Material material)
        {
            GameObject rod = Part(name, root, (a + b) * 0.5f, new Vector3(radius * 2f, (b - a).magnitude * 0.5f, radius * 2f), material, PrimitiveType.Cylinder);
            rod.transform.localRotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
        }

        private void Rail(Vector3 a, Vector3 b, float height, int posts)
        {
            for (int i = 0; i <= posts; i++) { Vector3 p = Vector3.Lerp(a, b, i / (float)posts); Rod("Safety stanchion", p, p + Vector3.up * height, 0.011f, hull); }
            Rod("Safety lifeline", a + Vector3.up * height, b + Vector3.up * height, 0.008f, hull);
            Rod("Safety lifeline", a + Vector3.up * height * 0.5f, b + Vector3.up * height * 0.5f, 0.006f, hull);
        }

        private void Louver(Vector3 p, int side)
        {
            Box("Vent surround", p, new Vector3(0.035f, 0.54f, 0.69f), hull);
            Box("Vent aperture", p + Vector3.right * side * 0.025f, new Vector3(0.025f, 0.45f, 0.59f), dark);
            for (int j = 0; j < 6; j++) Box("Vent louver", p + new Vector3(side * 0.045f, -0.19f + j * 0.075f, 0f), new Vector3(0.025f, 0.024f, 0.58f), hull);
        }

        private void OvalCylinder(string name, Transform parent, Vector3 position, float radiusX, float radiusZ, float height, Material material)
        {
            GameObject oval = Part(name, parent, position, new Vector3(radiusX * 2f, height * 0.5f, radiusZ * 2f), material, PrimitiveType.Cylinder);
        }

        private void Block(string name, Vector3 p, float lowerWidth, float upperWidth, float height, float length, float slope, Material material)
        {
            // Chamfered eight-corner footprint. Each face has independent vertices for crisp normals.
            const float bevel = 0.12f;
            List<Vector3> corners = new();
            for (int level = 0; level < 2; level++)
            {
                float w = (level == 0 ? lowerWidth : upperWidth) * 0.5f;
                float back = -length * 0.5f, front = length * 0.5f - (level == 1 ? slope : 0f);
                float y = (level - 0.5f) * height;
                corners.AddRange(new[] { new Vector3(-w + bevel,y,back),new Vector3(w - bevel,y,back),new Vector3(w,y,back + bevel),new Vector3(w,y,front - bevel),new Vector3(w - bevel,y,front),new Vector3(-w + bevel,y,front),new Vector3(-w,y,front - bevel),new Vector3(-w,y,back + bevel) });
            }
            List<Vector3> v = new(); List<int> t = new();
            for (int i = 0; i < 8; i++)
            {
                if (name == "Bridge" && (i == 2 || i == 6))
                    BridgeSideWithDoor(v,t,p,i == 2 ? 1 : -1);
                else Face(v, t, new[] { corners[i], corners[(i + 1) % 8], corners[(i + 1) % 8 + 8], corners[i + 8] }, p, Vector3.zero);
            }
            Vector3[] top = new Vector3[8], bottom = new Vector3[8];
            for (int i = 0; i < 8; i++) { top[i] = corners[i + 8]; bottom[i] = corners[i]; }
            Face(v, t, top, p, Vector3.up); Face(v, t, bottom, p, Vector3.down);
            Mesh mesh = new() { name = name, vertices = v.ToArray(), triangles = t.ToArray() }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            MeshObject(name, mesh, material);
        }

        private static void BridgeSideWithDoor(List<Vector3> vertices, List<int> triangles, Vector3 origin, int side)
        {
            // Match the shell's sloping side and chamfered corners, retaining only a person-sized opening.
            float[] ys = { 2.21f,2.46f,3.00f,3.17f };
            for (int band=0;band<3;band++)
            {
                float y0=ys[band],y1=ys[band+1];
                float x0=side*Mathf.Lerp(1.51f,1.425f,Mathf.InverseLerp(2.21f,3.17f,y0));
                float x1=side*Mathf.Lerp(1.51f,1.425f,Mathf.InverseLerp(2.21f,3.17f,y1));
                float front0=Mathf.Lerp(5.475f,5.595f,Mathf.InverseLerp(2.21f,3.17f,y0));
                float front1=Mathf.Lerp(5.475f,5.595f,Mathf.InverseLerp(2.21f,3.17f,y1));
                if (band == 1)
                {
                    WallStrip(2.965f,3.05f,2.965f,3.05f);
                    WallStrip(3.45f,front0,3.45f,front1);
                }
                else WallStrip(2.965f,front0,2.965f,front1);
                void WallStrip(float back0,float end0,float back1,float end1)
                {
                    Vector3[] points={new(x0,y0,back0),new(x0,y0,end0),new(x1,y1,end1),new(x1,y1,back1)};
                    for(int i=0;i<points.Length;i++)points[i]-=origin;
                    Face(vertices,triangles,points,origin,Vector3.right*side);
                }
            }
        }

        private static void Face(List<Vector3> vertices, List<int> triangles, Vector3[] points, Vector3 offset, Vector3 expected)
        {
            Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);
            if (expected == Vector3.zero) { foreach (Vector3 point in points) expected += point; expected /= points.Length; }
            bool flip = Vector3.Dot(normal, expected) < 0f;
            int first = vertices.Count; foreach (Vector3 point in points) vertices.Add(point + offset);
            for (int i = 1; i < points.Length - 1; i++) { triangles.Add(first); triangles.Add(first + (flip ? i + 1 : i)); triangles.Add(first + (flip ? i : i + 1)); }
        }

        private void MeshObject(string name, Mesh mesh, Material material, bool combine = true)
        {
            GameObject obj = new(name); obj.transform.SetParent(root, false);
            MeshFilter filter = obj.AddComponent<MeshFilter>(); filter.sharedMesh = mesh;
            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (name.StartsWith("Bow pennant") || name.StartsWith("Aft pennant"))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            if (combine) mergeCandidates.Add(filter);
        }

        private static void Quad(List<int> t, int a, int b, int c, int d)
        { t.Add(a);t.Add(b);t.Add(c);t.Add(a);t.Add(c);t.Add(d); }

        private void CombineStaticParts()
        {
            // Detail count need not become a draw call per stanchion on a phone.
            Dictionary<Material, List<CombineInstance>> batches = new();
            foreach (MeshFilter filter in mergeCandidates)
            {
                Material material = filter.GetComponent<MeshRenderer>().sharedMaterial;
                if (!batches.ContainsKey(material)) batches[material] = new List<CombineInstance>();
                batches[material].Add(new CombineInstance { mesh = filter.sharedMesh, transform = root.worldToLocalMatrix * filter.transform.localToWorldMatrix });
                filter.gameObject.SetActive(false);
            }
            foreach (KeyValuePair<Material,List<CombineInstance>> batch in batches)
            {
                Mesh combined = new() { name = "F244 " + batch.Key.name, indexFormat = IndexFormat.UInt32 };
                combined.CombineMeshes(batch.Value.ToArray(), true, true);
                MeshObject("Exterior / " + batch.Key.name, combined, batch.Key, false);
            }
        }
    }
}
