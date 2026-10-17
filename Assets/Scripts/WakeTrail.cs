using UnityEngine;

namespace Meridian
{
    public sealed class WakeTrail : MonoBehaviour
    {
        private ShipController ship;
        private ParticleSystem portWake;
        private ParticleSystem starboardWake;
        private ParticleSystem bowSpray;

        public void Configure(ShipController controller, Material foamMaterial)
        {
            ship = controller;
            portWake = CreateEmitter("Port propeller wake", new Vector3(-1.05f, -0.45f, -13.2f), foamMaterial);
            starboardWake = CreateEmitter("Starboard propeller wake", new Vector3(1.05f, -0.45f, -13.2f), foamMaterial);
            bowSpray = CreateEmitter("Bow spray", new Vector3(0f, -0.18f, 14.4f), foamMaterial, 32f);
        }

        private void Update()
        {
            if (ship == null || portWake == null || starboardWake == null) return;

            float baseRate = ship.SpeedKnots * 5f;
            SetRate(portWake, baseRate + Mathf.Abs(ship.PortThrottle) * 18f);
            SetRate(starboardWake, baseRate + Mathf.Abs(ship.StarboardThrottle) * 18f);
            SetRate(bowSpray, Mathf.Max(0f, ship.SpeedKnots - 1f) * 2.8f);
        }

        private ParticleSystem CreateEmitter(string emitterName, Vector3 localPosition, Material foamMaterial, float angle = 14f)
        {
            GameObject emitter = new(emitterName);
            emitter.transform.SetParent(transform, false);
            emitter.transform.localPosition = localPosition;
            emitter.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            ParticleSystem particles = emitter.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 4.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.82f, 0.95f, 0.97f, 0.55f),
                new Color(1f, 1f, 1f, 0.2f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 260;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = 0.18f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.65f, 0.88f, 0.92f), 1f) },
                new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.35f, 1f, 1.8f));

            ParticleSystemRenderer particleRenderer = emitter.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = foamMaterial;
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particles.Play();
            return particles;
        }

        private static void SetRate(ParticleSystem particles, float rate)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = Mathf.Clamp(rate, 0f, 75f);
        }
    }
}
