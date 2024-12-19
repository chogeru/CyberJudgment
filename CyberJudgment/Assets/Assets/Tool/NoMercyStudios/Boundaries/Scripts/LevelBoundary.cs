using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;

namespace NoMercyStudios.BoundariesPro
{
    public class LevelBoundary : MonoBehaviour
    {
        // Enum to determine the type of boundary shape.
        public enum LevelBoundaryShape { Cube, Sphere, Hemisphere, Cylinder, Custom }       // Those are on the way for next versions

        [Header("Boundaries - Shape")]
        public LevelBoundaryShape shapeType = LevelBoundaryShape.Cube; // Choose the shape type via Inspector.
        public Vector3 boundarySize = new Vector3(500f, 500f, 500f);

        // Boundaries Size - Getter
        public Vector3 BoxedBoundaryCenter
        {
            get
            {
                switch (this.shapeType)
                {
                    case LevelBoundaryShape.Hemisphere:
                        float upVectorOffset = (0.25f * this.boundarySize.y);
                        return this.transform.position + (upVectorOffset * this.transform.up);

                    default:
                        return this.transform.position;
                }
            }
        }
        public Vector3 BoxedBoundarySize
        {
            get
            {
                switch (this.shapeType)
                {
                    case LevelBoundaryShape.Hemisphere:
                        return new Vector3(this.boundarySize.x, 0.5f * this.boundarySize.y, this.boundarySize.z);

                    default:
                        return this.boundarySize;
                }
            }
        }

        private bool TryUpdateBounds(out Bounds localBounds)
        {
            // Init out values
            localBounds = new Bounds();

            // Try Update
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                localBounds = TransformBoundsToWorldSpace(meshRenderer.bounds, meshRenderer.transform);

                // Success
                return true;
            }

            // Fail
            return false;
        }

        private Bounds TransformBoundsToWorldSpace(Bounds localBounds, Transform transform)
        {
            // Create a new bounds object in world space
            Bounds worldBounds = new Bounds();

            // Transform the bounds center to world space
            worldBounds.center = transform.TransformPoint(localBounds.center);

            // Calculate the axis-aligned size in world space
            Vector3 extents = localBounds.extents;
            worldBounds.extents = new Vector3(
                Mathf.Abs(transform.lossyScale.x) * extents.x,
                Mathf.Abs(transform.lossyScale.y) * extents.y,
                Mathf.Abs(transform.lossyScale.z) * extents.z
            );

            return worldBounds;
        }


        [Header("Boundary - Custom Shape")]
        public Mesh boundaryCustomMesh = null;

        // Register/Unregister
        private void OnEnable()
        {
            if (LevelBoundariesManager.Instance != null)
                LevelBoundariesManager.Instance.RegisterBoundary(this);
        }

        private void OnDisable()
        {
            if (LevelBoundariesManager.Instance != null)
                LevelBoundariesManager.Instance.UnregisterBoundary(this);
        }

        // Regularly call
        private void Update()
        {
            CheckBoundary();
        }

        // Update the  boundary status
        private List<LevelBoundaryTrackedObject> trackedObjectsInside = new List<LevelBoundaryTrackedObject>();
        public List<LevelBoundaryTrackedObject> TrackedObjectsInside { get { return this.trackedObjectsInside; } }
        protected virtual void CheckBoundary()
        {
            //// You can override this function or modify it to deal with special cases, for example:
            /////////////////////////////////////////////////////////////////////////////////////////
            //
            // if (isAnyCutscenePlaying == true)
            // {
            //     return;
            // }

            // Update status
            List<LevelBoundaryTrackedObject> trackedObjects = null;
            if (LevelBoundariesManager.Instance != null)
                trackedObjects = LevelBoundariesManager.Instance.TrackedObjects;
            if (trackedObjects != null)
            {
                for (int i = 0; i < trackedObjects.Count; i++)
                {
                    // Retrieve current player boundaries check
                    LevelBoundaryTrackedObject trackedObject = trackedObjects[i];
                    if (trackedObject != null)
                    {
                        // Init list of tracked objects
                        if (this.trackedObjectsInside == null)
                            this.trackedObjectsInside = new List<LevelBoundaryTrackedObject>();

                        // Check
                        bool isInside = this.IsInsideBoundaries(trackedObject);

                        // Depending on check result, add/remove from list
                        if (isInside == true)
                        {
                            if (this.trackedObjectsInside.Contains(trackedObject) == false)
                                this.trackedObjectsInside.Add(trackedObject);
                        }
                        else
                        {
                            if (this.trackedObjectsInside.Contains(trackedObject) == true)
                                this.trackedObjectsInside.Remove(trackedObject);
                        }
                    }
                }
            }
        }

