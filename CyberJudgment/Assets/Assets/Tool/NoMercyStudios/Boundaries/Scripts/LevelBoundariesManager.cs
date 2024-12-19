using System.Collections.Generic;
using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public class LevelBoundariesManager : MonoBehaviour
    {
        // Singleton
        private static LevelBoundariesManager _instance;
        public static LevelBoundariesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LevelBoundariesManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LevelBoundaryManager");
                        _instance = go.AddComponent<LevelBoundariesManager>();
                    }
                }
                return _instance;
            }
        }


        // Initialization
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        [Header("Boundaries")]
        [SerializeField] private List<LevelBoundary> _boundaries = new List<LevelBoundary>();
        public List<LevelBoundary> Boundaries
        {
            get
            {
                return this._boundaries;
            }
        }

        // Boundaries - Register/Unregister
        public void RegisterBoundary(LevelBoundary boundary)
        {
            if (_boundaries != null && _boundaries.Contains(boundary) == false)
                _boundaries.Add(boundary);
        }

        public void UnregisterBoundary(LevelBoundary boundary)
        {
            if (_boundaries != null && _boundaries.Contains(boundary) == true)
                _boundaries.Remove(boundary);
        }



        [Header("Tracked Objects")]
        [SerializeField] private List<LevelBoundaryTrackedObject> trackedObjects = new List<LevelBoundaryTrackedObject>();
        public List<LevelBoundaryTrackedObject> TrackedObjects
        {
            get
            {
                return trackedObjects;
            }
        }

        // Tracked Objects - Registration
        public void RegisterTrackedObject(LevelBoundaryTrackedObject trackedObject)
        {
            // Init list
            if (this.trackedObjects == null)
                this.trackedObjects = new List<LevelBoundaryTrackedObject>();

            // Check if input isn't in lit
            if (this.trackedObjects.Contains(trackedObject) == false)
            {
                // Add to list
                this.trackedObjects.Add(trackedObject);
            }
        }

        public void UnregisterTrackedObject(LevelBoundaryTrackedObject trackedObject)
        {
            // Init list
            if (this.trackedObjects == null)
                this.trackedObjects = new List<LevelBoundaryTrackedObject>();

            // Check if input is in lit
            if (this.trackedObjects.Contains(trackedObject) == true)
            {
                // Remove from list
                this.trackedObjects.Remove(trackedObject);
            }
        }

        // Tracked Objects - Boundaries - Checks
        public bool IsInsideAnyBoundary(LevelBoundaryTrackedObject trackedObject)
        {
            if (trackedObject != null)
            {
                foreach (LevelBoundary boundary in _boundaries)
                {
                    if (boundary.IsInsideBoundaries(trackedObject) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}