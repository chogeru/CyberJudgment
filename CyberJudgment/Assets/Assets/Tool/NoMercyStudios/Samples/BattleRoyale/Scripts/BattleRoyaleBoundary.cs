using NoMercyStudios.BoundariesPro;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace NoMercyStudios.BoundariesPro.Samples.FPS
{
    public class BattleRoyaleBoundary : MonoBehaviour
    {
        // Reference to the LevelBoundary component
        [Header("References")]
        [SerializeField] private LevelBoundary levelBoundary = null;
        public LevelBoundary LevelBoundary
        {
            get
            {
                if (this.levelBoundary == null)
                    this.levelBoundary = this.GetComponentInParent<LevelBoundary>();
                return this.levelBoundary;
            }
        }

        // Define the fit zone (the area where the boundary should stay within)
        [Header("Fit Zone")]
        public Vector3 fitZoneSize = new Vector3(100f, 30f, 100f);
        public Vector3 fitZoneCenter = Vector3.zero;

        // Starting conditions for the boundary
        [Header("Starting conditions")]
        public bool retrieveInitialSizeFromBoundary = true;
        public Vector3 initialSize = new Vector3(100f, 30f, 100f);
        public bool retrieveInitialCenterFromBoundary = true;
        public Vector3 initialCenter = Vector3.zero;
        private Vector3 currentSize;
        private Vector3 currentCenter;

        // Configuration for resizing the boundary
        [Header("Resize configuration")]
        [Tooltip("% of scaling down the size of the boundary")]
        [Range(0f, 1f)]
        public float resizeScaleFactor = 0.7f;
        [Tooltip("We should resize EVERY [...] seconds")]
        public float resizeInterval = 10f;
        [Tooltip("We should resize FOR / DURING [...] seconds")]
        public float resizeDuration = 5f;
        public int maxResizes = 5;
        private int resizeCount = 0;

        // Events triggered during the battle royale shrinking process
        [Header("Events")]
        public UnityEvent OnBattleRoyaleMaxResizeReached;
        public UnityEvent OnStartResizing;
        public UnityEvent OnDoneResizing;

        // Initialize the boundary and start the resizing process
        private void Start()
        {
            InitializeBoundary();
            StartCoroutine(ResizeLoop());
        }

        // Set up the initial boundary conditions
        private void InitializeBoundary()
        {
            if (this.LevelBoundary != null)
            {
                if (retrieveInitialSizeFromBoundary)
                    initialSize = this.LevelBoundary.boundarySize;
                if (retrieveInitialCenterFromBoundary)
                    initialCenter = this.LevelBoundary.transform.position;

                currentSize = initialSize;
                currentCenter = initialCenter;

                this.LevelBoundary.boundarySize = currentSize;
                this.LevelBoundary.transform.position = currentCenter;
            }
        }

        // Update the LevelBoundary position and size every frame
        private void Update()
        {
            if (this.LevelBoundary != null)
            {
                this.LevelBoundary.transform.position = currentCenter;
                this.LevelBoundary.boundarySize = currentSize;
            }
        }

        // Coroutine to handle the resizing loop
        private IEnumerator ResizeLoop()
        {
            while (resizeCount < maxResizes)
            {
                // Wait a bit
                yield return new WaitForSeconds(resizeInterval);
                
                // Log of new resize process starting
                Debug.Log("BattleRoyaleBoundary.ResizeLoop() - Start resize #" + resizeCount.ToString());

                // Callback - Start Resize
                OnStartResizing?.Invoke();

                // Select new size/center
                Vector3 newSize = Vector3.Max(currentSize * resizeScaleFactor, fitZoneSize * 0.1f);
                Vector3 newCenter = ChooseNewCenter(newSize);

                // Resize
                yield return StartCoroutine(Resize(newCenter, newSize, resizeDuration));

                // Callback - End Resize
                OnDoneResizing?.Invoke();

                // Save new state
                currentSize = newSize;
                currentCenter = newCenter;
                resizeCount++;
            }

            // Callback that the "game" is done
            OnBattleRoyaleMaxResizeReached?.Invoke();
        }

        // Choose a new center for the boundary within the fit zone
        private Vector3 ChooseNewCenter(Vector3 newSize)
        {
            Vector3 availableSpace = fitZoneSize - newSize;
            Vector3 randomOffset = new Vector3
            (
                Random.Range(-availableSpace.x, availableSpace.x),
                0,
                Random.Range(-availableSpace.z, availableSpace.z)
            ) * 0.5f;

            Vector3 newCenter = fitZoneCenter + randomOffset;
            newCenter = ClampToFitZone(newCenter, newSize);

            return Vector3.Lerp(currentCenter, newCenter, 0.35f);
        }

        // Ensure the given position stays within the fit zone
        private Vector3 ClampToFitZone(Vector3 position, Vector3 size = default)
        {
            Vector3 halfSize = size == default ? currentSize * 0.5f : size * 0.5f;
            Vector3 min = fitZoneCenter - (fitZoneSize * 0.5f) + halfSize;
            Vector3 max = fitZoneCenter + (fitZoneSize * 0.5f) - halfSize;

            return new Vector3
            (
                Mathf.Clamp(position.x, min.x, max.x),
                position.y,
                Mathf.Clamp(position.z, min.z, max.z)
            );
        }

        // Coroutine to handle the resizing process
        private IEnumerator Resize(Vector3 newCenter, Vector3 newSize, float duration)
        {
            Vector3 initialSize = currentSize;
            Vector3 initialPosition = currentCenter;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                currentSize = Vector3.Lerp(initialSize, newSize, t);
                currentCenter = Vector3.Lerp(initialPosition, newCenter, t);

                yield return null;
            }

            currentSize = newSize;
            currentCenter = newCenter;
        }

#if UNITY_EDITOR
        // Draw gizmos in the Unity editor to visualize the fit zone
        private void OnDrawGizmos()
        {
            // Draw the fit zone
            {
                Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); // Semi-transparent yellow
                Gizmos.DrawCube(fitZoneCenter, fitZoneSize);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(fitZoneCenter, fitZoneSize);
            }
        }
#endif
    }
}