using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(GameObject))]
[CanEditMultipleObjects]
public class EnhancedGameObjectInspector : Editor
{
    private Editor defaultEditor;
    private Vector2 scrollPos;
    private List<Component> components = new List<Component>();
    private static List<Component> copiedComponents = new List<Component>();

    private void OnEnable()
    {
        System.Type gameObjectInspectorType = System.Type.GetType("UnityEditor.GameObjectInspector, UnityEditor");
        if (gameObjectInspectorType != null)
        {
            defaultEditor = CreateEditor(targets, gameObjectInspectorType);
        }
        RefreshComponents();
    }

    private void OnDisable()
    {
        if (defaultEditor != null)
        {
            DestroyImmediate(defaultEditor);
        }
    }

    public override void OnInspectorGUI()
    {
        if (targets.Length > 1)
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("複数の GameObject には拡張機能はまだサポートしていない。", MessageType.Info);
            return;
        }

        if (defaultEditor != null)
        {
            defaultEditor.DrawHeader();

            defaultEditor.OnInspectorGUI();
        }

        EditorGUILayout.LabelField("あぶぶインスペクター拡張アセット", EditorStyles.boldLabel);

        GameObject selectedObject = (GameObject)target;
        if (selectedObject == null)
        {
            EditorGUILayout.LabelField("対象の GameObject が存在しない");
            return;
        }

        RefreshComponents();

        EditorGUILayout.LabelField("コンポーネント一覧", EditorStyles.boldLabel);

