using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class EnemySpawnData
{
    [HorizontalGroup("Basic")]
    [LabelText("敵プリファブ")]
    [LabelWidth(80)]
    public GameObject enemyPrefab;

    [HorizontalGroup("Basic")]
    [LabelText("数量")]
    [LabelWidth(40)]
    [Range(1, 20)]
    public int count = 1;

    [HorizontalGroup("Settings")]
    [LabelText("スポーン間隔")]
    [LabelWidth(80)]
    [Range(0f, 2f)]
    public float spawnInterval = 0.2f;

    [HorizontalGroup("Settings")]
    [LabelText("ランダム配置")]
    [LabelWidth(80)]
    public bool randomizePosition = false;

    [ShowIf("randomizePosition")]
    [LabelText("ランダム半径")]
    [Range(0f, 5f)]
    public float randomRadius = 2f;

    [FoldoutGroup("挙動設定")]
    [LabelText("初期動作")]
    public FlyEnemyMovement.EnemyType initialMovement = FlyEnemyMovement.EnemyType.Circling;

    [FoldoutGroup("挙動設定")]
    [LabelText("移動速度倍率")]
    [Range(0.5f, 3f)]
    public float speedMultiplier = 1f;

    [FoldoutGroup("フォーメーション設定")]
    [LabelText("フォーメーション")]
    public FormationMode formationMode = FormationMode.None;

    [FoldoutGroup("フォーメーション設定")]
    [ShowIf("formationMode", FormationMode.UseFormation)]
    [LabelText("フォーメーションタイプ")]
    public FlyFormationManager.FormationType formationType = FlyFormationManager.FormationType.VFormation;

    [FoldoutGroup("フォーメーション設定")]
    [ShowIf("formationMode", FormationMode.UseFormation)]
    [LabelText("フォーメーション間隔")]
    [Range(1f, 10f)]
    public float formationSpacing = 2f;

    [FoldoutGroup("フォーメーション設定")]
    [ShowIf("formationMode", FormationMode.UseFormation)]
    [LabelText("フォーメーション維持")]
    public bool maintainFormation = true;

    [FoldoutGroup("フォーメーション設定")]
    [ShowIf("formationMode", FormationMode.UseFormation)]
    [ShowIf("maintainFormation")]
    [LabelText("フォーメーション強度")]
    [Range(1f, 10f)]
    public float formationStrength = 5f;

    // 新機能: スポーン位置の調整
    [FoldoutGroup("スポーン位置調整")]
    [LabelText("プレイヤー前方にスポーン")]
    public bool spawnAheadOfPlayer = true;

    [FoldoutGroup("スポーン位置調整")]
    [ShowIf("spawnAheadOfPlayer")]
    [LabelText("前方距離")]
    [Range(5f, 50f)]
    public float aheadDistance = 20f;

    [FoldoutGroup("スポーン位置調整")]
    [LabelText("初期向きをプレイヤーに")]
    public bool facePlayer = true;

    public enum FormationMode
    {
        [LabelText("フォーメーションなし")]
        None,
        [LabelText("フォーメーションを使用")]
        UseFormation
    }
}

[System.Serializable]
public class SpawnEvent
{
    [HorizontalGroup("Timing")]
    [LabelText("時間")]
    [LabelWidth(40)]
    [SuffixLabel("秒", true)]
    public float spawnTime;

    [HorizontalGroup("Timing")]
    [LabelText("有効")]
    [LabelWidth(40)]
    public bool enabled = true;

    [LabelText("イベント名")]
    public string eventName = "敵スポーン";

    [LabelText("スポーンポイント")]
    [ValueDropdown("GetSpawnPointNames")]
    public string spawnPointName;

    [ListDrawerSettings(ShowIndexLabels = true)]
    [LabelText("敵データ")]
    public List<EnemySpawnData> enemyData = new List<EnemySpawnData>();

    [FoldoutGroup("特殊設定")]
    [LabelText("音響効果")]
    public AudioClip spawnSound;

    [FoldoutGroup("特殊設定")]
    [LabelText("エフェクト")]
    public GameObject spawnEffect;

