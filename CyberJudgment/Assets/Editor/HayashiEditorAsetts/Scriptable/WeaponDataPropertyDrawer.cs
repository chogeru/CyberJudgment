using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(WeaponData))]
public class WeaponDataPropertyDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 各フィールドのレイアウトをカスタマイズ
        var weaponIDRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var weaponNameRect = new Rect(position.x, position.y + 20, position.width, EditorGUIUtility.singleLineHeight);
        var weaponTypeRect = new Rect(position.x, position.y + 40, position.width, EditorGUIUtility.singleLineHeight);
        var weaponAttributeRect = new Rect(position.x, position.y + 60, position.width, EditorGUIUtility.singleLineHeight);
        var weaponDescriptionRect = new Rect(position.x, position.y + 80, position.width, EditorGUIUtility.singleLineHeight * 3);
        var attackPowerRect = new Rect(position.x, position.y + 140, position.width, EditorGUIUtility.singleLineHeight);
        var attackSpeedRect = new Rect(position.x, position.y + 160, position.width, EditorGUIUtility.singleLineHeight);
        var weaponWeightRect = new Rect(position.x, position.y + 180, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(weaponIDRect, property.FindPropertyRelative("_weaponID"), new GUIContent("武器Id"));
        EditorGUI.PropertyField(weaponNameRect, property.FindPropertyRelative("_weaponName"), new GUIContent("武器の名前"));
        EditorGUI.PropertyField(attackPowerRect, property.FindPropertyRelative("_attackPower"), new GUIContent("攻撃力"));
        EditorGUI.PropertyField(attackSpeedRect, property.FindPropertyRelative("_attackSpeed"), new GUIContent("攻撃速度"));
        EditorGUI.PropertyField(weaponWeightRect, property.FindPropertyRelative("_weaponWeight"), new GUIContent("武器重量"));
        EditorGUI.PropertyField(weaponTypeRect, property.FindPropertyRelative("_weaponType"), new GUIContent("武器のタイプ"));
        EditorGUI.PropertyField(weaponAttributeRect, property.FindPropertyRelative("_weaponAttribute"), new GUIContent("武器の属性"));
        EditorGUI.PropertyField(weaponDescriptionRect, property.FindPropertyRelative("_weaponDescription"), new GUIContent("武器説明文"));


        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // フィールドの高さを設定
        return EditorGUIUtility.singleLineHeight * 10;
    }
}
