using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public static class BoundariesUtils
    {
        // Checks if a point is inside a cube boundary.
        public static bool IsPointInsideCube(Vector3 worldPoint, Transform boundaryTransform, Vector3 boundarySize)
        {
            // Check
            if (boundaryTransform == null)
                return false;

            Vector3 localPoint = boundaryTransform.InverseTransformPoint(worldPoint);
            Vector3 halfBoundarySize = boundarySize / 2;
            return localPoint.x > -halfBoundarySize.x && localPoint.x < halfBoundarySize.x
                && localPoint.y > -halfBoundarySize.y && localPoint.y < halfBoundarySize.y
                && localPoint.z > -halfBoundarySize.z && localPoint.z < halfBoundarySize.z;
        }

        // Checks if a point is inside a spherical boundary.
        public static bool IsPointInsideSphere(Vector3 worldPoint, Transform boundaryTransform, Vector3 boundarySize)
        {
            // Check
            if (boundaryTransform == null)
                return false;

            Vector3 localPoint = boundaryTransform.InverseTransformPoint(worldPoint);
            float radius = boundarySize.x / 2;
            return Vector3.Distance(Vector3.zero, localPoint) <= radius;
        }

        // Checks if a point is inside a half-spherical boundary.
        public static bool IsPointInsideHalfSphere(Vector3 worldPoint, Transform boundaryTransform, Vector3 boundarySize)
        {
            // Check
            if (boundaryTransform == null)
                return false;

            Vector3 localPoint = boundaryTransform.InverseTransformPoint(worldPoint);
            float radius = boundarySize.x / 2;
            bool isInsideSphere = Vector3.Distance(Vector3.zero, localPoint) <= radius;

            // Assuming the half-sphere opens upwards along the local y-axis.
            return isInsideSphere && localPoint.y >= 0;
        }

        // Checks if a point is inside a cylinder boundary.
        public static bool IsPointInsideCylinder(Vector3 worldPoint, Transform boundaryTransform, Vector3 boundarySize)
        {
            // Check
            if (boundaryTransform == null)
                return false;

            Vector3 localPoint = boundaryTransform.InverseTransformPoint(worldPoint);
            float radius = boundarySize.x / 2f;
            float halfHeight = boundarySize.y / 2f;

            // Check Y-coordinate is within height range
            if (localPoint.y < -halfHeight || localPoint.y > halfHeight)
                return false;

            // Check horizontal distance to cylinder axis is within radius
            float distanceToAxis = Vector2.Distance(new Vector2(localPoint.x, localPoint.z), Vector2.zero);

            return distanceToAxis <= radius;
        }
        public static bool IsPointInsideMesh(Vector3 worldPoint, MeshFilter meshFilter)
        {
            // Check meshFilter
            if (meshFilter != null)
            {
                // Check that we have a collider
                MeshCollider meshFilterCollider = meshFilter.GetComponent<MeshCollider>();
                bool meshFilterHadNoCollider = (meshFilterCollider == null || meshFilterCollider.enabled == false);
                if (meshFilterCollider == null)
                {
                    meshFilterCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                    meshFilterCollider.convex = true;
                    meshFilterCollider.isTrigger = true;

                    //// Flip normals of mesh & apply modified mesh
                    ////// Note: if we don't, at least on the models i checked, the physics raycast will never hit
                    //{
                    //    // Retrive mesh from meshfilter
                    //    Mesh colliderMesh = MeshUtils.DuplicateMesh(meshFilter.sharedMesh);

                    //    // Name it
                    //    colliderMesh.name = meshFilter.sharedMesh.name + "_collider";

                    //    // Flip its normal
                    //    MeshUtils.FlipNormals(colliderMesh);

                    //    // Apply it to the new mesh collider
                    //    meshFilterCollider.sharedMesh = colliderMesh;
                    //}
                }

                // Enable collider
                if (meshFilterCollider != null)
                    meshFilterCollider.enabled = true;

                // Raycast hits count registered
                int hitsCount = 0;

                // Perform the raycast with the specific layer mask (if needed).
                // Commented out: RaycastHit[] raycastHits = Physics.RaycastAll(worldPoint, Vector3.up, Mathf.Infinity, meshFilter.gameObject.layer, QueryTriggerInteraction.Collide);
                //// To solve issues with the generated convex mesh, we're going to do it opposite way
                RaycastHit[] raycastHits = Physics.RaycastAll(worldPoint + Vector3.up * 1000f, Vector3.down, 1000f, -1, QueryTriggerInteraction.Collide);
                if (raycastHits != null && raycastHits.Length > 0)
                {
                    // Go through list
                    for (int i=0; i < raycastHits.Length; i++)
                    {
                        //// Log
                        //Debug.Log("hit" + raycastHits[i].collider.gameObject.name);

                        // Check if the hit collider belongs to the mesh filter's game object.
                        MeshFilter otherMeshFilter = raycastHits[i].collider.GetComponent<MeshFilter>();
                        if (otherMeshFilter != null && otherMeshFilter == meshFilter)
                        {

                            // The point is inside the mesh.
                            hitsCount++;
                        }
                    }
                }

                //// Disable collider
                //if (meshFilterHadNoCollider == false)
                //    meshFilterCollider.enabled = false;

                // If the total number of hits is odd, the point is inside a mesh.
                // If the total number of hits is even, the point is outside a mesh.
                return ((hitsCount % 2) == 1);
            }

            // The point is outside the mesh or there's no valid mesh filter.
            return false;
        }
    }
}