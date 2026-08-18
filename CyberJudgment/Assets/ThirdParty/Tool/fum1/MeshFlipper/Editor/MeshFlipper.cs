using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Collections;

public class MeshFlipper : EditorWindow
{
    private Renderer renderer;
    private bool flip = true;
    private bool twoSides = false;
    private bool separate = false;
    private bool merge = true;
    private bool canCreate = true;//複数回生成しないように

    [MenuItem("fum1/Mesh Flipper")]
    static void Init()
    {
        MeshFlipper window = (MeshFlipper)EditorWindow.GetWindow(typeof(MeshFlipper));
        window.Show();
    }

    void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Renderer", GUILayout.Width(75));
            renderer = (Renderer)EditorGUILayout.ObjectField(renderer, typeof(Renderer), true);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Mesh", GUILayout.Width(100));
            if (twoSides == true)
            {
                EditorGUILayout.LabelField("SubMesh", GUILayout.Width(100));
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            flip = GUILayout.Toggle(flip, "Flip", GUILayout.Width(100));
            if (flip)
            {
                twoSides = false;
            }
            else
            {
                twoSides = true;
            }
            if (twoSides == true)
            {
                separate = GUILayout.Toggle(separate, "Separate", GUILayout.Width(100));
                if (separate)
                {
                    merge = false;
                }
                else
                {
                    merge = true;
                }
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            twoSides = GUILayout.Toggle(twoSides, "TwoSides", GUILayout.Width(100));
            if (twoSides)
            {
                flip = false;
            }
            else
            {
                flip = true;
            }
            if (twoSides == true)
            {
                merge = GUILayout.Toggle(merge, "Merge", GUILayout.Width(100));
                if (merge)
                {
                    separate = false;
                }
                else
                {
                    separate = true;
                }
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Create Mesh"))
        {
            if (renderer != null)
            {
                if (renderer.GetComponent<SkinnedMeshRenderer>())
                {
                    if (renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh != null)
                    {
                        if (flip == true)
                        {
                            FlipMesh(renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh, true);
                            
                        }
                        if (twoSides == true)
                        {
                            TwoSidesMesh(renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh, true);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Mesh is missing.", "OK");
                    }
                }
                else if (!renderer.GetComponent<SkinnedMeshRenderer>() && renderer.GetComponent<MeshFilter>())
                {
                    if (renderer.GetComponent<MeshFilter>().sharedMesh != null)
                    {
                        if (flip == true)
                        {
                            FlipMesh(renderer.GetComponent<MeshFilter>().sharedMesh, false);
                        }
                        if (twoSides == true)
                        {
                            TwoSidesMesh(renderer.GetComponent<MeshFilter>().sharedMesh, false);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Mesh is missing.", "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "MeshFilter is missing.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Renderer is not set.", "OK");
            }
        }
    }

    private void FlipMesh(Mesh mesh, bool a)
    {
        if (canCreate == false)
        {
            canCreate = true;
            return;
        }
        else
        {
            canCreate = false;
        }
        Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        newMesh.normals = normals;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                int temp = triangles[j];
                triangles[j] = triangles[j + 1];
                triangles[j + 1] = temp;
            }
            newMesh.SetTriangles(triangles, i);
        }
        if (!Directory.Exists("Assets/fum1/MeshFlipper/Generated"))
        {
            AssetDatabase.CreateFolder("Assets/fum1/MeshFlipper", "Generated");
        }
        string path = "Assets/fum1/MeshFlipper/Generated/" + Regex.Replace(mesh.name, @"_Flipped\d*", string.Empty) + "_Flipped.asset";
        int x = 1;
        while (File.Exists(path))
        {
            path = "Assets/fum1/MeshFlipper/Generated/" + Regex.Replace(mesh.name, @"_Flipped\d*", string.Empty) + "_Flipped" + x + ".asset";
            x++;
        }
        AssetDatabase.CreateAsset(newMesh, path);
        if (a == true)
        {
            renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = newMesh;
        }
        else
        {
            renderer.GetComponent<MeshFilter>().sharedMesh = newMesh;
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Notification", "Flipped Mesh has been created. path:" + path, "OK");
        canCreate = true;
    }

    private void TwoSidesMesh(Mesh mesh, bool a)
    {
        if (canCreate == false)
        {
            canCreate = true;
            return;
        }
        else
        {
            canCreate = false;
        }
        Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
        Vector3[] v = mesh.vertices;//毎回mesh.verticesを使うとめちゃくちゃ重くなる
        if (v.Length > 32767)
        {
            EditorUtility.DisplayDialog("Error", "Too many vertices. Number of vertices must be less than 32768.", "OK");
            canCreate = true;
            return;
        }
        Vector3[] n = mesh.normals;
        Vector4[] t = mesh.tangents;
        Vector3[] normals = new Vector3[n.Length * 2];
        int abwLength = mesh.GetAllBoneWeights().Length;
        int bpvLength = mesh.GetBonesPerVertex().Length;
        NativeArray<BoneWeight1> allBoneWeights = new NativeArray<BoneWeight1>(abwLength * 2, Allocator.Temp);
        NativeArray<Byte> bonesPerVertex = new NativeArray<Byte>(bpvLength * 2, Allocator.Temp);
        Array.Copy(n, 0, normals, 0, n.Length);
        for (int i = 0; i < n.Length; i++)
        {
            normals[n.Length + i] = -n[i];
        }
        for (int i = 0; i < bpvLength; i++)
        {
            bonesPerVertex[i] = mesh.GetBonesPerVertex()[i];
            bonesPerVertex[bpvLength + i] = mesh.GetBonesPerVertex()[i];
        }
        for (int i = 0; i < abwLength; i++)
        {
            allBoneWeights[i] = mesh.GetAllBoneWeights()[i];
            allBoneWeights[abwLength + i] = mesh.GetAllBoneWeights()[i];
        }
        newMesh.vertices = DoubleArray(v);
        newMesh.normals = normals;
        newMesh.tangents = DoubleArray(t);
        newMesh.colors = DoubleArray(mesh.colors);
        newMesh.colors32 = DoubleArray(mesh.colors32);
        newMesh.uv = DoubleArray(mesh.uv);
        newMesh.uv2 = DoubleArray(mesh.uv2);
        newMesh.uv3 = DoubleArray(mesh.uv3);
        newMesh.uv4 = DoubleArray(mesh.uv4);
        newMesh.uv5 = DoubleArray(mesh.uv5);
        newMesh.uv6 = DoubleArray(mesh.uv6);
        newMesh.uv7 = DoubleArray(mesh.uv7);
        newMesh.uv8 = DoubleArray(mesh.uv8);
        if (bpvLength != 0 | abwLength != 0)//WeightがないのにSetBoneWeightsをやるとエラーが出る
        {
            newMesh.SetBoneWeights(bonesPerVertex, allBoneWeights);
        }
        allBoneWeights.Dispose();//メモリ解放
        bonesPerVertex.Dispose();
        newMesh.ClearBlendShapes();
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            for (int j = 0; j < mesh.GetBlendShapeFrameCount(i); j++)
            {
                Vector3[] deltaVertices = new Vector3[v.Length];
                Vector3[] deltaNormals = new Vector3[n.Length];
                Vector3[] deltaTangents = new Vector3[t.Length];
                mesh.GetBlendShapeFrameVertices(i, j, deltaVertices, deltaNormals, deltaTangents);
                Array.Resize(ref deltaNormals, n.Length * 2);
                for (int k = 0; k < n.Length; k++)
                {
                    deltaNormals[n.Length + k] = -deltaNormals[k];
                }
                newMesh.AddBlendShapeFrame(mesh.GetBlendShapeName(i), mesh.GetBlendShapeFrameWeight(i, j), DoubleArray(deltaVertices), deltaNormals, DoubleArray(deltaTangents));
            }
        }
        if (separate == true)
        {
            newMesh.subMeshCount = mesh.subMeshCount * 2;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] tri = mesh.GetTriangles(i);
                int[] triangles = new int[tri.Length];
                for (int j = 0; j < tri.Length; j++)
                {
                    triangles[tri.Length - 1 - j] = tri[j] + v.Length;
                }
                newMesh.SetTriangles(tri, i * 2);
                newMesh.SetTriangles(triangles, i * 2 + 1);
            }
            Material[] materials = new Material[renderer.sharedMaterials.Length * 2];
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                materials[i * 2] = renderer.sharedMaterials[i];
                materials[i * 2 + 1] = renderer.sharedMaterials[i];
            }
            renderer.materials = materials;
        }
        if (merge == true)
        {
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] tri = mesh.GetTriangles(i);
                int[] triangles = new int[tri.Length * 2];
                for (int j = 0; j < tri.Length; j++)
                {
                    triangles[j] = tri[j];
                    triangles[tri.Length * 2 - 1 - j] = tri[j] + v.Length;
                }
                newMesh.SetTriangles(triangles, i);
            }
        }
        if (!Directory.Exists("Assets/fum1/MeshFlipper/Generated"))
        {
            AssetDatabase.CreateFolder("Assets/fum1/MeshFlipper", "Generated");
        }
        string path = "Assets/fum1/MeshFlipper/Generated/" + Regex.Replace(mesh.name, @"_TwoSided\d*", string.Empty) + "_TwoSided.asset";
        int x = 1;
        while (File.Exists(path))
        {
            path = "Assets/fum1/MeshFlipper/Generated/" + Regex.Replace(mesh.name, @"_TwoSided\d*", string.Empty) + "_TwoSided" + x + ".asset";
            x++;
        }
        AssetDatabase.CreateAsset(newMesh, path);
        if (a == true)
        {
            renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = newMesh;
        }
        else
        {
            renderer.GetComponent<MeshFilter>().sharedMesh = newMesh;
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Notification", "Two sided Mesh has been created. path:" + path, "OK");
        canCreate = true;
    }

    static T[] DoubleArray<T>(T[] array)
    {
        T[] doubledArray = new T[array.Length * 2];
        Array.Copy(array, 0, doubledArray, 0, array.Length);
        Array.Copy(array, 0, doubledArray, array.Length, array.Length);
        return doubledArray;
    }
}
