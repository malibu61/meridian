using System.Collections.Generic;
using UnityEngine;

namespace Meridian
{
    public sealed class EnvironmentResources : MonoBehaviour
    {
        private readonly List<Object> owned = new();
        public T Own<T>(T resource) where T : Object { owned.Add(resource); return resource; }
        private void OnDestroy() { foreach (Object resource in owned) if (resource != null) Destroy(resource); }
    }
}
