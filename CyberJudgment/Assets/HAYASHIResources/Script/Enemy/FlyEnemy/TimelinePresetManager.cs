using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public class TimelinePreset
{
    [LabelText("プリセット名")]
    public string presetName = "新しいプリセット";

    [LabelText("説明")]
    [TextArea(2, 4)]
    public string description = "";

    [LabelText("難易度")]
    [Range(1, 5)]
    public int difficulty = 1;

    [LabelText("推定時間")]
    [SuffixLabel("秒", true)]
    public float estimatedDuration = 60f;

    [LabelText("イベントリスト")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<SpawnEvent> events = new List<SpawnEvent>();
}

[CreateAssetMenu(fileName = "TimelinePresetManager", menuName = "Palutena/Timeline Preset Manager")]
public class TimelinePresetManager : ScriptableObject
{
    [TabGroup("プリセット")]
    [Header("タイムラインプリセット")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    public List<TimelinePreset> presets = new List<TimelinePreset>();

    [TabGroup("テンプレート")]
    [Header("自動生成テンプレート")]
    [LabelText("基本敵プリファブ")]
    public List<GameObject> basicEnemyPrefabs = new List<GameObject>();

    [TabGroup("テンプレート")]
    [LabelText("強敵プリファブ")]
    public List<GameObject> eliteEnemyPrefabs = new List<GameObject>();

    [TabGroup("テンプレート")]
    [LabelText("ボスプリファブ")]
    public List<GameObject> bossPrefabs = new List<GameObject>();

    [TabGroup("テンプレート")]
    [Header("自動生成設定")]
    [LabelText("イベント数")]
    [Range(5, 50)]
    public int autoEventCount = 20;

    [TabGroup("テンプレート")]
    [LabelText("総時間")]
    [Range(30f, 300f)]
    public float autoTotalDuration = 120f;

    [TabGroup("テンプレート")]
    [LabelText("難易度カーブ")]
    public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 0.2f, 1, 1f);

    [TabGroup("テンプレート")]
    [LabelText("フォーメーション使用率")]
    [Range(0f, 1f)]
    public float formationUsageRate = 0.3f;

    [TabGroup("テンプレート")]
    [Button("ランダムタイムライン生成")]
    public TimelinePreset GenerateRandomTimeline()
    {
        TimelinePreset newPreset = new TimelinePreset();
        newPreset.presetName = $"自動生成_{System.DateTime.Now:HHmmss}";
        newPreset.description = "自動生成されたタイムライン";
        newPreset.estimatedDuration = autoTotalDuration;

        List<SpawnEvent> events = new List<SpawnEvent>();

        for (int i = 0; i < autoEventCount; i++)
        {
            float timeProgress = (float)i / (autoEventCount - 1);
            float eventTime = timeProgress * autoTotalDuration;
            float difficultyValue = difficultyCurve.Evaluate(timeProgress);

            SpawnEvent newEvent = CreateEventByDifficulty(eventTime, difficultyValue, i);
            events.Add(newEvent);
        }

        newPreset.events = events;
        return newPreset;
    }

    SpawnEvent CreateEventByDifficulty(float time, float difficulty, int index)
    {
        SpawnEvent evt = new SpawnEvent();
        evt.spawnTime = time;
        evt.enabled = true;
        evt.eventName = $"ウェーブ {index + 1}";
        evt.spawnPointName = "ランダム";

        // 難易度に応じて敵を選択
        List<EnemySpawnData> enemyDataList = new List<EnemySpawnData>();

        if (difficulty < 0.3f)
        {
            // 初級：基本敵のみ
            enemyDataList.Add(CreateEnemyData(GetRandomBasicEnemy(), Random.Range(1, 3),
                FlyEnemyMovement.EnemyType.Circling, false));
        }
        else if (difficulty < 0.6f)
        {
            // 中級：基本敵 + 少数の強敵
            enemyDataList.Add(CreateEnemyData(GetRandomBasicEnemy(), Random.Range(2, 5),
                FlyEnemyMovement.EnemyType.Following, ShouldUseFormation()));

            if (Random.value > 0.5f && eliteEnemyPrefabs.Count > 0)
            {
                enemyDataList.Add(CreateEnemyData(GetRandomEliteEnemy(), 1,
                    FlyEnemyMovement.EnemyType.Spiral, false));
            }
        }
        else
        {
            // 終級：複合編成
            enemyDataList.Add(CreateEnemyData(GetRandomBasicEnemy(), Random.Range(3, 7),
                FlyEnemyMovement.EnemyType.ZigZag, ShouldUseFormation()));

            if (eliteEnemyPrefabs.Count > 0)
            {
                enemyDataList.Add(CreateEnemyData(GetRandomEliteEnemy(), Random.Range(1, 3),
                    FlyEnemyMovement.EnemyType.Dive, ShouldUseFormation()));
            }

            // ボス出現チャンス
            if (difficulty > 0.8f && Random.value > 0.7f && bossPrefabs.Count > 0)
            {
                enemyDataList.Add(CreateEnemyData(GetRandomBossEnemy(), 1,
                    FlyEnemyMovement.EnemyType.Following, false));
            }
        }

        evt.enemyData = enemyDataList;

        // 警告の設定
        if (difficulty > 0.5f)
        {
            evt.showWarning = true;
            evt.warningDuration = Mathf.Lerp(0.5f, 2f, difficulty);
        }

        return evt;
    }

    bool ShouldUseFormation()
    {
        return Random.value < formationUsageRate;
    }

    EnemySpawnData CreateEnemyData(GameObject prefab, int count, FlyEnemyMovement.EnemyType movement, bool useFormation = false)
    {
        if (prefab == null) return null;

        EnemySpawnData data = new EnemySpawnData();
        data.enemyPrefab = prefab;
        data.count = count;
        data.spawnInterval = Random.Range(0.1f, 0.5f);
        data.randomizePosition = Random.value > 0.5f;
        data.randomRadius = Random.Range(1f, 3f);
        data.initialMovement = movement;
        data.speedMultiplier = Random.Range(0.8f, 1.5f);

        // フォーメーション設定
        if (useFormation)
        {
            data.formationMode = EnemySpawnData.FormationMode.UseFormation;

            FlyFormationManager.FormationType[] formations =
            {
                FlyFormationManager.FormationType.VFormation,
                FlyFormationManager.FormationType.Line,
                FlyFormationManager.FormationType.Circle,
                FlyFormationManager.FormationType.Diamond,
                FlyFormationManager.FormationType.Wave
            };
            data.formationType = formations[Random.Range(0, formations.Length)];
            data.formationSpacing = Random.Range(1.5f, 4f);
            data.maintainFormation = Random.value > 0.3f;
            data.formationStrength = Random.Range(3f, 8f);
        }
        else
        {
            data.formationMode = EnemySpawnData.FormationMode.None;
        }

        return data;
    }

    GameObject GetRandomBasicEnemy()
    {
        return basicEnemyPrefabs.Count > 0 ?
               basicEnemyPrefabs[Random.Range(0, basicEnemyPrefabs.Count)] : null;
    }

    GameObject GetRandomEliteEnemy()
    {
        return eliteEnemyPrefabs.Count > 0 ?
               eliteEnemyPrefabs[Random.Range(0, eliteEnemyPrefabs.Count)] : null;
    }

    GameObject GetRandomBossEnemy()
    {
        return bossPrefabs.Count > 0 ?
               bossPrefabs[Random.Range(0, bossPrefabs.Count)] : null;
    }

    [TabGroup("プリセット")]
    [Button("新しいプリセットを作成")]
    public void CreateNewPreset()
    {
        TimelinePreset newPreset = new TimelinePreset();
        newPreset.presetName = $"プリセット {presets.Count + 1}";
        presets.Add(newPreset);
    }

    [TabGroup("プリセット")]
    [Button("プリセットを複製")]
    public void DuplicatePreset(int index)
    {
        if (index >= 0 && index < presets.Count)
        {
            TimelinePreset original = presets[index];
            TimelinePreset duplicate = new TimelinePreset();

            duplicate.presetName = original.presetName + "_コピー";
            duplicate.description = original.description;
            duplicate.difficulty = original.difficulty;
            duplicate.estimatedDuration = original.estimatedDuration;
            duplicate.events = new List<SpawnEvent>(original.events);

            presets.Add(duplicate);
        }
    }

    public void ApplyPresetToManager(AerialBattleManager manager, int presetIndex)
    {
        if (manager == null || presetIndex < 0 || presetIndex >= presets.Count)
        {
            Debug.LogError("無効なマネージャーまたはプリセットインデックス");
            return;
        }

        TimelinePreset preset = presets[presetIndex];

        // イベントリストをコピー
        manager.spawnEvents = new List<SpawnEvent>(preset.events);

        Debug.Log($"プリセット '{preset.presetName}' を適用しました。");
    }

    [TabGroup("エクスポート")]
    [Button("JSONエクスポート")]
    public void ExportToJSON()
    {
        string json = JsonUtility.ToJson(this, true);
        string path = UnityEditor.EditorUtility.SaveFilePanel(
            "タイムラインをエクスポート", "", "timeline_presets", "json");

        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"タイムラインをエクスポートしました: {path}");
        }
    }

    [TabGroup("エクスポート")]
    [Button("JSONインポート")]
    public void ImportFromJSON()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel(
            "タイムラインをインポート", "", "json");

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string json = System.IO.File.ReadAllText(path);
                TimelinePresetManager imported = JsonUtility.FromJson<TimelinePresetManager>(json);

                foreach (TimelinePreset preset in imported.presets)
                {
                    preset.presetName += "_インポート";
                    presets.Add(preset);
                }

                Debug.Log($"タイムラインをインポートしました: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"インポートエラー: {e.Message}");
            }
        }
    }

    [TabGroup("ユーティリティ")]
    [Button("すべてのプリセットを時間順ソート")]
    public void SortAllPresetsByTime()
    {
        foreach (TimelinePreset preset in presets)
        {
            preset.events.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
        }

        Debug.Log("すべてのプリセットを時間順にソートしました。");
    }

    [TabGroup("ユーティリティ")]
    [Button("空のイベントを除去")]
    public void RemoveEmptyEvents()
    {
        foreach (TimelinePreset preset in presets)
        {
            preset.events.RemoveAll(evt => evt.enemyData == null || evt.enemyData.Count == 0);
        }

        Debug.Log("空のイベントを除去しました。");
    }

    [TabGroup("ユーティリティ")]
    [Button("フォーメーション設定を一括更新")]
    public void UpdateFormationSettings()
    {
        int updatedCount = 0;

        foreach (TimelinePreset preset in presets)
        {
            foreach (SpawnEvent evt in preset.events)
            {
                foreach (EnemySpawnData data in evt.enemyData)
                {
                    // 旧式のuseFormationフィールドがある場合の移行処理
                    // リフレクションで旧フィールドをチェック
                    var oldUseFormationField = typeof(EnemySpawnData).GetField("useFormation");
                    if (oldUseFormationField != null)
                    {
                        bool oldUseFormation = (bool)oldUseFormationField.GetValue(data);
                        if (oldUseFormation)
                        {
                            data.formationMode = EnemySpawnData.FormationMode.UseFormation;
                            updatedCount++;
                        }
                        else
                        {
                            data.formationMode = EnemySpawnData.FormationMode.None;
                        }
                    }
                }
            }
        }

        Debug.Log($"フォーメーション設定を更新しました。更新数: {updatedCount}");
    }

    [TabGroup("統計")]
    [Button("プリセット統計を表示")]
    public void ShowPresetStatistics()
    {
        Debug.Log("=== タイムラインプリセット統計 ===");
        Debug.Log($"総プリセット数: {presets.Count}");

        int totalFormationEvents = 0;
        int totalNonFormationEvents = 0;

        foreach (TimelinePreset preset in presets)
        {
            int totalEnemies = 0;
            int formationEnemies = 0;
            int nonFormationEnemies = 0;

            foreach (SpawnEvent evt in preset.events)
            {
                foreach (EnemySpawnData data in evt.enemyData)
                {
                    totalEnemies += data.count;

                    if (data.formationMode == EnemySpawnData.FormationMode.UseFormation)
                    {
                        formationEnemies += data.count;
                        totalFormationEvents++;
                    }
                    else
                    {
                        nonFormationEnemies += data.count;
                        totalNonFormationEvents++;
                    }
                }
            }

            Debug.Log($"プリセット '{preset.presetName}': " +
                     $"イベント数={preset.events.Count}, " +
                     $"総敵数={totalEnemies}, " +
                     $"フォーメーション敵数={formationEnemies}, " +
                     $"単体敵数={nonFormationEnemies}, " +
                     $"時間={preset.estimatedDuration}秒, " +
                     $"難易度={preset.difficulty}");
        }

        Debug.Log($"全体統計: フォーメーション使用={totalFormationEvents}, 単体行動={totalNonFormationEvents}");
    }

    [TabGroup("テンプレート")]
    [Button("フォーメーション重視タイムライン生成")]
    public TimelinePreset GenerateFormationFocusedTimeline()
    {
        float originalRate = formationUsageRate;
        formationUsageRate = 0.8f; // フォーメーション使用率を80%に設定

        TimelinePreset preset = GenerateRandomTimeline();
        preset.presetName = $"フォーメーション重視_{System.DateTime.Now:HHmmss}";
        preset.description = "フォーメーションを重視した自動生成タイムライン";

        formationUsageRate = originalRate; // 元の値に戻す
        return preset;
    }

    [TabGroup("テンプレート")]
    [Button("単体行動重視タイムライン生成")]
    public TimelinePreset GenerateIndividualFocusedTimeline()
    {
        float originalRate = formationUsageRate;
        formationUsageRate = 0.1f; // フォーメーション使用率を10%に設定

        TimelinePreset preset = GenerateRandomTimeline();
        preset.presetName = $"単体行動重視_{System.DateTime.Now:HHmmss}";
        preset.description = "単体行動を重視した自動生成タイムライン";

        formationUsageRate = originalRate; // 元の値に戻す
        return preset;
    }
}