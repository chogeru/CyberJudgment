using System.Collections.Generic;
using UnityEngine;
using static NoMercyStudios.BoundariesPro.LevelBoundary;

namespace NoMercyStudios.BoundariesPro
{
    [ExecuteAlways]
    //[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]          // TODO: Uncomment ?
    public class LevelBoundaryChaperone : MonoBehaviour
    {
        // LevelBoundary
        private LevelBoundary levelBoundary = null;
        public LevelBoundary LevelBoundary
        {
            get
            {
                if (this.levelBoundary == null)
                    this.levelBoundary = this.gameObject.GetComponentInParent<LevelBoundary>();
                return this.levelBoundary;
            }
        }

        // LevelBoundary - Selected Shape
        [HideInInspector]/*[SerializeField]*/ private LevelBoundaryShape currentShape = LevelBoundaryShape.Custom;
        [HideInInspector]/*[SerializeField]*/ private Mesh currentBoundaryCustomMesh = null;
        [HideInInspector]/*[SerializeField]*/ private bool autoRefreshCurrentShape = true;

        // Mesh filter
        private MeshFilter boundaryMeshFilter = null;
        public MeshFilter BoundaryMeshFilter
        {
            get
            {
                if (this.boundaryMeshFilter == null)
                    this.boundaryMeshFilter = this.GetComponent<MeshFilter>();
                //if (this.boundaryMeshFilter == null)
                //    this.boundaryMeshFilter = this.AddComponent<MeshFilter>();
                return this.boundaryMeshFilter;
            }
        }

        // Mesh renderer
        private MeshRenderer boundaryRenderer = null;
        public MeshRenderer BoundaryRenderer
        {
            get
            {
                if (this.boundaryRenderer == null)
                    this.boundaryRenderer = this.GetComponent<MeshRenderer>();
                //if (this.boundaryRenderer == null)
                //    this.boundaryRenderer = this.AddComponent<MeshRenderer>();
                return this.boundaryRenderer;
            }
        }

        // Mesh renderer material
        public Material BoundaryRendererMaterial
        {
            get
            {
                if (this.BoundaryRenderer != null)
                    return this.BoundaryRenderer.sharedMaterial;
                return null;
            }
        }

        [Header("Chaperone - Select Material (optional)")]
        [SerializeField] private Material chaperoneMaterial = null;
        public Material ChaperoneMaterial
        {
            get
            {
                if (this.chaperoneMaterial == null)
                    this.chaperoneMaterial = Resources.Load<Material>("Chaperone/Materials/ChaperoneMaterial");
                return this.chaperoneMaterial;
            }
        }

        [Header("Chaperone - Autotiling")]
        public float tilingFactor = 0.025f;
        private float appliedTilingFactor = 0.025f;

        [Header("Chaperone - Sphere Masking")]
        public bool drawSphereMaskingGizmo = false;
        public float sphereMaskingRange = 100f;

        [Header("Chaperone - Collision")]
        public bool autoAddCollider = false;
        public bool autoAddMeshCollider = false;
        private void Start()
        {
            // If there's no boundary renderer attached, we'll log a warning and disable the script.
            if (this.BoundaryRenderer == null)
            {
                Debug.LogWarning("Chaperone: No MeshRenderer found on this GameObject.");
                this.enabled = false;
                return;
            }

            // Check current shape is different,
            if (this.LevelBoundary != null)
            {
                if (this.autoRefreshCurrentShape == true)
                {
                    // Update mesh depending on shape
                    if (this.LevelBoundary.shapeType != this.currentShape)
                        UpdateMeshShape(this.LevelBoundary.shapeType);

                    // Deal with custom mesh & different mesh
                    if (this.BoundaryMeshFilter != null)
                    {
                        if (this.LevelBoundary.shapeType == LevelBoundaryShape.Custom && this.LevelBoundary.boundaryCustomMesh != this.BoundaryMeshFilter.sharedMesh)
                            UpdateMeshShape(this.LevelBoundary.shapeType);
                    }
                }
            }

            // Apply material
            if (this.BoundaryRenderer != null && this.BoundaryRenderer.sharedMaterial == null)
                this.BoundaryRenderer.sharedMaterial = Instantiate(this.ChaperoneMaterial);

            //// Note: do this at runtime only
            if (Application.isPlaying == true)
            {
                // Add collider ?
                if (this.autoAddCollider == true)
                {
                    // Retrive mesh from meshfilter
                    Mesh colliderMesh = MeshUtils.DuplicateMesh(this.BoundaryMeshFilter.sharedMesh);

                    // Name it
                    colliderMesh.name = colliderMesh.name + "_collider";

                    // Flip its normal
                    MeshUtils.FlipNormals(colliderMesh);
                    if (autoAddMeshCollider == true)
                    {
                        // Apply it to the new mesh collider
                        MeshCollider meshCollider = this.gameObject.GetComponent<MeshCollider>();
                        if (meshCollider == null)
                            meshCollider = this.gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = colliderMesh;
                    }
                }
            }
        }

