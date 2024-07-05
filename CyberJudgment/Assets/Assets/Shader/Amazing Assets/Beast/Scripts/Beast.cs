using System.Collections.Generic;

using UnityEngine;


namespace AmazingAssets.Beast
{
    static public class Beast
    {
        public static Mesh GenerateSmoothNormals(this Mesh sourceMesh)
        {
            if (sourceMesh == null)
            {
                Debug.LogError("GenerateSmoothNormals: 'sourceMesh' mesh is NULL.\n");
                return null;
            }
            if (sourceMesh.normals == null || sourceMesh.normals.Length != sourceMesh.vertexCount)
            {
                Debug.LogError("GenerateSmoothNormals: 'sourceMesh' mesh has no normals.\n");
                return null;
            }


            Mesh newMesh = UnityEngine.Object.Instantiate(sourceMesh);
            newMesh.name = newMesh.name.Replace("(Clone)", string.Empty);
            newMesh.name += " (Smooth Normals)";

            Vector3[] sourceVertices = sourceMesh.vertices;
            Vector3[] sourceNormals = sourceMesh.normals;

            Dictionary<Vector3, Vector3> smoothNormalsHash = new Dictionary<Vector3, Vector3>();
            for (int i = 0; i < sourceVertices.Length; i++)
            {
                Vector3 key = sourceVertices[i];

                if (smoothNormalsHash.ContainsKey(key))
                {
                    smoothNormalsHash[key] = (smoothNormalsHash[key] + sourceNormals[i]).normalized;
                }
                else
                {
                    smoothNormalsHash.Add(key, sourceNormals[i]);
                }
            }


            List<Vector3> smoothNormals = new List<Vector3>(sourceMesh.normals);
            for (int i = 0; i < sourceVertices.Length; i++)
            {
                smoothNormals[i] = smoothNormalsHash[sourceVertices[i]];
            }

            newMesh.SetUVs(3, smoothNormals);

            return newMesh;
        }
    }
}