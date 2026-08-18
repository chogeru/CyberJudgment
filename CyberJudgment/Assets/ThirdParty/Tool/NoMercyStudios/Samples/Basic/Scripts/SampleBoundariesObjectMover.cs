using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public class SampleBoundariesObjectMover : MonoBehaviour
    {
        [SerializeField] private LevelBoundaryTrackedObject trackedObject = null;
        public LevelBoundaryTrackedObject TrackedObject
        {
            get
            {
                if (this.trackedObject == null)
                {
                    LevelBoundaryTrackedObject attachedTrackedObject = this.gameObject.GetComponent<LevelBoundaryTrackedObject>();
                    if (attachedTrackedObject == null)
                        attachedTrackedObject = this.gameObject.AddComponent<LevelBoundaryTrackedObject>();
                    return attachedTrackedObject;
                }
                return this.trackedObject;
            }
        }

        public float speed = 5f;
        public float stayDuration = 2f; // Time to stay at each targetted position.

        private Vector3 targetPosition;
        private bool isMovingToInside = false;
        private float stayTimer = 0f;

        private void Start()
        {
            // Init status
            if (LevelBoundariesManager.Instance != null)
            {
                isMovingToInside = !(LevelBoundariesManager.Instance.IsInsideAnyBoundary(this.TrackedObject));
            }

            // Set first random position
            SetRandomTargetPosition();
        }

        private void Update()
        {
            MoveTowardsTarget();

            // Check if we have reached the target position
            if (Vector3.Distance(this.TrackedObject.transform.position, targetPosition) < 0.1f)
            {
                if (stayTimer > stayDuration)
                {
                    isMovingToInside = !isMovingToInside;
                    SetRandomTargetPosition();
                    stayTimer = 0f;
                }
                else
                {
                    stayTimer += Time.deltaTime;
                }
            }
        }

        [Header("Sample - Move inside a specific boundary")]
        [SerializeField] private LevelBoundary sampleMoveBoundaries = null;

        private void SetRandomTargetPosition()
        {
            // If no boundaries, end here
            if (this.sampleMoveBoundaries == null)
                return;

            // Randomise X/Y/Z factor (-1 or 1)
            System.Random rand = new System.Random();
            bool randomXFactor = rand.Next(0, 2) == 0;  // It'll generate either 0 or 1
            bool randomYFactor = rand.Next(0, 2) == 0;  // It'll generate either 0 or 1
            bool randomZFactor = rand.Next(0, 2) == 0;  // It'll generate either 0 or 1


            // Determine half boundary
            Vector3 halfBoundarySize;
            if (isMovingToInside)
            {
                halfBoundarySize = this.sampleMoveBoundaries.BoxedBoundarySize * 0.5f;
            }
            else
            {
                halfBoundarySize = (1.1f * this.sampleMoveBoundaries.BoxedBoundarySize) * 0.5f;
            }

            // Determine boundaries (center to exterior)
            // Center
            Vector3 center = this.sampleMoveBoundaries.BoxedBoundaryCenter;
            // Inside
            Vector3 minBoundary = Vector3.zero;
            if (isMovingToInside == false)
                minBoundary = (this.sampleMoveBoundaries.BoxedBoundarySize * 0.5f);

            // Exterior
            Vector3 maxBoundary = new Vector3
            (
                (randomXFactor == true ? 1f : -1f) * Random.Range(minBoundary.x, halfBoundarySize.x),
                (randomYFactor == true ? 1f : -1f) * Random.Range(minBoundary.y, halfBoundarySize.y),
                (randomZFactor == true ? 1f : -1f) * Random.Range(minBoundary.z, halfBoundarySize.z)
            );

            // Use all this to determine the next move
            targetPosition = new Vector3
            (
                 center.x + maxBoundary.x,
                 center.y + maxBoundary.y,
                 center.z + maxBoundary.z
            );
        }

        private void MoveTowardsTarget()
        {
            this.TrackedObject.transform.position = Vector3.MoveTowards(this.TrackedObject.transform.position, targetPosition, speed * Time.deltaTime);
        }

#if UNITY_EDITOR
        // Debug
        private void OnDrawGizmos()
        {
            if (Application.isPlaying == true)
            {
                Gizmos.color = Color.blue;
                if (this.TrackedObject != null)
                    Gizmos.DrawLine(this.TrackedObject.transform.position, targetPosition);
                Gizmos.DrawSphere(targetPosition, 5f);
            }
        }
#endif
    }
}