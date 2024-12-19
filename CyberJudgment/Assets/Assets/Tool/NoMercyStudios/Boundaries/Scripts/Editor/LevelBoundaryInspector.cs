using UnityEditor;
using UnityEngine;

namespace NoMercyStudios.BoundariesPro
{
    [CustomEditor(typeof(LevelBoundary))]
    public class LevelBoundaryInspector : Editor
    {
        SerializedProperty shapeType;
        SerializedProperty boundarySize;
        SerializedProperty boundaryCustomMesh;

        private void OnEnable()
        {
            shapeType = serializedObject.FindProperty("shapeType");
            boundarySize = serializedObject.FindProperty("boundarySize");
            boundaryCustomMesh = serializedObject.FindProperty("boundaryCustomMesh");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Display default properties
            EditorGUILayout.PropertyField(shapeType, new GUIContent("Shape Type"));
            EditorGUILayout.PropertyField(boundarySize, new GUIContent("Boundary Size"));

            // Display custom mesh property only if the shape type is set to Custom
            if ((LevelBoundary.LevelBoundaryShape)shapeType.enumValueIndex == LevelBoundary.LevelBoundaryShape.Custom)
            {
                EditorGUILayout.PropertyField(boundaryCustomMesh, new GUIContent("Custom Mesh"));
            }

            // Apply changes to the serializedObject
            serializedObject.ApplyModifiedProperties();
        }
    }
}