        private void Update()
        {
            // Retrieve informations from the level boundary we depend on
            if (this.LevelBoundary != null)
            {
                // Check current shape is different,
                if (this.autoRefreshCurrentShape == true)
                {
                    // Update mesh depending on shape
                    if (this.LevelBoundary.shapeType != this.currentShape)
                        UpdateMeshShape(this.LevelBoundary.shapeType);

                    // Deal with custom mesh & different mesh
                    if (this.BoundaryMeshFilter != null)
                    {
                        if (this.LevelBoundary.shapeType == LevelBoundaryShape.Custom && this.LevelBoundary.boundaryCustomMesh != this.currentBoundaryCustomMesh)
                            UpdateMeshShape(this.LevelBoundary.shapeType);
                    }
                }

                // Check parent
                if (this.transform.parent != this.LevelBoundary.transform)
                    this.LevelBoundary.transform.parent = this.LevelBoundary.transform;

                // Update pos/rot
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;

                // Update scale if necessary
                if (this.transform.localScale != this.LevelBoundary.boundarySize)
                {
                    // Scale
                    this.transform.localScale = this.LevelBoundary.boundarySize;
                }
            }

            // Autoset material
            if (this.BoundaryRendererMaterial == null)
            {
                if (this.BoundaryRenderer != null)
                    this.BoundaryRenderer.sharedMaterial = Instantiate(this.ChaperoneMaterial);
            }

            // Auto-Tiling
            if (this.BoundaryRendererMaterial != null)
                this.BoundaryRendererMaterial.SetTextureScale("_MainTex", this.tilingFactor * this.transform.localScale);

            // Update mask radius (this portion can also be directly be manipulated in the materials properties)
            if (this.BoundaryRendererMaterial != null)
                this.BoundaryRendererMaterial.SetFloat("_MaskRadius", this.sphereMaskingRange);

            // Update mask centers
            {
                // Variable 
                int maskCentersCount = 0;
                Vector4[] maskCenters = new Vector4[32];

                // Store spherical mask centers
                if (LevelBoundariesManager.Instance != null)
                {
                    List<LevelBoundaryTrackedObject> trackedObjects = LevelBoundariesManager.Instance.TrackedObjects;
                    if (trackedObjects != null && trackedObjects.Count > 0)
                    {
                        for (int i = 0; i < trackedObjects.Count; i++)
                        {
                            if (trackedObjects[i] != null && trackedObjects[i].UseForChaperone == true)
                            {
                                // Handle max quantity
                                //// Note: you can change that in the shaders property if needed though
                                if (maskCentersCount < 32)
                                {
                                    maskCenters[maskCentersCount] = trackedObjects[i].transform.position;
                                    maskCentersCount++;
                                }
                                //else
                                //{
                                //    // Log
                                //    Debug.LogError("Chaperone() - Error: Too many mask centers! Shader supports up to 32.");
                                //}
                            }
                        }
                    }
                }

                // Update shader property
                this.BoundaryRendererMaterial.SetInt("_MaskCentersCount", maskCentersCount);
                if (maskCentersCount > 0)
                    this.BoundaryRendererMaterial.SetVectorArray("_MaskCentersArray", maskCenters);
            }

            //#if UNITY_EDITOR
            //if (Application.isPlaying == false)
            {
                // Update tiling
                if (this.appliedTilingFactor != this.tilingFactor)
                {
                    // Tiling
                    if (this.BoundaryRendererMaterial != null)
                        this.BoundaryRendererMaterial.SetTextureScale("_MainTex", this.tilingFactor * this.transform.localScale);

                    // Change
                    this.appliedTilingFactor = this.tilingFactor;
                }
            }
            //#endif
        }

