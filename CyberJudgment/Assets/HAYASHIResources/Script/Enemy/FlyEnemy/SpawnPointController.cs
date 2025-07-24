using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnPointController : MonoBehaviour
{
    [TabGroup("基本設定")]
    [Header("スポーンポイント設定")]
    [LabelText("ポイント名")]
    public string pointName = "SpawnPoint";

    [TabGroup("基本設定")]
    [LabelText("スポーンタイプ")]
    public SpawnType spawnType = SpawnType.Ground;

    [TabGroup("基本設定")]
    [LabelText("使用可能")]
    public bool isActive = true;

    [TabGroup("基本設定")]
    [LabelText("優先度")]
    [Range(1, 10)]
    public int priority = 5;

    [TabGroup("位置設定")]
    [Header("位置とサイズ")]
    [LabelText("スポーン範囲")]
    [Range(0f, 10f)]
    public float spawnRadius = 2f;

    [TabGroup("位置設定")]
    [LabelText("高度オフセット")]
    [Range(-5f, 10f)]
    public float heightOffset = 0f;

    [TabGroup("位置設定")]
    [LabelText("前方スポーン")]
    public bool spawnInFront = true;

    [TabGroup("位置設定")]
    [ShowIf("spawnInFront")]
    [LabelText("前方距離")]
    [Range(1f, 20f)]
    public float frontDistance = 5f;

    [TabGroup("制限")]
    [Header("スポーン制限")]
    [LabelText("最大敵数")]
    [Range(1, 50)]
    public int maxEnemies = 10;

    [TabGroup("制限")]
    [LabelText("クールダウン")]
    [Range(0f, 10f)]
    public float cooldownTime = 1f;

    [TabGroup("制限")]
    [LabelText("プレイヤー距離制限")]
    public bool respectPlayerDistance = true;

    [TabGroup("制限")]
    [ShowIf("respectPlayerDistance")]
    [LabelText("最小プレイヤー距離")]
    [Range(2f, 20f)]
    public float minPlayerDistance = 5f;

    [TabGroup("制限")]
    [ShowIf("respectPlayerDistance")]
    [LabelText("最大プレイヤー距離")]
    [Range(5f, 50f)]
    public float maxPlayerDistance = 30f;

    [TabGroup("エフェクト")]
    [Header("エフェクト設定")]
    [LabelText("スポーンエフェクト")]
    public GameObject defaultSpawnEffect;

    [TabGroup("エフェクト")]
    [LabelText("警告エフェクト")]
    public GameObject warningEffect;

    [TabGroup("エフェクト")]
    [LabelText("スポーン音")]
    public AudioClip spawnSound;

    [TabGroup("エフェクト")]
    [LabelText("警告時間")]
    [Range(0.5f, 3f)]
    public float warningDuration = 1f;

    [TabGroup("デバッグ")]
    [Header("デバッグ")]
    [LabelText("可視化")]
    public bool showGizmos = true;

    [TabGroup("デバッグ")]
    [ShowIf("showGizmos")]
    [LabelText("ギズモ色")]
    public Color gizmoColor = Color.green;

    [TabGroup("デバッグ")]
    [LabelText("現在の敵数")]
    [ReadOnly]
    public int currentEnemyCount = 0;

    [TabGroup("デバッグ")]
    [LabelText("最後のスポーン時間")]
    [ReadOnly]
    public float lastSpawnTime = 0f;

    private Transform playerTransform;
    private AudioSource audioSource;

    public enum SpawnType
    {
        [LabelText("地上")]
        Ground,
        [LabelText("空中")]
        Air,
        [LabelText("高空")]
        HighAir,
        [LabelText("背後")]
        Behind,
        [LabelText("側面")]
        Side,
        [LabelText("上空")]
        Above
    }

    void Start()
    {
        // プレイヤーを検索
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // AudioSourceを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // タグを設定
        if (!gameObject.CompareTag("SpawnPoint"))
        {
            gameObject.tag = "SpawnPoint";
        }

        // 名前を設定
        if (string.IsNullOrEmpty(pointName))
        {
            pointName = gameObject.name;
        }
    }

    void Update()
    {
        // 現在の敵数を更新（子オブジェクトの敵をカウント）
        UpdateCurrentEnemyCount();

        // プレイヤー距離チェック
        if (respectPlayerDistance && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            isActive = distance >= minPlayerDistance && distance <= maxPlayerDistance;
        }
    }

    public bool CanSpawn()
    {
        if (!isActive) return false;
        if (currentEnemyCount >= maxEnemies) return false;
        if (Time.time - lastSpawnTime < cooldownTime) return false;

        return true;
    }

    public Vector3 GetSpawnPosition(bool useRandomPosition = false)
    {
        Vector3 basePosition = transform.position;

        // 高度オフセット適用
        basePosition.y += heightOffset;

        // スポーンタイプに応じた位置調整
        basePosition = ApplySpawnTypeOffset(basePosition);

        // 前方スポーン
        if (spawnInFront)
        {
            basePosition += transform.forward * frontDistance;
        }

        // ランダム位置
        if (useRandomPosition && spawnRadius > 0)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y); // 上方向のみ
            basePosition += randomOffset;
        }

        return basePosition;
    }

    Vector3 ApplySpawnTypeOffset(Vector3 basePosition)
    {
        switch (spawnType)
        {
            case SpawnType.Air:
                basePosition.y += 3f;
                break;
            case SpawnType.HighAir:
                basePosition.y += 8f;
                break;
            case SpawnType.Behind:
                if (playerTransform != null)
                {
                    Vector3 playerForward = playerTransform.forward;
                    basePosition = playerTransform.position - playerForward * 10f;
                    basePosition.y += 2f;
                }
                break;
            case SpawnType.Side:
                if (playerTransform != null)
                {
                    Vector3 playerRight = playerTransform.right;
                    float side = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
                    basePosition = playerTransform.position + playerRight * side * 8f;
                }
                break;
            case SpawnType.Above:
                if (playerTransform != null)
                {
                    basePosition = playerTransform.position + Vector3.up * 12f;
                }
                break;
        }

        return basePosition;
    }

    public Quaternion GetSpawnRotation()
    {
        if (playerTransform != null)
        {
            // プレイヤーの方向を向く
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            return Quaternion.LookRotation(directionToPlayer);
        }

        return transform.rotation;
    }

    public void OnEnemySpawned()
    {
        lastSpawnTime = Time.time;
        currentEnemyCount++;

        // スポーン音を再生
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

    void UpdateCurrentEnemyCount()
    {
        // 子オブジェクトの敵をカウント
        int count = 0;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<FlyEnemyMovement>() != null)
            {
                count++;
            }
        }
        currentEnemyCount = count;
    }

    [TabGroup("制御")]
    [Button("警告エフェクト再生")]
    public void PlayWarningEffect()
    {
        if (warningEffect != null)
        {
            GameObject warning = Instantiate(warningEffect, GetSpawnPosition(), GetSpawnRotation());
            Destroy(warning, warningDuration);
        }
    }

    [TabGroup("制御")]
    [Button("スポーンエフェクト再生")]
    public void PlaySpawnEffect()
    {
        if (defaultSpawnEffect != null)
        {
            GameObject effect = Instantiate(defaultSpawnEffect, GetSpawnPosition(), GetSpawnRotation());
            Destroy(effect, 3f);
        }
    }

    [TabGroup("制御")]
    [Button("テストスポーン位置表示")]
    public void ShowTestSpawnPosition()
    {
        Vector3 testPos = GetSpawnPosition(true);
        Debug.Log($"[{pointName}] テストスポーン位置: {testPos}");

        // シーンビューでマーカーを表示
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "TestSpawnMarker";
        marker.transform.position = testPos;
        marker.transform.localScale = Vector3.one * 0.5f;

        // 5秒後に削除
        if (Application.isPlaying)
        {
            Destroy(marker, 5f);
        }
    }

    [TabGroup("制御")]
    [Button("プレイヤー距離チェック")]
    public void CheckPlayerDistance()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            Debug.Log($"[{pointName}] プレイヤーまでの距離: {distance:F2}m");
            Debug.Log($"使用可能: {(distance >= minPlayerDistance && distance <= maxPlayerDistance)}");
        }
        else
        {
            Debug.LogWarning("プレイヤーが見つかりません");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;

        // メインのスポーンポイント
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // スポーン範囲
        if (spawnRadius > 0)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }

        // 前方方向
        Gizmos.color = gizmoColor;
        if (spawnInFront)
        {
            Vector3 frontPos = transform.position + transform.forward * frontDistance;
            Gizmos.DrawLine(transform.position, frontPos);
            Gizmos.DrawWireSphere(frontPos, 0.3f);
        }

        // プレイヤー距離範囲
        if (respectPlayerDistance && playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, minPlayerDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, maxPlayerDistance);
        }

        // スポーンタイプの可視化
        Gizmos.color = Color.cyan;
        Vector3 typePosition = ApplySpawnTypeOffset(transform.position + Vector3.up * heightOffset);
        Gizmos.DrawWireCube(typePosition, Vector3.one * 0.5f);

        // 名前表示
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, pointName);
#endif
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // 詳細情報の表示
        Gizmos.color = Color.white;

        // 現在の敵数表示用の小さなキューブ
        for (int i = 0; i < currentEnemyCount; i++)
        {
            float angle = (i * 360f / Mathf.Max(1, maxEnemies)) * Mathf.Deg2Rad;
            Vector3 cubePos = transform.position + new Vector3(
                Mathf.Cos(angle) * 2f,
                1f,
                Mathf.Sin(angle) * 2f
            );
            Gizmos.DrawWireCube(cubePos, Vector3.one * 0.2f);
        }

        // 優先度の可視化
        Gizmos.color = Color.magenta;
        for (int i = 0; i < priority; i++)
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.up * (i * 0.5f + 3f), 0.1f);
        }
    }
}