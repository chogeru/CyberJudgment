using NoMercyStudios.BoundariesPro;
using Parabox.CSG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public class MergedLevelBoundariesChaperone : MonoBehaviour
    {
        [Header("Merged Chaperone - Generation")]
        [SerializeField] private bool useAllBoundaries = false;
        [SerializeField] private List<LevelBoundary> _boundaries = new List<LevelBoundary>();
        public List<LevelBoundary> Boundaries
        {
            get
            {
                if (this.useAllBoundaries == true)
                {
                    if (LevelBoundariesManager.Instance != null)
                        return LevelBoundariesManager.Instance.Boundaries;
                }
                return this._boundaries;
            }
        }


        [Header("Merged Chaperone - Settings")]

        [SerializeField] private Material chaperoneMaterial = null;
        public Material ChaperoneMaterial
        {
            get
            {
                if (this.chaperoneMaterial == null)
                {
                    this.chaperoneMaterial = Resources.Load<Material>("Chaperone/Materials/ChaperoneMaterial");
                }
                //if (this.BoundaryRenderer != null)
                //    return this.BoundaryRenderer.sharedMaterial;
                return this.chaperoneMaterial;
            }
        }

        [Header("Merged Chaperone - Autotiling")]
        //private float appliedTilingFactor = 0.025f;
        public float tilingFactor = 0.025f;

        [Header("Merged Chaperone - Sphere Masking")]
        public bool drawSphereMaskingGizmo = false;
        public float sphereMaskingRange = 100f;

        [Header("Merged Chaperone - Collision")]
        public bool autoAddCollider = false;

        // Handle Generation - Raise flag when done
        private bool refreshChaperone = true;
        private void Update()
        {
            if (this.refreshChaperone == true)
            {
                StartCoroutine(this.RefreshChaperoneCoroutine());
                refreshChaperone = false;
            }
        }

        [ContextMenu("Refresh Chaperone")]
        public void TryRefreshChaperone()
        {
            this.refreshChaperone = true;
        }

        private IEnumerator RefreshChaperoneCoroutine()
        {
            // Yield
            //yield return null;
            yield return new WaitForEndOfFrame();

            // Log
            UnityEngine.Debug.LogWarning("InitializeIntersections");

            // Keep list
            List<LevelBoundary> boundaries = new List<LevelBoundary>(this.Boundaries);

            // GameObject
            GameObject unionModelGameObject = UnionOfAll(boundaries);
            {
                // If not null, add Chaperone
                if (unionModelGameObject != null)
                {
                    // Parent
                    unionModelGameObject.transform.parent = this.transform;

                    // Add chaperone
                    LevelBoundaryChaperone chaperone = unionModelGameObject.gameObject.AddComponent<LevelBoundaryChaperone>();
                    {
                        chaperone.tilingFactor = this.tilingFactor;
                        chaperone.sphereMaskingRange = this.sphereMaskingRange;
                        chaperone.drawSphereMaskingGizmo = this.drawSphereMaskingGizmo;
                    }

                    // Mesh filter
                    MeshFilter meshFilter = unionModelGameObject.GetComponent<MeshFilter>();

                    // Clear materials
                    MeshRenderer meshRenderer = unionModelGameObject.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        for (int i = 0; i < meshRenderer.materials.Length; i++)
                            meshRenderer.sharedMaterials[i] = this.ChaperoneMaterial;
                    }

                    // Add collider ?
                    if (this.autoAddCollider == true)
                    {
                        // Retrive mesh from meshfilter
                        Mesh colliderMesh = MeshUtils.DuplicateMesh(meshFilter.sharedMesh);

                        // Name it
                        colliderMesh.name = colliderMesh.name + "_collider";

                        // Flip its normal
                        MeshUtils.FlipNormals(colliderMesh);

                        // Apply it to the new mesh collider
                        MeshCollider meshCollider = unionModelGameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = colliderMesh;
                    }
                }
            }

            // Hide all boundaries
            if (boundaries != null && boundaries.Count > 0)
            {
                for (int i = 0; i < boundaries.Count; i++)
                {
                    if (boundaries[i] != null)
                    {
                        LevelBoundaryChaperone levelBoundaryChaperone = boundaries[i].GetComponentInChildren<LevelBoundaryChaperone>();
                        if (levelBoundaryChaperone != null)
                        {
                            levelBoundaryChaperone.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        // TODO: Need to add a UnionOfAll(List<LevelBoundaryChaperone> boundariesChaperones) that will be called by this function
        public GameObject UnionOfAll(List<LevelBoundary> boundaries)
        {
            // GameObject
            GameObject unionModelGameObject = null;

            // Go through boundaries
            if (boundaries != null && boundaries.Count > 0)
            {
                // Go through boundaries
                for (int i = 0; i < boundaries.Count; i++)
                {
                    // Nullcheck
                    if (boundaries[i] != null)
                    {
                        // Ignore specific shapes (which can trigger stack overflow in pb_CSG library)
                        switch (boundaries[i].shapeType)
                        {
                            // For now we have issues with the Union algorithm
                            case LevelBoundary.LevelBoundaryShape.Sphere:
                            case LevelBoundary.LevelBoundaryShape.Cylinder:
                            case LevelBoundary.LevelBoundaryShape.Hemisphere:
                            case LevelBoundary.LevelBoundaryShape.Custom:
                                continue;
                                //break;
                        }

                        // Retrieve chaperone component
                        LevelBoundaryChaperone levelBoundaryChaperone = boundaries[i].GetComponentInChildren<LevelBoundaryChaperone>();
                        if (levelBoundaryChaperone != null && levelBoundaryChaperone.BoundaryMeshFilter != null && levelBoundaryChaperone.BoundaryMeshFilter.mesh != null)
                        {
                            // if null, init
                            if (unionModelGameObject == null)
                            {
                                //// Log
                                //Debug.Log("Init with mesh: " + levelBoundaryChaperone.LevelBoundary.gameObject.name);

                                // Create a gameObject to render the result
                                unionModelGameObject = new GameObject(this.gameObject.name + "_Chaperone");
                                {
                                    // Mesh filter
                                    MeshFilter meshFilter = unionModelGameObject.AddComponent<MeshFilter>();
                                    meshFilter.sharedMesh = levelBoundaryChaperone.BoundaryMeshFilter.mesh;

                                    // Renderer
                                    MeshRenderer meshRenderer = unionModelGameObject.AddComponent<MeshRenderer>();
                                    meshRenderer.materials = levelBoundaryChaperone.BoundaryRenderer.materials.ToArray();
                                }
                            }

                            // Union of all the previous meshes
                            {
                                // Log
                                Debug.Log("Union #" + i.ToString() + " with mesh: " + levelBoundaryChaperone.LevelBoundary.gameObject.name);

                                // Mesh name
                                string meshName = "go_mesh_union_" + i.ToString();

                                // Perform boolean operation
                                Model result = CSG.Union(unionModelGameObject, levelBoundaryChaperone.gameObject);

                                // Name it
                                result.mesh.name = meshName;

                                ////// Destroy current
                                GameObject.DestroyImmediate(unionModelGameObject);
                                //GameObject objectToDestroy = unionModelGameObject;

                                // Create a gameObject to render the result
                                unionModelGameObject = new GameObject(meshName);
                                {
                                    // Mesh filter
                                    MeshFilter meshFilter = unionModelGameObject.GetComponent<MeshFilter>();
                                    if (meshFilter == null)
                                        meshFilter = unionModelGameObject.AddComponent<MeshFilter>();
                                    meshFilter.sharedMesh = result.mesh;

                                    // Merge resulting mesh material
                                    MeshUtils.MergeMeshMaterials(meshFilter);

                                    // Renderer
                                    MeshRenderer meshRenderer = unionModelGameObject.GetComponent<MeshRenderer>();
                                    if (meshRenderer == null)
                                        meshRenderer = unionModelGameObject.AddComponent<MeshRenderer>();
                                    meshRenderer.sharedMaterial = this.ChaperoneMaterial;

                                    // Parent
                                    unionModelGameObject.transform.parent = this.transform.parent;
                                }

                                //// Destroy previous
                                //GameObject.DestroyImmediate(objectToDestroy.gameObject);
                            }
                        }
                    }
                }
            }

            // Return
            return unionModelGameObject;
        }
    }
}