        public void UpdateMeshShape(LevelBoundaryShape shape)
        {
            if (this.BoundaryMeshFilter != null)
            {
                // Clear collider (if current shape is custom?)
                if (this.currentShape == LevelBoundaryShape.Custom)
                {
                    //   MeshCollider meshFilterCollider = this.gameObject.GetComponent<MeshCollider>();
                    //  GameObject.DestroyImmediate(meshFilterCollider);

                    //// TODO: Clear mesh shape?
                    //if (this.BoundaryMeshFilter.sharedMesh != null)
                    //    UnityEngine.Object.DestroyImmediate(this.BoundaryMeshFilter.sharedMesh);
                }

                // Create mesh depending on shape
                switch (shape)
                {
                    case LevelBoundaryShape.Cube:
                        GeneratePrimitiveMesh(PrimitiveType.Cube);
                        break;

                    case LevelBoundaryShape.Sphere:
                        GeneratePrimitiveMesh(PrimitiveType.Sphere);
                        break;

                    case LevelBoundaryShape.Hemisphere:
                        GenerateHalfSphereMesh(0.5f, 20, 20);  // radius, widthSegments, heightSegments
                        break;

                    case LevelBoundaryShape.Cylinder:
                        GeneratePrimitiveMesh(PrimitiveType.Cylinder);

                        // Scale cylinder mesh by half
                        {
                            // Retrieve source
                            Mesh cylinderMesh = this.BoundaryMeshFilter.sharedMesh;

                            // Clone the mesh to avoid modifying the Unity's internal mesh.
                            Mesh meshCopy = UnityEngine.Object.Instantiate(cylinderMesh);

                            // Scale down the Y coordinates of the vertices.
                            Vector3[] vertices = meshCopy.vertices;
                            for (int i = 0; i < vertices.Length; i++)
                            {
                                vertices[i].y *= 0.5f;
                            }

                            // Scale the UVs
                            Vector2[] uvs = meshCopy.uv;
                            for (int i = 0; i < uvs.Length; i++)
                            {
                                uvs[i] = new Vector2(uvs[i].x, uvs[i].y * 0.5f);
                            }

                            // Assign the modified vertices back and recalculate the bounds of the mesh.
                            meshCopy.vertices = vertices;
                            meshCopy.uv = uvs;
                            meshCopy.RecalculateBounds();

                            // Assign the modified mesh to the target mesh filter.
                            BoundaryMeshFilter.sharedMesh = meshCopy;
                        }
                        break;

                    case LevelBoundaryShape.Custom:
                        if (this.BoundaryMeshFilter != null)
                        {
                            // No mesh yet, can't consider we actually switched
                            if (this.LevelBoundary.boundaryCustomMesh == null)
                                return;

                            // Generate a mesh "copy" from our custom mesh
                            this.BoundaryMeshFilter.sharedMesh = UnityEngine.Object.Instantiate(this.LevelBoundary.boundaryCustomMesh);
                        }
                        break;
                }
            }

            // Register
            this.currentShape = shape;
            switch (this.currentShape)
            {
                case LevelBoundaryShape.Custom:
                    this.currentBoundaryCustomMesh = this.LevelBoundary.boundaryCustomMesh;
                    break;

                default:
                    this.currentBoundaryCustomMesh = null;
                    break;
            }
        }

        private void GeneratePrimitiveMesh(PrimitiveType type)
        {
            if (this.BoundaryMeshFilter != null)
            {
                GameObject primitiveObject = GameObject.CreatePrimitive(type);
                if (primitiveObject != null)
                {
                    MeshFilter primitiveObjectMeshFilter = primitiveObject.GetComponent<MeshFilter>();
                    if (primitiveObjectMeshFilter != null && primitiveObjectMeshFilter.sharedMesh != null)
                    {
                        this.BoundaryMeshFilter.sharedMesh = primitiveObjectMeshFilter.sharedMesh;
                    }
                    DestroyImmediate(primitiveObject);
                }
            }
        }

        private void GenerateHalfSphereMesh(float radius, int widthSegments, int heightSegments)
        {
            if (this.BoundaryMeshFilter != null)
            {
                HemisphereGeneratorInfos hemisphereBuilderInfos = new HemisphereGeneratorInfos()
                {
                    fullUvRange = true,
                    hemisphere = true,
                    radius = radius,
                };

                // Generate
                HemisphereGenerator.GenerateHemisphere(this.BoundaryMeshFilter, hemisphereBuilderInfos);
            }
        }

    }
}
