using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public static class MeshUtils
    {
        public static void MergeMeshMaterials(MeshFilter meshFilter)
        {
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                CombineInstance[] combine = new CombineInstance[mesh.subMeshCount];

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    combine[i].mesh = mesh;
                    combine[i].subMeshIndex = i;
                    combine[i].transform = Matrix4x4.identity;
                }

                meshFilter.mesh = new Mesh();
                meshFilter.mesh.CombineMeshes(combine, true, false);
            }
        }
        public static Mesh DuplicateMesh(Mesh originalMesh)
        {
            if (originalMesh == null)
                return null;

            Mesh duplicatedMesh = new Mesh
            {
                vertices = originalMesh.vertices,
                triangles = originalMesh.triangles,
                uv = originalMesh.uv,
                normals = originalMesh.normals,
                tangents = originalMesh.tangents,
                colors = originalMesh.colors,
                bindposes = originalMesh.bindposes,
                boneWeights = originalMesh.boneWeights
            };

            // Copy submeshes
            duplicatedMesh.subMeshCount = originalMesh.subMeshCount;
            for (int i = 0; i < originalMesh.subMeshCount; i++)
            {
                duplicatedMesh.SetTriangles(originalMesh.GetTriangles(i), i);
            }

            // Optionally: Name the duplicated mesh
            duplicatedMesh.name = originalMesh.name + "_duplicate";

            return duplicatedMesh;
        }

        public static void FlipNormals(Mesh mesh)
        {
            if (mesh != null)
            {
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < normals.Length; i++)
                    normals[i] = -normals[i];
                mesh.normals = normals;

                for (int m = 0; m < mesh.subMeshCount; m++)
                {
                    int[] triangles = mesh.GetTriangles(m);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        int temp = triangles[i + 0];
                        triangles[i + 0] = triangles[i + 1];
                        triangles[i + 1] = temp;
                    }
                    mesh.SetTriangles(triangles, m);
                }
            }
        }
    }
}