    [FoldoutGroup("特殊設定")]
    [LabelText("警告表示")]
    public bool showWarning = false;

    [FoldoutGroup("特殊設定")]
    [ShowIf("showWarning")]
    [LabelText("警告時間")]
    [Range(0.5f, 3f)]
    public float warningDuration = 1f;

    private IEnumerable<string> GetSpawnPointNames()
    {
        AerialBattleManager manager = GameObject.FindObjectOfType<AerialBattleManager>();
        List<string> names = new List<string> { "ランダム" };

        if (manager != null && manager.spawnPoints != null)
        {
            for (int i = 0; i < manager.spawnPoints.Count; i++)
            {
                if (manager.spawnPoints[i] != null)
                {
                    names.Add($"{i}: {manager.spawnPoints[i].name}");
                }
            }
        }
        return names;
    }
}

public class AerialBattleManager : MonoBehaviour
{
    [TabGroup("タイムライン")]
    [Header("タイムライン設定")]
    [LabelText("自動開始")]
    public bool autoStart = true;

    [TabGroup("タイムライン")]
    [LabelText("ループ再生")]
    public bool loopTimeline = false;

    [TabGroup("タイムライン")]
    [ShowIf("loopTimeline")]
    [LabelText("ループ間隔")]
    [Range(1f, 10f)]
    public float loopDelay = 5f;

    [TabGroup("タイムライン")]
    [LabelText("時間倍率")]
    [Range(0.1f, 3f)]
    public float timeScale = 1f;

    [TabGroup("スポーンポイント")]
    [Header("スポーンポイント")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    public List<Transform> spawnPoints = new List<Transform>();

    [TabGroup("スポーンポイント")]
    [Button("スポーンポイントを自動検索")]
    public void FindSpawnPoints()
    {
        spawnPoints.Clear();
        Transform[] points = FindObjectsOfType<Transform>();
        foreach (Transform point in points)
        {
            if (point.name.Contains("SpawnPoint") || point.CompareTag("SpawnPoint"))
            {
                spawnPoints.Add(point);
            }
        }
    }

    [TabGroup("イベント")]
    [Header("スポーンイベント")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ShowPaging = true, NumberOfItemsPerPage = 5)]
    public List<SpawnEvent> spawnEvents = new List<SpawnEvent>();

    [TabGroup("イベント")]
    [Button("時間順にソート")]
    public void SortEventsByTime()
    {
        spawnEvents.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
    }

    [TabGroup("イベント")]
    [Button("新しいイベントを追加")]
    public void AddNewEvent()
    {
        SpawnEvent newEvent = new SpawnEvent();
        newEvent.spawnTime = GetNextEventTime();
        newEvent.eventName = $"イベント {spawnEvents.Count + 1}";
        spawnEvents.Add(newEvent);
    }

    [TabGroup("管理")]
    [Header("敵管理")]
    [LabelText("最大敵数")]
    [Range(10, 100)]
    public int maxEnemyCount = 50;

    [TabGroup("管理")]
    [LabelText("敵を親オブジェクトに")]
    public bool parentEnemiesToManager = true;

    [TabGroup("管理")]
    [ShowIf("parentEnemiesToManager")]
    [LabelText("敵コンテナ")]
    public Transform enemyContainer;

    [TabGroup("管理")]
    [LabelText("フォーメーションマネージャープリファブ")]
    public GameObject formationManagerPrefab;

    // 新機能: プレイヤー追跡
    [TabGroup("プレイヤー設定")]
    [Header("プレイヤー設定")]
    [LabelText("プレイヤーオブジェクト")]
    public Transform playerTransform;

    [TabGroup("プレイヤー設定")]
    [LabelText("プレイヤー移動速度")]
    [Range(1f, 20f)]
    public float playerMoveSpeed = 10f;

    [TabGroup("デバッグ")]
    [Header("デバッグ")]
    [LabelText("デバッグモード")]
    public bool debugMode = false;

    [TabGroup("デバッグ")]
    [ShowIf("debugMode")]
    [LabelText("現在時間表示")]
    [ReadOnly]
    public float currentTime;

