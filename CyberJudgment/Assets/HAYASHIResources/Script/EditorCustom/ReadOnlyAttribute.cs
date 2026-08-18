using UnityEditor;
using UnityEngine;

namespace AbubuResouse.Editor
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
        public class ReadOnlyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                GUI.enabled = false; // フィールドを読み取り専用にするためにGUIを無効化
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = true; // 後続のフィールドが正常に表示されるようにGUIを有効化
            }
        }
#endif
    }
}