        // Updated method to check if a tracked object is inside the boundary.
        public bool IsInsideBoundaries(LevelBoundaryTrackedObject trackedObject)
        {
            bool isTrackedObjectInside = false;
            if (trackedObject != null)
            {
                Vector3 trackedObjectWorldPosition = trackedObject.transform.position;

                // Check position is inside 
                switch (shapeType)
                {
                    case LevelBoundaryShape.Cube:
                        isTrackedObjectInside = IsPointInsideCube(trackedObjectWorldPosition);
                        break;
                    case LevelBoundaryShape.Sphere:
                        isTrackedObjectInside = IsPointInsideSphere(trackedObjectWorldPosition);
                        break;
                    case LevelBoundaryShape.Cylinder:
                        isTrackedObjectInside = IsPointInsideCylinder(trackedObjectWorldPosition);
                        break;
                    case LevelBoundaryShape.Hemisphere:
                        isTrackedObjectInside = IsPointInsideHalfSphere(trackedObjectWorldPosition);
                        break;
                    case LevelBoundaryShape.Custom:
                        isTrackedObjectInside = IsPointInsideCustomMesh(trackedObjectWorldPosition);
                        break;
                    default:
                        isTrackedObjectInside = false;
                        break;
                }

                // Update list of tracked objects
                {
                    // Init list of tracked objects
                    if (this.trackedObjectsInside == null)
                        this.trackedObjectsInside = new List<LevelBoundaryTrackedObject>();

                    // Depending on check result, add/remove from list
                    if (isTrackedObjectInside == true)
                    {
                        if (this.trackedObjectsInside.Contains(trackedObject) == false)
                            this.trackedObjectsInside.Add(trackedObject);
                    }
                    else
                    {
                        if (this.trackedObjectsInside.Contains(trackedObject) == true)
                            this.trackedObjectsInside.Remove(trackedObject);
                    }
                }
            }

            // Return
            return isTrackedObjectInside;
        }

        // Checks if a point is inside a cube boundary.
        private bool IsPointInsideCube(Vector3 worldPoint)
        {
            return BoundariesUtils.IsPointInsideCube(worldPoint, transform, boundarySize);
        }

        // Checks if a point is inside a spherical boundary.
        private bool IsPointInsideSphere(Vector3 worldPoint)
        {
            return BoundariesUtils.IsPointInsideSphere(worldPoint, transform, boundarySize);
        }

        // Checks if a point is inside a half-spherical boundary.
        private bool IsPointInsideHalfSphere(Vector3 worldPoint)
        {
            return BoundariesUtils.IsPointInsideHalfSphere(worldPoint, transform, boundarySize);
        }

        // Checks if a point is inside a cylinder boundary.
        private bool IsPointInsideCylinder(Vector3 worldPoint)
        {
            return BoundariesUtils.IsPointInsideCylinder(worldPoint, transform, boundarySize);
        }

        // Checks if a point is inside our custom mesh... and our custom mesh for the moment is 
        private LevelBoundaryChaperone boundaryChaperone = null;
        private bool IsPointInsideCustomMesh(Vector3 worldPoint)
        {
            if (this.boundaryChaperone == null)
                this.boundaryChaperone = this.GetComponentInChildren<LevelBoundaryChaperone>();

            if (this.boundaryChaperone != null)
            {
                MeshFilter meshFilter = this.boundaryChaperone.GetComponent<MeshFilter>();
                if (meshFilter != null)
                    return BoundariesUtils.IsPointInsideMesh(worldPoint, meshFilter);
            }

            return false;
        }

#if UNITY_EDITOR
        // Debug
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            switch (shapeType)
            {
                case LevelBoundaryShape.Cube:
                    GizmosExtensions.DrawWireCube(transform.position, boundarySize, transform.rotation);
                    break;
                case LevelBoundaryShape.Sphere:
                    GizmosExtensions.DrawWireSphere(transform.position, boundarySize.x * 0.5f, transform.rotation);
                    break;
                case LevelBoundaryShape.Hemisphere:
                    // Drawing a half-sphere might be a bit tricky with default Gizmos. 
                    // This will require custom Gizmo drawing, and it's outside of the scope of this example.
                    // For now, we'll draw a full sphere for visualization.
                    GizmosExtensions.DrawHalfSphere(transform.position, boundarySize.x * 0.5f, boundarySize.y * 0.5f, 10, 20, transform.rotation);
                    break;
                case LevelBoundaryShape.Cylinder:
                    GizmosExtensions.DrawWireCylinder(transform.position, boundarySize.x * 0.5f, boundarySize.y, transform.rotation);
                    break;
                case LevelBoundaryShape.Custom:
                    // TODO: Deal with bounding box?
                    GizmosExtensions.DrawWireCube(transform.position, boundarySize, transform.rotation);
                    break;
            }

            // Draw "box" around the shape
            Gizmos.color = Color.yellow;
            GizmosExtensions.DrawWireCube(this.BoxedBoundaryCenter, this.BoxedBoundarySize, transform.rotation);
        }
#endif
    }
}