    [TabGroup("デバッグ")]
    [ShowIf("debugMode")]
    [LabelText("実行済みイベント")]
    [ReadOnly]
    public int executedEvents;

    [TabGroup("デバッグ")]
    [ShowIf("debugMode")]
    [LabelText("現在の敵数")]
    [ReadOnly]
    public int currentEnemyCount;

    private float battleTimer = 0f;
    private int currentEventIndex = 0;
    private bool isPlaying = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<FlyFormationManager> activeFormations = new List<FlyFormationManager>();
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // プレイヤーの自動検索
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (enemyContainer == null && parentEnemiesToManager)
        {
            GameObject container = new GameObject("EnemyContainer");
            container.transform.parent = transform;
            enemyContainer = container.transform;
        }

        if (autoStart)
        {
            StartBattle();
        }
    }

    void Update()
    {
        if (isPlaying)
        {
            battleTimer += Time.deltaTime * timeScale;
            currentTime = battleTimer;

            CheckAndExecuteEvents();
            UpdateEnemyCount();

            if (currentEventIndex >= spawnEvents.Count && loopTimeline)
            {
                StartCoroutine(RestartBattleWithDelay());
            }
        }
    }

    void CheckAndExecuteEvents()
    {
        while (currentEventIndex < spawnEvents.Count)
        {
            SpawnEvent currentEvent = spawnEvents[currentEventIndex];

            if (!currentEvent.enabled)
            {
                currentEventIndex++;
                continue;
            }

            if (battleTimer >= currentEvent.spawnTime)
            {
                ExecuteSpawnEvent(currentEvent);
                currentEventIndex++;
                executedEvents++;
            }
            else
            {
                break;
            }
        }
    }

    void ExecuteSpawnEvent(SpawnEvent spawnEvent)
    {
        if (debugMode)
        {
            Debug.Log($"[AerialBattleManager] イベント実行: {spawnEvent.eventName} (時間: {battleTimer:F2}秒)");
        }

        if (spawnEvent.showWarning)
        {
            StartCoroutine(ShowWarning(spawnEvent));
        }

        if (spawnEvent.spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnEvent.spawnSound);
        }

        Transform spawnPoint = GetSpawnPoint(spawnEvent.spawnPointName);
        StartCoroutine(SpawnEnemiesFromEvent(spawnEvent, spawnPoint));
    }

    Transform GetSpawnPoint(string spawnPointName)
    {
        if (spawnPointName == "ランダム" || string.IsNullOrEmpty(spawnPointName))
        {
            if (spawnPoints.Count > 0)
            {
                return spawnPoints[Random.Range(0, spawnPoints.Count)];
            }
            return transform;
        }

        if (spawnPointName.Contains(":"))
        {
            string indexStr = spawnPointName.Split(':')[0];
            if (int.TryParse(indexStr, out int index) && index < spawnPoints.Count)
            {
                return spawnPoints[index];
            }
        }

        return transform;
    }

    Vector3 CalculateSpawnPosition(Transform spawnPoint, EnemySpawnData enemyData)
    {
        Vector3 basePosition = spawnPoint.position;

        // プレイヤーの前方にスポーンする場合
        if (enemyData.spawnAheadOfPlayer && playerTransform != null)
        {
            Vector3 playerForward = playerTransform.forward.normalized;
            basePosition = playerTransform.position + playerForward * enemyData.aheadDistance;
        }

        // ランダム配置
        if (enemyData.randomizePosition)
        {
            Vector3 randomOffset = Random.insideUnitSphere * enemyData.randomRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y); // 上方向のみ
            basePosition += randomOffset;
        }

        return basePosition;
    }

    Quaternion CalculateSpawnRotation(Vector3 spawnPosition, EnemySpawnData enemyData)
    {
        if (enemyData.facePlayer && playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;
            return Quaternion.LookRotation(directionToPlayer);
        }

        return Quaternion.identity;
    }

    IEnumerator SpawnEnemiesFromEvent(SpawnEvent spawnEvent, Transform spawnPoint)
    {
        Dictionary<int, List<GameObject>> formationGroups = new Dictionary<int, List<GameObject>>();
        int currentFormationGroupId = 0;

        foreach (EnemySpawnData enemyData in spawnEvent.enemyData)
        {
            if (enemyData.enemyPrefab == null) continue;

            List<GameObject> currentGroupEnemies = new List<GameObject>();

            for (int i = 0; i < enemyData.count; i++)
            {
                if (spawnedEnemies.Count >= maxEnemyCount)
                {
                    if (debugMode)
                    {
                        Debug.LogWarning("[AerialBattleManager] 最大敵数に到達しました");
                    }
                    yield break;
                }

                Vector3 spawnPosition = CalculateSpawnPosition(spawnPoint, enemyData);
                Quaternion spawnRotation = CalculateSpawnRotation(spawnPosition, enemyData);

                if (spawnEvent.spawnEffect != null)
                {
                    GameObject effect = Instantiate(spawnEvent.spawnEffect, spawnPosition, Quaternion.identity);
                    Destroy(effect, 3f);
                }

                GameObject enemy = Instantiate(enemyData.enemyPrefab, spawnPosition, spawnRotation);

                // 敵を自分の子オブジェクトにする
                if (parentEnemiesToManager && enemyContainer != null)
                {
                    enemy.transform.parent = enemyContainer;
                }

                ConfigureSpawnedEnemy(enemy, enemyData);

                spawnedEnemies.Add(enemy);
                currentGroupEnemies.Add(enemy);

                if (enemyData.spawnInterval > 0 && i < enemyData.count - 1)
                {
                    yield return new WaitForSeconds(enemyData.spawnInterval);
                }
            }

            if (enemyData.formationMode == EnemySpawnData.FormationMode.UseFormation && currentGroupEnemies.Count > 0)
            {
                formationGroups[currentFormationGroupId] = new List<GameObject>(currentGroupEnemies);
                currentFormationGroupId++;
            }
        }

        SetupFormations(spawnEvent, formationGroups);
    }

    void ConfigureSpawnedEnemy(GameObject enemy, EnemySpawnData enemyData)
    {
        FlyEnemyMovement enemyMovement = enemy.GetComponent<FlyEnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.enemyType = enemyData.initialMovement;

            // 敵の移動速度をプレイヤーに合わせて調整
            float adjustedSpeed = Mathf.Max(playerMoveSpeed * 0.8f, enemyData.speedMultiplier * 5f);
            enemyMovement.moveSpeed = adjustedSpeed * enemyData.speedMultiplier;

            // プレイヤーをターゲットに設定
            if (playerTransform != null)
            {
                enemyMovement.target = playerTransform;
            }

            // 敵の追跡距離を調整（プレイヤーに追いつけるように）
            if (enemyMovement.followDistance < playerMoveSpeed)
            {
                enemyMovement.followDistance = playerMoveSpeed * 1.5f;
            }
        }
    }

    void SetupFormations(SpawnEvent spawnEvent, Dictionary<int, List<GameObject>> formationGroups)
    {
        if (formationGroups.Count == 0 || formationManagerPrefab == null) return;

        foreach (var formationGroup in formationGroups)
        {
            List<GameObject> enemies = formationGroup.Value;
            if (enemies.Count == 0) continue;

            EnemySpawnData formationData = null;
            int dataIndex = 0;

            foreach (EnemySpawnData data in spawnEvent.enemyData)
            {
                if (data.formationMode == EnemySpawnData.FormationMode.UseFormation)
                {
                    if (dataIndex == formationGroup.Key)
                    {
                        formationData = data;
                        break;
                    }
                    dataIndex++;
                }
            }

            if (formationData == null) continue;

            GameObject formationObj = Instantiate(formationManagerPrefab);
            formationObj.name = $"Formation_{formationGroup.Key}_{formationData.formationType}";

            // フォーメーションマネージャーも子オブジェクトにする
            if (parentEnemiesToManager && enemyContainer != null)
            {
                formationObj.transform.parent = enemyContainer;
            }

            FlyFormationManager formation = formationObj.GetComponent<FlyFormationManager>();

            if (formation != null)
            {
                List<FlyEnemyMovement> formationEnemies = new List<FlyEnemyMovement>();

                foreach (GameObject enemy in enemies)
                {
                    FlyEnemyMovement enemyMovement = enemy.GetComponent<FlyEnemyMovement>();
                    if (enemyMovement != null)
                    {
                        formationEnemies.Add(enemyMovement);
                    }
                }

                var enemiesField = typeof(FlyFormationManager).GetField("enemies",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                enemiesField?.SetValue(formation, formationEnemies);

                formation.formationType = formationData.formationType;
                formation.formationSpacing = formationData.formationSpacing;
                formation.maintainFormation = formationData.maintainFormation;
                formation.formationStrength = formationData.formationStrength;

                // フォーメーション速度もプレイヤーに合わせる
                formation.formationSpeed = Mathf.Max(playerMoveSpeed * 0.9f, 1f);

                formation.InitializeFormationButton();

                activeFormations.Add(formation);

                if (debugMode)
                {
                    Debug.Log($"[AerialBattleManager] フォーメーション作成: {formationData.formationType}, 敵数: {formationEnemies.Count}");
                }
            }
        }
    }

    IEnumerator ShowWarning(SpawnEvent spawnEvent)
    {
        if (debugMode)
        {
            Debug.Log($"[警告] {spawnEvent.eventName} が {spawnEvent.warningDuration}秒後に実行されます！");
        }

        yield return new WaitForSeconds(spawnEvent.warningDuration);
    }

    void UpdateEnemyCount()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        currentEnemyCount = spawnedEnemies.Count;

        activeFormations.RemoveAll(formation => formation == null);
    }

    IEnumerator RestartBattleWithDelay()
    {
        isPlaying = false;
        yield return new WaitForSeconds(loopDelay);
        RestartBattle();
    }

    float GetNextEventTime()
    {
        if (spawnEvents.Count == 0) return 0f;

        float maxTime = 0f;
        foreach (SpawnEvent evt in spawnEvents)
        {
            if (evt.spawnTime > maxTime)
                maxTime = evt.spawnTime;
        }

        return maxTime + 5f;
    }

    [TabGroup("制御")]
    [Button("バトル開始", ButtonSizes.Large)]
    public void StartBattle()
    {
        battleTimer = 0f;
        currentEventIndex = 0;
        executedEvents = 0;
        isPlaying = true;

        if (debugMode)
        {
            Debug.Log("[AerialBattleManager] バトル開始");
        }
    }

    [TabGroup("制御")]
    [Button("バトル停止", ButtonSizes.Large)]
    public void StopBattle()
    {
        isPlaying = false;

        if (debugMode)
        {
            Debug.Log("[AerialBattleManager] バトル停止");
        }
    }

    [TabGroup("制御")]
    [Button("バトルリセット")]
    public void RestartBattle()
    {
        StopBattle();
        ClearAllEnemies();
        StartBattle();
    }

    [TabGroup("制御")]
    [Button("全ての敵を除去")]
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                DestroyImmediate(enemy);
            }
        }
        spawnedEnemies.Clear();

        foreach (FlyFormationManager formation in activeFormations)
        {
            if (formation != null)
            {
                DestroyImmediate(formation.gameObject);
            }
        }
        activeFormations.Clear();

        currentEnemyCount = 0;
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoints[i].position, 1f);
                Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i].position + spawnPoints[i].forward * 2f);

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(spawnPoints[i].position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
        }

        if (debugMode && spawnEvents.Count > 0)
        {
            Gizmos.color = Color.yellow;
            float maxTime = 0f;
            foreach (SpawnEvent evt in spawnEvents)
            {
                if (evt.spawnTime > maxTime) maxTime = evt.spawnTime;
            }

            if (maxTime > 0)
            {
                float progress = battleTimer / maxTime;
                Vector3 progressPos = transform.position + Vector3.up * 5f + Vector3.right * (progress * 10f - 5f);
                Gizmos.DrawWireSphere(progressPos, 0.3f);
            }
        }
    }
}