using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace AbubuResouse.Spawn
{
    /// <summary>
    /// 敵のスポーンデータを定義する ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Spawn Data", menuName = "AbubuResouse/Enemy Spawn Data")]
    public class EnemySpawnData : ScriptableObject
    {
        [System.Serializable]
        public class EnemySpawnInfo
        {
            [TabGroup("基本情報"), PreviewField(80), LabelText("敵Prefab")]
            public GameObject enemyPrefab;               // 敵のプレハブ

            [TabGroup("基本情報"), LabelText("名前")]
            public string enemyName = "Enemy";           // 敵の名前

            [TabGroup("スポーン設定"), Range(1, 50), LabelText("最大数")]
            public int maxSpawnCount = 5;

            [TabGroup("スポーン設定"), Range(0.1f, 60f), LabelText("クールタイム(秒)")]
            public float spawnCoolTime = 2f;

            [TabGroup("スポーン設定"), Range(0f, 1f), LabelText("スポーン確率")]
            public float spawnProbability = 1f;

            [TabGroup("アニメーション"), Range(0.1f, 2f), LabelText("時間(秒)")]
            public float spawnAnimationDuration = 0.5f;

            [TabGroup("アニメーション"), Range(0f, 1f), LabelText("初期スケール")]
            public float initialScale = 0f;

            [TabGroup("アニメーション"), Range(0.5f, 2f), LabelText("最終スケール")]
            public float finalScale = 1f;

            [TabGroup("アニメーション"), LabelText("スケールカーブ")]
            public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            [TabGroup("エフェクト/サウンド"), PreviewField(60), LabelText("エフェクト")]
            public GameObject spawnEffect;

            [TabGroup("エフェクト/サウンド"), LabelText("SE名")]
            public string spawnSEName = "enemy_spawn";
        }

        [FoldoutGroup("スポーンデータ")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true, NumberOfItemsPerPage = 5)]
        public List<EnemySpawnInfo> enemySpawnInfos = new List<EnemySpawnInfo>();

        [FoldoutGroup("スポーンポイント設定"), Range(0.5f, 10f), LabelText("半径")]
        public float spawnRadius = 2f;

        [FoldoutGroup("スポーンポイント設定"), Range(0.1f, 5f), LabelText("高さオフセット")]
        public float spawnHeight = 0f;

        [FoldoutGroup("スポーンポイント設定"), LabelText("地面レイヤー")]
        public LayerMask groundLayerMask = 1;

        [FoldoutGroup("全体設定"), Range(0.1f, 10f), LabelText("基本間隔")]
        public float baseSpawnInterval = 1f;

        [FoldoutGroup("全体設定"), LabelText("ランダム順序")]
        public bool useRandomSpawnOrder = true;
    }
}
