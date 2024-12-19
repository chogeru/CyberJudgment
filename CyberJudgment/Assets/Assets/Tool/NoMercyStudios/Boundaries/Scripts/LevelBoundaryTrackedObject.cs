using UnityEngine;
using UnityEngine.Events;

namespace NoMercyStudios.BoundariesPro
{
    [System.Serializable]
    public class LevelBoundaryTrackedObjectEvent : UnityEvent<LevelBoundaryTrackedObject> { }

    public class LevelBoundaryTrackedObject : MonoBehaviour
    {
        [Header("Chaperone")]
        [SerializeField] private bool useForChaperone = false;
        public bool UseForChaperone
        {
            get
            {
                return this.useForChaperone;
            }
        }

        [Header("State")]
        /*[SerializeField]*/
        private bool isInsideBoundaries = true;
        public bool IsInsideBoundaries
        {
            get
            {
                if (LevelBoundariesManager.Instance != null)
                    return this.isInsideBoundaries;
                return true;
            }
            set
            {
                if (this.isInsideBoundaries != value)
                {
                    // Update value
                    this.isInsideBoundaries = value;

                    // Callback
                    if (this.isInsideBoundaries == true)
                    {
                        this.HandleEnterBoundary();
                    }
                    else
                    {
                        this.HandleExitBoundary();
                    }
                }
            }
        }

        [Header("Events")]
        public LevelBoundaryTrackedObjectEvent OnEnterBoundary = new LevelBoundaryTrackedObjectEvent();
        public LevelBoundaryTrackedObjectEvent OnExitBoundary = new LevelBoundaryTrackedObjectEvent();

        private void OnEnable()
        {
            // Register
            LevelBoundariesManager.Instance?.RegisterTrackedObject(this);

            // Init
            if (LevelBoundariesManager.Instance != null)
            {
                // Set var
                this.isInsideBoundaries = LevelBoundariesManager.Instance.IsInsideAnyBoundary(this);

                // Call
                if (this.IsInsideBoundaries == true)
                {
                    this.HandleEnterBoundary();
                }
                else
                {
                    this.HandleExitBoundary();
                }
            }
        }

        private void OnDisable()
        {
            // Register
            LevelBoundariesManager.Instance?.UnregisterTrackedObject(this);
        }

        // Check boundaries
        protected virtual void Update()
        {
            // Update boundaries status
            this.CheckBoundaries();
        }
        protected virtual void CheckBoundaries()
        {
            // Update boundaries status
            if (LevelBoundariesManager.Instance != null)
                this.IsInsideBoundaries = LevelBoundariesManager.Instance.IsInsideAnyBoundary(this);
        }

        // Check boundaries - Events
        protected virtual void HandleEnterBoundary()
        {
            // Log
            Debug.Log("Player entered boundary.");

            // Callback
            if (OnEnterBoundary != null)
                OnEnterBoundary.Invoke(this);
        }

        protected virtual void HandleExitBoundary()
        {
            // Log
            Debug.Log("Player exited boundary.");

            // Callback
            if (OnExitBoundary != null)
                OnExitBoundary.Invoke(this);
        }
    }
}