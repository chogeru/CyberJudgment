using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyFormationManager : MonoBehaviour
{
    [TabGroup("編隊設定")]
    [Header("編隊設定")]
    public FormationType formationType = FormationType.VFormation;

    [TabGroup("編隊設定")]
    [SerializeField]
    private List<FlyEnemyMovement> enemies = new List<FlyEnemyMovement>();

    [TabGroup("編隊設定")]
    [Range(1f, 10f)]
    public float formationSpacing = 2f;

    [TabGroup("編隊設定")]
    [Range(0.1f, 5f)]
    public float formationSpeed = 1f;

    [TabGroup("編隊設定")]
    public bool maintainFormation = true;

    [TabGroup("編隊設定")]
    [ShowIf("maintainFormation")]
    public float formationStrength = 5f;

    [TabGroup("攻撃パターン")]
    [Header("攻撃パターン")]
    public bool enableGroupAttack = true;

    [TabGroup("攻撃パターン")]
    [ShowIf("enableGroupAttack")]
    public AttackPattern attackPattern = AttackPattern.Simultaneous;

    [TabGroup("攻撃パターン")]
    [ShowIf("enableGroupAttack")]
    public float attackInterval = 3f;

    [TabGroup("攻撃パターン")]
    [ShowIf("enableGroupAttack")]
    public GameObject projectilePrefab;

    [TabGroup("攻撃パターン")]
    [ShowIf("enableGroupAttack")]
    public float projectileSpeed = 10f;

    [TabGroup("特殊行動")]
    [Header("特殊行動")]
    public bool enableSpecialManeuvers = true;

    [TabGroup("特殊行動")]
    [ShowIf("enableSpecialManeuvers")]
    public SpecialManeuver specialManeuver = SpecialManeuver.None;

    [TabGroup("特殊行動")]
    [ShowIf("enableSpecialManeuvers")]
    public float maneuverInterval = 10f;

    [TabGroup("特殊行動")]
    [ShowIf("enableSpecialManeuvers")]
    public float maneuverDuration = 3f;

    [TabGroup("デバッグ")]
    [Header("デバッグ")]
    public bool showFormationLines = true;

    [TabGroup("デバッグ")]
    public bool showAttackRange = false;

    [TabGroup("デバッグ")]
    [ShowIf("showAttackRange")]
    public float attackRange = 8f;

    private Transform player;
    private Vector3 formationCenter;
    private float attackTimer;
    private float maneuverTimer;
    private bool isPerformingManeuver = false;
    private Vector3 maneuverTarget;

    public enum FormationType
    {
        [LabelText("V字型")]
        VFormation,
        [LabelText("一列")]
        Line,
        [LabelText("円形")]
        Circle,
        [LabelText("ダイヤモンド")]
        Diamond,
        [LabelText("ウェーブ")]
        Wave,
        [LabelText("ランダム")]
        Random
    }

    public enum AttackPattern
    {
        [LabelText("同時攻撃")]
        Simultaneous,
        [LabelText("順次攻撃")]
        Sequential,
        [LabelText("ランダム攻撃")]
        Random,
        [LabelText("集中攻撃")]
        Focused
    }

    public enum SpecialManeuver
    {
        [LabelText("なし")]
        None,
        [LabelText("一斉突撃")]
        Charge,
        [LabelText("包囲攻撃")]
        Surround,
        [LabelText("螺旋攻撃")]
        Spiral,
        [LabelText("散開攻撃")]
        Scatter
    }

    void Start()
    {
        // プレイヤーを検索
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 編隊の初期化
        InitializeFormation();

        // 敵を自動検索
        if (enemies.Count == 0)
        {
            FlyEnemyMovement[] foundEnemies = FindObjectsOfType<FlyEnemyMovement>();
            enemies.AddRange(foundEnemies);
        }

        formationCenter = transform.position;
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        maneuverTimer += Time.deltaTime;

        // 編隊の維持
        if (maintainFormation && !isPerformingManeuver)
        {
            MaintainFormation();
        }

        // 攻撃パターンの実行
        if (enableGroupAttack && attackTimer >= attackInterval)
        {
            ExecuteAttackPattern();
            attackTimer = 0f;
        }

        // 特殊機動の実行
        if (enableSpecialManeuvers && maneuverTimer >= maneuverInterval && !isPerformingManeuver)
        {
            StartCoroutine(ExecuteSpecialManeuver());
            maneuverTimer = 0f;
        }

        // 編隊中心の更新
        UpdateFormationCenter();
    }

    void InitializeFormation()
    {
        SetFormationPositions();
    }

    void MaintainFormation()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;

            Vector3 targetPosition = GetFormationPosition(i);
            Vector3 currentPosition = enemies[i].transform.position;
            Vector3 direction = (targetPosition - currentPosition).normalized;

            float distance = Vector3.Distance(currentPosition, targetPosition);
            if (distance > 0.5f)
            {
                enemies[i].transform.position += direction * formationStrength * Time.deltaTime;
            }
        }
    }

    Vector3 GetFormationPosition(int index)
    {
        Vector3 position = formationCenter;

        switch (formationType)
        {
            case FormationType.VFormation:
                return GetVFormationPosition(index);
            case FormationType.Line:
                return GetLineFormationPosition(index);
            case FormationType.Circle:
                return GetCircleFormationPosition(index);
            case FormationType.Diamond:
                return GetDiamondFormationPosition(index);
            case FormationType.Wave:
                return GetWaveFormationPosition(index);
            case FormationType.Random:
                return GetRandomFormationPosition(index);
        }

        return position;
    }

    Vector3 GetVFormationPosition(int index)
    {
        if (index == 0) return formationCenter; // リーダーは中央

        int side = (index % 2 == 1) ? 1 : -1;
        int row = (index + 1) / 2;

        return formationCenter + new Vector3(
            side * row * formationSpacing,
            0,
            -row * formationSpacing
        );
    }

    Vector3 GetLineFormationPosition(int index)
    {
        float offset = (index - (enemies.Count - 1) * 0.5f) * formationSpacing;
        return formationCenter + new Vector3(offset, 0, 0);
    }

    Vector3 GetCircleFormationPosition(int index)
    {
        float angle = (index * 2f * Mathf.PI) / enemies.Count;
        float radius = formationSpacing * 2f;

        return formationCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            0,
            Mathf.Sin(angle) * radius
        );
    }

    Vector3 GetDiamondFormationPosition(int index)
    {
        Vector3[] diamondPositions = {
            Vector3.zero,
            Vector3.forward * formationSpacing,
            Vector3.right * formationSpacing,
            Vector3.back * formationSpacing,
            Vector3.left * formationSpacing
        };

        int posIndex = index % diamondPositions.Length;
        return formationCenter + diamondPositions[posIndex];
    }

    Vector3 GetWaveFormationPosition(int index)
    {
        float x = index * formationSpacing;
        float z = Mathf.Sin(index * 0.5f) * formationSpacing;

        return formationCenter + new Vector3(x - (enemies.Count * formationSpacing * 0.5f), 0, z);
    }

    Vector3 GetRandomFormationPosition(int index)
    {
        Random.InitState(index + 1000); // 一貫性のあるランダム
        Vector3 randomOffset = Random.insideUnitSphere * formationSpacing;
        randomOffset.y = 0; // Y軸は固定

        return formationCenter + randomOffset;
    }

    void SetFormationPositions()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].transform.position = GetFormationPosition(i);
            }
        }
    }

    void ExecuteAttackPattern()
    {
        if (player == null || projectilePrefab == null) return;

        switch (attackPattern)
        {
            case AttackPattern.Simultaneous:
                ExecuteSimultaneousAttack();
                break;
            case AttackPattern.Sequential:
                StartCoroutine(ExecuteSequentialAttack());
                break;
            case AttackPattern.Random:
                ExecuteRandomAttack();
                break;
            case AttackPattern.Focused:
                ExecuteFocusedAttack();
                break;
        }
    }

    void ExecuteSimultaneousAttack()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                FireProjectileFromEnemy(enemy);
            }
        }
    }

    IEnumerator ExecuteSequentialAttack()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                FireProjectileFromEnemy(enemy);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    void ExecuteRandomAttack()
    {
        int numAttackers = Random.Range(1, enemies.Count + 1);
        List<FlyEnemyMovement> shuffledEnemies = new List<FlyEnemyMovement>(enemies);

        for (int i = 0; i < shuffledEnemies.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledEnemies.Count);
            var temp = shuffledEnemies[i];
            shuffledEnemies[i] = shuffledEnemies[randomIndex];
            shuffledEnemies[randomIndex] = temp;
        }

        for (int i = 0; i < numAttackers && i < shuffledEnemies.Count; i++)
        {
            if (shuffledEnemies[i] != null)
            {
                FireProjectileFromEnemy(shuffledEnemies[i]);
            }
        }
    }

    void ExecuteFocusedAttack()
    {
        Vector3 playerPosition = player.position;
        Vector3 centerPosition = Vector3.zero;
        int validEnemies = 0;

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                centerPosition += enemy.transform.position;
                validEnemies++;
            }
        }

        if (validEnemies > 0)
        {
            centerPosition /= validEnemies;
            Vector3 attackDirection = (playerPosition - centerPosition).normalized;

            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    FireProjectileFromEnemy(enemy, attackDirection);
                }
            }
        }
    }

    void FireProjectileFromEnemy(FlyEnemyMovement enemy, Vector3? customDirection = null)
    {
        Vector3 fireDirection = customDirection ?? (player.position - enemy.transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, enemy.transform.position, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (projectileRb != null)
        {
            projectileRb.velocity = fireDirection * projectileSpeed;
        }

        Destroy(projectile, 5f);
    }

    IEnumerator ExecuteSpecialManeuver()
    {
        isPerformingManeuver = true;

        switch (specialManeuver)
        {
            case SpecialManeuver.Charge:
                yield return StartCoroutine(PerformCharge());
                break;
            case SpecialManeuver.Surround:
                yield return StartCoroutine(PerformSurround());
                break;
            case SpecialManeuver.Spiral:
                yield return StartCoroutine(PerformSpiral());
                break;
            case SpecialManeuver.Scatter:
                yield return StartCoroutine(PerformScatter());
                break;
        }

        isPerformingManeuver = false;
    }

    IEnumerator PerformCharge()
    {
        if (player == null) yield break;

        Vector3 chargeDirection = (player.position - formationCenter).normalized;

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ChangeMovementPattern(FlyEnemyMovement.EnemyType.Dive);
            }
        }

        yield return new WaitForSeconds(maneuverDuration);

        // 元の動きに戻す
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ChangeMovementPattern(FlyEnemyMovement.EnemyType.Circling);
            }
        }
    }

    IEnumerator PerformSurround()
    {
        if (player == null) yield break;

        // 敵をプレイヤーの周りに配置
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
            {
                float angle = (i * 2f * Mathf.PI) / enemies.Count;
                Vector3 surroundPosition = player.position + new Vector3(
                    Mathf.Cos(angle) * attackRange,
                    0,
                    Mathf.Sin(angle) * attackRange
                );

                enemies[i].target = null; // 一時的にターゲットを解除
                StartCoroutine(MoveEnemyToPosition(enemies[i], surroundPosition));
            }
        }

        yield return new WaitForSeconds(maneuverDuration);

        // ターゲットを復元
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.target = player;
            }
        }
    }

    IEnumerator PerformSpiral()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ChangeMovementPattern(FlyEnemyMovement.EnemyType.Spiral);
            }
        }

        yield return new WaitForSeconds(maneuverDuration);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ChangeMovementPattern(FlyEnemyMovement.EnemyType.Following);
            }
        }
    }

    IEnumerator PerformScatter()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ChangeMovementPattern(FlyEnemyMovement.EnemyType.Random);
            }
        }

        yield return new WaitForSeconds(maneuverDuration);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ChangeMovementPattern(FlyEnemyMovement.EnemyType.Following);
            }
        }
    }

    IEnumerator MoveEnemyToPosition(FlyEnemyMovement enemy, Vector3 targetPosition)
    {
        float moveTime = 2f;
        Vector3 startPosition = enemy.transform.position;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;
            enemy.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
    }

    void UpdateFormationCenter()
    {
        if (player != null)
        {
            // プレイヤーの前方に編隊中心を設定
            Vector3 playerForward = player.forward;
            formationCenter = player.position + playerForward * 5f;
        }
    }

    [Button("編隊を初期化")]
    public void InitializeFormationButton()
    {
        InitializeFormation();
    }

    [Button("編隊タイプを変更")]
    public void ChangeFormationType(FormationType newType)
    {
        formationType = newType;
        SetFormationPositions();
    }

    [Button("敵を自動検索")]
    public void FindEnemies()
    {
        enemies.Clear();
        FlyEnemyMovement[] foundEnemies = FindObjectsOfType<FlyEnemyMovement>();
        enemies.AddRange(foundEnemies);
    }

    [Button("特殊機動を実行")]
    public void ExecuteManeuverNow()
    {
        if (!isPerformingManeuver)
        {
            StartCoroutine(ExecuteSpecialManeuver());
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showFormationLines && enemies.Count > 0)
        {
            Gizmos.color = Color.cyan;

            // 編隊の線を描画
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null)
                {
                    Vector3 formationPos = GetFormationPosition(i);
                    Gizmos.DrawLine(enemies[i].transform.position, formationPos);
                    Gizmos.DrawWireSphere(formationPos, 0.2f);
                }
            }

            // 編隊中心を描画
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(formationCenter, 0.5f);
        }

        if (showAttackRange && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, attackRange);
        }
    }
}