        float cellWidth = 80f;
        float cellHeight = 80f;
        float spacing = 4f;
        float iconSize = 40f;
        float labelHeight = 20f; 
        float verticalPadding = (cellHeight - iconSize - labelHeight) / 2f;
        int columns = Mathf.Max(1, Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 20f) / (cellWidth + spacing)));

        GUIStyle componentLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        if (EditorGUIUtility.isProSkin)
        {
            componentLabelStyle.normal.textColor = Color.white;
        }
        else
        {
            componentLabelStyle.normal.textColor = Color.black;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(cellHeight * 1.5f));
        for (int i = 0; i < components.Count; i++)
        {
            if (i % columns == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            Component comp = components[i];
            Texture compIcon = EditorGUIUtility.ObjectContent(comp, comp.GetType()).image;
            if (compIcon == null)
            {
                compIcon = EditorGUIUtility.IconContent("DefaultAsset Icon").image;
            }

            Rect rect = GUILayoutUtility.GetRect(cellWidth, cellHeight,
                GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = cellWidth,
                fixedHeight = cellHeight
            };
            if (GUI.Button(rect, GUIContent.none, buttonStyle))
            {
                FocusComponent(comp);
            }

            Rect iconRect = new Rect(
                rect.x + (cellWidth - iconSize) / 2f,
                rect.y + verticalPadding,
                iconSize,
                iconSize
            );
            GUI.DrawTexture(iconRect, compIcon, ScaleMode.ScaleToFit);

            Rect labelRect = new Rect(
                rect.x,
                rect.y + verticalPadding + iconSize,
                cellWidth,
                labelHeight
            );
            GUI.Label(labelRect, comp.GetType().Name, componentLabelStyle);

            if ((i % columns) == (columns - 1) || i == components.Count - 1)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();

        GUIContent copyIconContent = EditorGUIUtility.IconContent("EditCopy");
        if (copyIconContent.image == null)
        {
            copyIconContent = EditorGUIUtility.IconContent("Clipboard");
        }
        GUIContent copyContent = new GUIContent(" Copy Components", copyIconContent.image);

        if (GUILayout.Button(copyContent, GUILayout.Height(24)))
        {
            CopyComponents(selectedObject);
        }

        GUIContent pasteIconContent = EditorGUIUtility.IconContent("EditPaste");
        if (pasteIconContent.image == null)
        {
            pasteIconContent = EditorGUIUtility.IconContent("Clipboard");
        }
        GUIContent pasteContent = new GUIContent(" Paste Components", pasteIconContent.image);

        if (GUILayout.Button(pasteContent, GUILayout.Height(24)))
        {
            PasteComponents(selectedObject);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 対象オブジェクトのコンポーネント一覧を更新
    /// </summary>
    private void RefreshComponents()
    {
        components.Clear();
        GameObject obj = target as GameObject;
        if (obj != null)
        {
            components.AddRange(obj.GetComponents<Component>());
        }
    }

    /// <summary>
    /// コンポーネントをコピー（Transform は除外）
    /// </summary>
    private void CopyComponents(GameObject obj)
    {
        copiedComponents.Clear();
        if (obj != null)
        {
            foreach (var comp in obj.GetComponents<Component>())
            {
                if (comp is Transform)
                    continue;
                copiedComponents.Add(comp);
            }
            Debug.Log("コンポーネントをコピーした！: " + obj.name);
        }
    }

    /// <summary>
    /// コピーしたコンポーネントのシリアライズデータを貼り付け
    /// </summary>
    private void PasteComponents(GameObject obj)
    {
        if (obj != null && copiedComponents.Count > 0)
        {
            Dictionary<Component, Component> mapping = new Dictionary<Component, Component>();
            foreach (var comp in copiedComponents)
            {
                System.Type type = comp.GetType();
                if (type == typeof(Transform))
                    continue;

                if (obj.GetComponent(type) == null)
                {
                    Component newComp = obj.AddComponent(type);
                    EditorUtility.CopySerialized(comp, newComp);
                    mapping.Add(comp, newComp);
                }
            }

            foreach (var kvp in mapping)
            {
                Component originalComp = kvp.Key;
                Component newComp = kvp.Value;
                SerializedObject so = new SerializedObject(newComp);
                SerializedProperty sp = so.GetIterator();
                bool enterChildren = true;
                while (sp.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue != null)
                    {
                        Object referencedObj = sp.objectReferenceValue;
                        foreach (var orig in mapping.Keys)
                        {
                            if (referencedObj == orig)
                            {
                                sp.objectReferenceValue = mapping[orig];
                                break;
                            }
                        }
                    }
                }
                so.ApplyModifiedProperties();
            }

            Debug.Log("コンポーネントを貼り付けた！ " + obj.name);
        }
    }

    /// <summary>
    /// コンポーネントがクリックされたとき、専用ウィンドウでインスペクターを表示
    /// </summary>
    private void FocusComponent(Component comp)
    {
        ComponentInspectorWindow.ShowWindow(comp);
    }
}

/// <summary>
/// 選択したコンポーネントのインスペクターを表示する EditorWindow
/// </summary>
public class ComponentInspectorWindow : EditorWindow
{
    private Editor componentEditor;
    private Component component;

    public static void ShowWindow(Component comp)
    {
        ComponentInspectorWindow window = CreateInstance<ComponentInspectorWindow>();
        window.component = comp;
        window.titleContent = new GUIContent(comp.GetType().Name);
        window.minSize = new Vector2(300, 400);
        window.Show();
    }

    private void OnEnable()
    {
        if (component != null)
        {
            componentEditor = Editor.CreateEditor(component);
        }
    }

    private void OnDisable()
    {
        if (componentEditor != null)
        {
            DestroyImmediate(componentEditor);
        }
    }

    private void OnGUI()
    {
        if (component == null)
        {
            EditorGUILayout.HelpBox("No component selected.", MessageType.Info);
            return;
        }

        if (componentEditor == null)
        {
            componentEditor = Editor.CreateEditor(component);
        }

        if (componentEditor != null)
        {
            componentEditor.OnInspectorGUI();
        }
        else
        {
            EditorGUILayout.HelpBox("Editor could not be created for this component.", MessageType.Warning);
        }
    }
}
