using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public class SampleBoundariesObjectLookAt : MonoBehaviour
    {
        [SerializeField] private Transform targetObject = null;

        public Transform TargetObject
        {
            get
            {
                if (this.targetObject == null)
                    return this.transform;
                return this.targetObject;
            }
        }

        private void Update()
        {
            LookAtTarget();
        }

        private void LookAtTarget()
        {
            Vector3 directionToTarget = TargetObject.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = targetRotation;
        }
    }

}