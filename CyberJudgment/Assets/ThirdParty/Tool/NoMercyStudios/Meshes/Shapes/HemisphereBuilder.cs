using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    public class HemisphereGeneratorInfos
    {
        // Radius
        public float radius = 1f;
        // Smoothness factor (1 to 3 potential value)
        public int meshSmoothness = 1;
        // Create north hemisphere only
        public bool hemisphere = false;
        // UV covers full sphere
        public bool fullUvRange = true;
    }

    // Fix of a code i found by user "Fergicide" at: https://forum.unity.com/threads/script-to-create-a-hemisphere-mesh.496720/
    //// Note: fixing the generation of the "base" set of vertices not yet done.
    public static class HemisphereGenerator
    {
        // Note: for the moment, i replaced all (nbLat / 2) by (nbLat / 2 + 1)
        //// This is a very bad fix, but it's late & i prefer that it's easily droppable on a ground.
        public static void GenerateHemisphere(MeshFilter mf, HemisphereGeneratorInfos hemisphereGeneratorInfos)
        {
            if (mf == null)  // Check if the MeshFilter (mf) is not null.
                return;  // If it is null, exit the method early.

            Mesh mesh = new Mesh();  // Create a new mesh.
            mesh.Clear();  // Clear any existing data in the mesh.

            // Set the number of longitude and latitude lines.
            int nbLong = 24 * hemisphereGeneratorInfos.meshSmoothness;  // Longitude lines (vertical)
            int nbLat = 16 * hemisphereGeneratorInfos.meshSmoothness;   // Latitude lines (horizontal)

            #region Vertices
            // Determine the number of vertices and create a vertex array.
            Vector3[] vertices = new Vector3[(nbLong + 1) * ((hemisphereGeneratorInfos.hemisphere) ? (nbLat / 2 + 1) : nbLat) + 2];
            float _pi = Mathf.PI;  // Define Pi.
            float _2pi = _pi * 2f; // Define 2*Pi.

            vertices[0] = Vector3.up * hemisphereGeneratorInfos.radius;  // Set the top vertex.

            // Loop through each latitude and longitude to create vertices.
            for (int lat = 0; lat < ((hemisphereGeneratorInfos.hemisphere) ? (nbLat / 2 + 1) : nbLat); lat++)
            {
                // Calculate the angle, sine, and cosine for the latitude.
                float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                // Hemisphere & Last lat
                bool isLastLatSpecial = (hemisphereGeneratorInfos.hemisphere == true && (lat == (nbLat / 2)));

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    // Calculate the angle, sine, and cosine for the longitude.
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    // Hemisphere: Force minY to 0
                    if (hemisphereGeneratorInfos.hemisphere == true && cos1 < 0f)
                    {
                        // Calculate and assign the vertex position in the sphere.
                        vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, 0f, sin1 * sin2) * hemisphereGeneratorInfos.radius;
                    }
                    // Default behaviour
                    else
                    {
                        // Calculate and assign the vertex position in the sphere.
                        vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * hemisphereGeneratorInfos.radius;
                    }
                }
            }
            vertices[vertices.Length - 1] = -1f * Vector3.up * hemisphereGeneratorInfos.radius;  // Set the bottom vertex.
            #endregion

            #region Normals
            // Create an array for normals and assign them based on vertices.
            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normals[n] = vertices[n].normalized;  // Normalize the vertex to get the normal.
            #endregion

            #region UVs
            // Create an array for UVs and calculate their values.
            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < ((hemisphereGeneratorInfos.hemisphere) ? (nbLat / 2 + 1) : nbLat); lat++)
            {
                // Hemisphere & Last lat
                bool isLastLatSpecial = (hemisphereGeneratorInfos.hemisphere == true && (lat == (nbLat / 2)));

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    // Hemisphere
                    if (isLastLatSpecial == true)
                    {
                        uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, (float)lat / nbLat);
                    }
                    else
                    {
                        if (!hemisphereGeneratorInfos.hemisphere || hemisphereGeneratorInfos.fullUvRange)
                            uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
                        else
                            uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / ((nbLat / 2)));
                    }
                }
            }
            #endregion

            #region Triangles
            // Determine the number of triangles and create an array for triangle indices.
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            int i = 0;  // Index counter for the triangle array.

            // Create triangles for the top cap.
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            // Create triangles for the middle part.
            for (int lat = 0; lat < ((hemisphereGeneratorInfos.hemisphere) ? (nbLat / 2 + 1) : nbLat) - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }
            }

            // If it’s not a hemisphere, create triangles for the bottom cap.
            if (!hemisphereGeneratorInfos.hemisphere)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    triangles[i++] = vertices.Length - 1;
                    triangles[i++] = vertices.Length - (lon + 2) - 1;
                    triangles[i++] = vertices.Length - (lon + 1) - 1;
                }
            }
            #endregion

            // Assign the vertices, normals, UVs, and triangles to the mesh.
            mesh.name = "Hemisphere";
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            // Recalculate the bounds of the mesh.
            mesh.RecalculateBounds();

            // Assign the mesh to the MeshFilter.
            mf.mesh = mesh;
        }
    }
}