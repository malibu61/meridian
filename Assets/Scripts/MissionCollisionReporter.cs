using UnityEngine;
namespace Meridian
{
    public sealed class MissionCollisionReporter:MonoBehaviour
    {
        public MissionController Owner{get;set;}
        private void OnCollisionEnter(Collision collision)
        {if(Owner!=null)Owner.ReportCollision(Vector3.ProjectOnPlane(collision.relativeVelocity,Vector3.up).magnitude*VesselCatalog.ModelMetresPerUnit);}
    }
}
