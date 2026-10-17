using UnityEngine;

namespace Meridian
{
    [RequireComponent(typeof(Renderer))]
    public sealed class HullNumberVisibility : MonoBehaviour
    {
        public float SideSign { get; set; }
        private Renderer labelRenderer;

        private void Awake() => labelRenderer = GetComponent<Renderer>();

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null || labelRenderer == null) return;
            float cameraSide = Vector3.Dot(transform.root.right, camera.transform.position - transform.root.position);
            labelRenderer.enabled = cameraSide * SideSign > 0.03f;
        }
    }
}
