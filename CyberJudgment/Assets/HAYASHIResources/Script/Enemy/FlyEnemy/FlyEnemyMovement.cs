using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class FlyEnemyMovement : MonoBehaviour
{
    [TabGroup("基本設定")]
    [Header("基本設定")]
    public EnemyType enemyType = EnemyType.Circling;

    [TabGroup("基本設定")]
    [Range(0.5f, 20f)]
    public float moveSpeed = 5f;

    [TabGroup("基本設定")]
    [Range(10f, 360f)]
    public float rotationSpeed = 90f;

    [TabGroup("基本設定")]
    [Header("プレイヤー通過設定")]
    public PassBehavior passBehavior = PassBehavior.StopBeforePlayer;

    [TabGroup("基本設定")]
    [ShowIf("passBehavior", PassBehavior.StopAtDistance)]
    [Range(1f, 10f)]
    public float stopDistance = 3f;

    [TabGroup("基本設定")]
    [ShowIf("passBehavior", PassBehavior.PassThrough)]
    public bool returnAfterPass = true;

    [TabGroup("基本設定")]
    [ShowIf("returnAfterPass")]
    public float returnDelay = 2f;

    [TabGroup("円運動")]
    [Header("円運動設定")]
    [Range(1f, 10f)]
    public float circleRadius = 3f;

    [TabGroup("円運動")]
    [Range(0.5f, 5f)]
    public float circleSpeed = 2f;

    [TabGroup("円運動")]
    public bool clockwise = true;

    [TabGroup("円運動")]
    [Header("楕円運動")]
    public bool useEllipse = false;

    [TabGroup("円運動")]
    [ShowIf("useEllipse")]
    public float ellipseWidth = 5f;

    [TabGroup("円運動")]
    [ShowIf("useEllipse")]
    public float ellipseHeight = 3f;

    [TabGroup("波動")]
    [Header("波動設定")]
    [Range(0.5f, 5f)]
    public float waveAmplitude = 2f;

    [TabGroup("波動")]
    [Range(0.5f, 5f)]
    public float waveFrequency = 2f;

    [TabGroup("波動")]
    public WaveAxis waveAxis = WaveAxis.Vertical;

    [TabGroup("波動")]
    public bool useComplexWave = false;

    [TabGroup("波動")]
    [ShowIf("useComplexWave")]
    public float secondaryWaveAmplitude = 1f;

    [TabGroup("波動")]
    [ShowIf("useComplexWave")]
    public float secondaryWaveFrequency = 3f;

    [TabGroup("ジグザグ")]
    [Header("ジグザグ設定")]
    [Range(1f, 10f)]
    public float zigzagWidth = 3f;

    [TabGroup("ジグザグ")]
    [Range(1f, 10f)]
    public float zigzagSpeed = 3f;

    [TabGroup("ジグザグ")]
    public ZigzagType zigzagType = ZigzagType.Linear;

    [TabGroup("ジグザグ")]
    [ShowIf("zigzagType", ZigzagType.Curved)]
    public float curveSmoothness = 2f;

    [TabGroup("追尾")]
    [Header("追尾設定")]
    public Transform target;

    [TabGroup("追尾")]
    [Range(1f, 15f)]
    public float followDistance = 8f;

    [TabGroup("追尾")]
    [Range(0.5f, 5f)]
    public float avoidanceRadius = 2f;

    [TabGroup("追尾")]
    public FollowType followType = FollowType.Circle;

    [TabGroup("追尾")]
    [ShowIf("followType", FollowType.Orbit)]
    public float orbitSpeed = 1f;

    [TabGroup("追尾")]
    [ShowIf("followType", FollowType.Weave)]
    public float weaveAmplitude = 2f;

    [TabGroup("上級")]
    [Header("上級設定")]
    public bool useGravity = false;

    [TabGroup("上級")]
    [ShowIf("useGravity")]
    public float gravityStrength = 9.8f;

    [TabGroup("上級")]
    public bool enableTurbo = false;

    [TabGroup("上級")]
    [ShowIf("enableTurbo")]
    public float turboMultiplier = 2f;

    [TabGroup("上級")]
    [ShowIf("enableTurbo")]
    public float turboInterval = 3f;

    [TabGroup("上級")]
    public bool enableRandomVariation = false;

    [TabGroup("上級")]
    [ShowIf("enableRandomVariation")]
    [Range(0f, 1f)]
    public float randomVariationStrength = 0.2f;

    [TabGroup("デバッグ")]
    [Header("デバッグ設定")]
    public bool showDebugInfo = false;

    [TabGroup("デバッグ")]
    [ShowIf("showDebugInfo")]
    public bool drawMovementPath = true;

    [TabGroup("デバッグ")]
    [ShowIf("showDebugInfo")]
    public bool showCurrentState = true;

    private Vector3 startPosition;
    private float timer;
    private int zigzagDirection = 1;
    private Vector3 lastPosition;
    private Vector3 velocity;
    private bool hasPassed = false;
    private bool isReturning = false;
    private float turboTimer = 0f;
    private bool isTurboActive = false;
    private Vector3 randomOffset;

    public enum EnemyType
    {
        [LabelText("円運動")]
        Circling,
        [LabelText("波動")]
        WavePattern,
        [LabelText("ジグザグ")]
        ZigZag,
        [LabelText("追尾")]
        Following,
        [LabelText("螺旋")]
        Spiral,
        [LabelText("急降下")]
        Dive,
        [LabelText("回転攻撃")]
        Spin,
        [LabelText("バウンス")]
        Bounce,
        [LabelText("ホバリング")]
        Hover,
        [LabelText("ランダム")]
        Random
    }

    public enum PassBehavior
    {
        [LabelText("プレイヤー手前で停止")]
        StopBeforePlayer,
        [LabelText("指定距離で停止")]
        StopAtDistance,
        [LabelText("通り過ぎる")]
        PassThrough,
        [LabelText("プレイヤーに向かって突進")]
        ChargeAtPlayer
    }

    public enum WaveAxis
    {
        [LabelText("上下")]
        Vertical,
        [LabelText("左右")]
        Horizontal,
        [LabelText("両方")]
        Both
    }

    public enum ZigzagType
    {
        [LabelText("直線")]
        Linear,
        [LabelText("曲線")]
        Curved,
        [LabelText("ランダム")]
        Random
    }

    public enum FollowType
    {
        [LabelText("円運動")]
        Circle,
        [LabelText("軌道")]
        Orbit,
        [LabelText("織り合い")]
        Weave,
        [LabelText("ストーク")]
        Stalk
    }

    void Start()
    {
        startPosition = transform.position;
        lastPosition = transform.position;

        // ターゲットが設定されていない場合はプレイヤーを探す
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // ランダムオフセットを初期化
        if (enableRandomVariation)
        {
            randomOffset = Random.insideUnitSphere * randomVariationStrength;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        turboTimer += Time.deltaTime;

        // ターボモードの切り替え
        if (enableTurbo && turboTimer >= turboInterval)
        {
            isTurboActive = !isTurboActive;
            turboTimer = 0f;
        }

        // 移動速度の計算
        float currentSpeed = moveSpeed;
        if (isTurboActive)
            currentSpeed *= turboMultiplier;

        // 通過行動の確認
        if (ShouldStopMovement())
        {
            return;
        }

        // 移動パターンの実行
        switch (enemyType)
        {
            case EnemyType.Circling:
                MoveInCircle();
                break;
            case EnemyType.WavePattern:
                MoveInWavePattern();
                break;
            case EnemyType.ZigZag:
                MoveInZigZag();
                break;
            case EnemyType.Following:
                FollowTarget();
                break;
            case EnemyType.Spiral:
                MoveInSpiral();
                break;
            case EnemyType.Dive:
                PerformDive();
                break;
            case EnemyType.Spin:
                PerformSpin();
                break;
            case EnemyType.Bounce:
                PerformBounce();
                break;
            case EnemyType.Hover:
                PerformHover();
                break;
            case EnemyType.Random:
                PerformRandomMovement();
                break;
        }

        // 重力の適用
        if (useGravity)
        {
            Vector3 gravityVector = Vector3.down * gravityStrength * Time.deltaTime;
            transform.position += gravityVector;
        }

        // ランダムバリエーションの適用
        if (enableRandomVariation)
        {
            Vector3 randomMovement = randomOffset * Time.deltaTime;
            transform.position += randomMovement;

            // 定期的にランダムオフセットを更新
            if (Random.value < 0.01f)
            {
                randomOffset = Random.insideUnitSphere * randomVariationStrength;
            }
        }

        // 速度の計算
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // 敵の向きを調整
        RotateTowardsMovement();
    }

    bool ShouldStopMovement()
    {
        if (target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        switch (passBehavior)
        {
            case PassBehavior.StopBeforePlayer:
                return distanceToTarget <= avoidanceRadius;

            case PassBehavior.StopAtDistance:
                return distanceToTarget <= stopDistance;

            case PassBehavior.PassThrough:
                if (!hasPassed && distanceToTarget <= avoidanceRadius)
                {
                    hasPassed = true;
                    if (returnAfterPass)
                    {
                        Invoke("StartReturning", returnDelay);
                    }
                }
                return false;

            case PassBehavior.ChargeAtPlayer:
                return false;
        }

        return false;
    }

    void StartReturning()
    {
        isReturning = true;
        startPosition = transform.position;
    }

    void MoveInCircle()
    {
        float direction = clockwise ? 1f : -1f;

        if (useEllipse)
        {
            float x = startPosition.x + Mathf.Cos(timer * circleSpeed * direction) * ellipseWidth;
            float z = startPosition.z + Mathf.Sin(timer * circleSpeed * direction) * ellipseHeight;
            transform.position = new Vector3(x, transform.position.y, z);
        }
        else
        {
            float x = startPosition.x + Mathf.Cos(timer * circleSpeed * direction) * circleRadius;
            float z = startPosition.z + Mathf.Sin(timer * circleSpeed * direction) * circleRadius;
            transform.position = new Vector3(x, transform.position.y, z);
        }
    }

    void MoveInWavePattern()
    {
        Vector3 newPosition = transform.position;
        newPosition.z += moveSpeed * Time.deltaTime;

        float waveValue = Mathf.Sin(timer * waveFrequency) * waveAmplitude;

        if (useComplexWave)
        {
            waveValue += Mathf.Sin(timer * secondaryWaveFrequency) * secondaryWaveAmplitude;
        }

        switch (waveAxis)
        {
            case WaveAxis.Vertical:
                newPosition.y = startPosition.y + waveValue;
                break;
            case WaveAxis.Horizontal:
                newPosition.x = startPosition.x + waveValue;
                break;
            case WaveAxis.Both:
                newPosition.y = startPosition.y + waveValue;
                newPosition.x = startPosition.x + Mathf.Cos(timer * waveFrequency) * waveAmplitude;
                break;
        }

        transform.position = newPosition;
    }

    void MoveInZigZag()
    {
        Vector3 newPosition = transform.position;
        newPosition.z += moveSpeed * Time.deltaTime;

        switch (zigzagType)
        {
            case ZigzagType.Linear:
                newPosition.x += zigzagDirection * zigzagSpeed * Time.deltaTime;
                break;
            case ZigzagType.Curved:
                float curveValue = Mathf.Sin(timer * curveSmoothness) * zigzagSpeed;
                newPosition.x += curveValue * Time.deltaTime;
                break;
            case ZigzagType.Random:
                newPosition.x += Random.Range(-zigzagSpeed, zigzagSpeed) * Time.deltaTime;
                break;
        }

        // 方向転換
        if (zigzagType == ZigzagType.Linear && Mathf.Abs(newPosition.x - startPosition.x) > zigzagWidth)
        {
            zigzagDirection *= -1;
        }

        transform.position = newPosition;
    }

    void FollowTarget()
    {
        if (target == null) return;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        switch (followType)
        {
            case FollowType.Circle:
                PerformCircleFollow(directionToTarget, distanceToTarget);
                break;
            case FollowType.Orbit:
                PerformOrbitFollow();
                break;
            case FollowType.Weave:
                PerformWeaveFollow(directionToTarget);
                break;
            case FollowType.Stalk:
                PerformStalkFollow(directionToTarget, distanceToTarget);
                break;
        }
    }

    void PerformCircleFollow(Vector3 directionToTarget, float distanceToTarget)
    {
        if (distanceToTarget > followDistance)
        {
            transform.position += directionToTarget * moveSpeed * Time.deltaTime;
        }
        else if (distanceToTarget < avoidanceRadius)
        {
            transform.position -= directionToTarget * moveSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 perpendicular = Vector3.Cross(directionToTarget, Vector3.up).normalized;
            transform.position += perpendicular * moveSpeed * Time.deltaTime;
        }
    }

    void PerformOrbitFollow()
    {
        Vector3 orbitCenter = target.position;
        Vector3 orbitDirection = (transform.position - orbitCenter).normalized;
        Vector3 tangent = Vector3.Cross(orbitDirection, Vector3.up).normalized;

        transform.position += tangent * orbitSpeed * Time.deltaTime;
    }

    void PerformWeaveFollow(Vector3 directionToTarget)
    {
        Vector3 weaveOffset = Vector3.Cross(directionToTarget, Vector3.up) *
                             Mathf.Sin(timer * 3f) * weaveAmplitude;
        Vector3 targetPosition = target.position + weaveOffset;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                                                moveSpeed * Time.deltaTime);
    }

    void PerformStalkFollow(Vector3 directionToTarget, float distanceToTarget)
    {
        if (distanceToTarget > followDistance * 1.5f)
        {
            transform.position += directionToTarget * moveSpeed * 1.5f * Time.deltaTime;
        }
        else if (distanceToTarget < followDistance * 0.5f)
        {
            transform.position -= directionToTarget * moveSpeed * 0.5f * Time.deltaTime;
        }
    }

    void MoveInSpiral()
    {
        float radius = circleRadius * (1 - timer * 0.1f);
        radius = Mathf.Max(radius, 0.5f);

        float x = startPosition.x + Mathf.Cos(timer * circleSpeed) * radius;
        float z = startPosition.z + Mathf.Sin(timer * circleSpeed) * radius;
        float y = startPosition.y + timer * moveSpeed * 0.3f;

        transform.position = new Vector3(x, y, z);
    }

    void PerformDive()
    {
        if (target != null)
        {
            Vector3 diveDirection = (target.position - transform.position).normalized;
            transform.position += diveDirection * moveSpeed * 2f * Time.deltaTime;
        }
        else
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }
    }

    void PerformSpin()
    {
        transform.Rotate(Vector3.up * rotationSpeed * 2f * Time.deltaTime);

        Vector3 spinDirection = transform.forward;
        transform.position += spinDirection * moveSpeed * Time.deltaTime;
    }

    void PerformBounce()
    {
        float bounceHeight = Mathf.Abs(Mathf.Sin(timer * 4f)) * waveAmplitude;
        Vector3 newPosition = transform.position;
        newPosition.y = startPosition.y + bounceHeight;
        newPosition.z += moveSpeed * Time.deltaTime;

        transform.position = newPosition;
    }

    void PerformHover()
    {
        float hoverOffset = Mathf.Sin(timer * 2f) * 0.5f;
        Vector3 hoverPosition = startPosition + Vector3.up * hoverOffset;

        transform.position = Vector3.Lerp(transform.position, hoverPosition,
                                         Time.deltaTime * 2f);
    }

    void PerformRandomMovement()
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        transform.position += randomDirection * moveSpeed * Time.deltaTime;

        // 開始位置から離れすぎないようにする
        if (Vector3.Distance(transform.position, startPosition) > circleRadius * 2f)
        {
            Vector3 returnDirection = (startPosition - transform.position).normalized;
            transform.position += returnDirection * moveSpeed * Time.deltaTime;
        }
    }

    void RotateTowardsMovement()
    {
        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                                  rotationSpeed * Time.deltaTime);
        }
    }

    [Button("動きパターンを変更")]
    public void ChangeMovementPattern(EnemyType newType)
    {
        enemyType = newType;
        startPosition = transform.position;
        timer = 0f;
        hasPassed = false;
        isReturning = false;
    }

    [Button("位置をリセット")]
    public void ResetPosition()
    {
        transform.position = startPosition;
        timer = 0f;
        hasPassed = false;
        isReturning = false;
    }

    [Button("ランダムパターンに切り替え")]
    public void SetRandomPattern()
    {
        EnemyType[] types = System.Enum.GetValues(typeof(EnemyType)) as EnemyType[];
        EnemyType randomType = types[Random.Range(0, types.Length - 1)]; // Randomを除く
        ChangeMovementPattern(randomType);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawMovementPath) return;

        Gizmos.color = Color.yellow;

        switch (enemyType)
        {
            case EnemyType.Circling:
                if (useEllipse)
                {
                    DrawEllipse(startPosition, ellipseWidth, ellipseHeight);
                }
                else
                {
                    Gizmos.DrawWireSphere(startPosition, circleRadius);
                }
                break;

            case EnemyType.Following:
                if (target != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(target.position, followDistance);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(target.position, avoidanceRadius);
                }
                break;

            case EnemyType.ZigZag:
                Gizmos.color = Color.green;
                Vector3 leftPoint = startPosition + Vector3.left * zigzagWidth;
                Vector3 rightPoint = startPosition + Vector3.right * zigzagWidth;
                Gizmos.DrawLine(leftPoint, rightPoint);
                break;
        }

        // 現在の状態表示
        if (showCurrentState)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.2f);

            if (isTurboActive)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }

    void DrawEllipse(Vector3 center, float width, float height)
    {
        int segments = 32;
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(width, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle = i * 2f * Mathf.PI / segments;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * width,
                0,
                Mathf.Sin(angle) * height
            );
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }

    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"敵タイプ: {enemyType}");
            GUI.Label(new Rect(10, 30, 200, 20), $"速度: {velocity.magnitude:F2}");
            GUI.Label(new Rect(10, 50, 200, 20), $"ターボ: {isTurboActive}");
            GUI.Label(new Rect(10, 70, 200, 20), $"通過済み: {hasPassed}");

            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                GUI.Label(new Rect(10, 90, 200, 20), $"距離: {distance:F2}");
            }
        }
    }
}
