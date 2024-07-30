using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyData))]
public class EnemyDataEditor : Editor
{
    private GUIStyle headerStyle;
    private bool enemyInfoFoldout = true;
    private bool statsFoldout = true;
    private bool attributesFoldout = true;

    private void OnEnable()
    {
        // Initialize custom style
        headerStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 14,
            fixedHeight = 20,
            padding = new RectOffset(10, 0, 0, 0),
            contentOffset = new Vector2(0, 0)
        };

        // Set gradient background
        headerStyle.normal.background = MakeTex(1, 20, Color.black, Color.gray);
        headerStyle.normal.textColor = Color.white;
        headerStyle.onNormal.background = MakeTex(1, 20, Color.black, Color.gray);
        headerStyle.onNormal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        EnemyData enemyData = (EnemyData)target;

        serializedObject.Update();

        EditorGUILayout.BeginVertical("box");

        enemyInfoFoldout = DrawFoldoutHeader("敵の情報", enemyInfoFoldout);
        if (enemyInfoFoldout)
        {
            DrawPropertyField("enemyID", "敵ID", "d_UnityEditor.InspectorWindow");
            DrawPropertyField("enemyName", "敵の名前", "d_UnityEditor.ProjectWindow");
        }

        statsFoldout = DrawFoldoutHeader("ステータス", statsFoldout);
        if (statsFoldout)
        {
            DrawPropertyField("health", "体力", "d_UnityEditor.AnimationWindow");
            DrawPropertyField("moveSpeed", "移動速度", "d_UnityEditor.ProfilerWindow");
            DrawPropertyField("detectionRange", "索敵範囲", "d_UnityEditor.GameView");
            DrawPropertyField("visionRange", "視野判定", "d_UnityEditor.GameView");
            DrawPropertyField("attackRange", "攻撃距離", "d_UnityEditor.GameView");
            DrawPropertyField("attackCooldown", "攻撃間隔", "d_UnityEditor.GameView");
            DrawPropertyField("dropItemPrefab", "ドロップアイテムのプレハブ", "d_UnityEditor.SceneView");
            DrawPropertyField("dropItemCount", "ドロップアイテムの数", "d_UnityEditor.HierarchyWindow");
        }

        attributesFoldout = DrawFoldoutHeader("タイプ", attributesFoldout);
        if (attributesFoldout)
        {
            DrawPropertyField("enemyAttribute", "属性", "d_UnityEditor.ConsoleWindow", true);
            DrawPropertyField("enemyType", "敵タイプ", "d_UnityEditor.HierarchyWindow", true);
            DrawPropertyField("behaviorType", "行動タイプ", "d_UnityEditor.HierarchyWindow", true);
            BehaviorType behaviorType = (BehaviorType)serializedObject.FindProperty("behaviorType").enumValueIndex;
            if (behaviorType == BehaviorType.Patrol)
            {
                DrawPropertyField("patrolPointWaitTime", "徘徊ポイント停止時間", "d_UnityEditor.ProfilerWindow");
            }
            DrawPropertyField("weaponType", "戦闘武器種類", "d_UnityEditor.SceneView", true);
            WeaponType weaponType = (WeaponType)serializedObject.FindProperty("weaponType").enumValueIndex;
            if (weaponType == WeaponType.Sword || weaponType == WeaponType.Axe || weaponType == WeaponType.Hammer)
            {
                DrawPropertyField("meleeAttackPower", "近接攻撃力", "d_UnityEditor.AnimationWindow");
            }
            else if (weaponType == WeaponType.Bow || weaponType == WeaponType.Gun)
            {
                DrawPropertyField("projectileSpeed", "弾速", "d_UnityEditor.ProjectWindow");
                DrawPropertyField("projectilePower", "弾の威力", "d_UnityEditor.GameView");
                DrawPropertyField("projectileCount", "弾数", "d_UnityEditor.HierarchyWindow");
            }
            else if (weaponType == WeaponType.Magic)
            {
                DrawPropertyField("magicPower", "魔力", "d_UnityEditor.ConsoleWindow");
                DrawPropertyField("magicAttackInterval", "魔法攻撃間隔", "d_UnityEditor.InspectorWindow");
            }
        }

        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(enemyData);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool DrawFoldoutHeader(string title, bool foldout)
    {
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        foldout = EditorGUILayout.Foldout(foldout, title, true, headerStyle);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        return foldout;
    }

    private void DrawPropertyField(string propertyName, string label, string icon, bool isEnum = false, bool isTextArea = false)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(EditorGUIUtility.IconContent(icon), GUILayout.Width(20), GUILayout.Height(20));
        EditorGUILayout.LabelField(label, GUILayout.Width(100));

        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (isEnum)
        {
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }
        else if (isTextArea)
        {
            property.stringValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.Height(60));
        }
        else
        {
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }

        EditorGUILayout.EndHorizontal();
    }

    private Texture2D MakeTex(int width, int height, Color col1, Color col2)
    {
        Color[] pix = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            Color lerpedColor = Color.Lerp(col1, col2, (float)y / height);
            for (int x = 0; x < width; x++)
            {
                pix[y * width + x] = lerpedColor;
            }
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
