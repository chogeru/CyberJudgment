using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyData))]
public class EnemyDataPropertyDrawer : Editor
{

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyID"), new GUIContent("敵ID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyName"), new GUIContent("敵の名前"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"), new GUIContent("体力"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"), new GUIContent("移動速度"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionRange"), new GUIContent("索敵範囲"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("visionRange"), new GUIContent("視野判定"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRange"), new GUIContent("攻撃距離"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackCooldown"), new GUIContent("攻撃間隔"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropItemPrefab"), new GUIContent("ドロップアイテムのプレハブ"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropItemCount"), new GUIContent("ドロップアイテムの数"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyAttribute"), new GUIContent("属性"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviorType"), new GUIContent("行動タイプ"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyType"), new GUIContent("敵タイプ"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"), new GUIContent("戦闘武器種類"));
        BehaviorType behaviorType = (BehaviorType)serializedObject.FindProperty("behaviorType").enumValueIndex;
        if (behaviorType == BehaviorType.Patrol)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolPointWaitTime"), new GUIContent("徘徊ポイント停止時間"));
        }

        WeaponType weaponType = (WeaponType)serializedObject.FindProperty("weaponType").enumValueIndex;
        if (weaponType == WeaponType.Sword || weaponType == WeaponType.Axe || weaponType == WeaponType.Hammer)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeAttackPower"), new GUIContent("近接攻撃力"));
        }
        else if (weaponType == WeaponType.Bow || weaponType == WeaponType.Gun)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileSpeed"), new GUIContent("弾速"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectilePower"), new GUIContent("弾の威力"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileCount"), new GUIContent("弾数"));
        }
        else if (weaponType == WeaponType.Magic)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magicPower"), new GUIContent("魔力"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magicAttackInterval"), new GUIContent("魔法攻撃間隔"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
