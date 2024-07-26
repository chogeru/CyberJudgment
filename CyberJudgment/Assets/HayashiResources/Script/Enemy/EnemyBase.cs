using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// 敵の基本動作を定義する抽象クラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField,Header("敵のデータ")] 
    public EnemyData enemyData;

    [SerializeField]
    public Transform _player { get; private set; }
    [SerializeField]
    public bool isPlayerInSight { get; private set; }
    public Rigidbody _rb { get; private set; }
    public Animator _animator { get; private set; }

    private IEnemyState _currentState;

    /// <summary>
    /// 敵の初期設定
    /// </summary>
    protected virtual void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _currentState = new IdleState();
        _currentState.EnterState(this);
    }

    protected virtual void Update()
    {
        _currentState.UpdateState(this);
        DetectPlayer();
    }

    /// <summary>
    /// 状態を遷移するためのメソッド
    /// </summary>
    /// <param name="newState">遷移する新しい状態</param>
    public void TransitionToState(IEnemyState newState)
    {
        _currentState.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    /// <summary>
    /// 巡回動作を実装する抽象メソッド
    /// </summary>
    public abstract void Patrol();

    /// <summary>
    /// プレイヤーを検出するためのメソッド
    /// </summary>
    protected void DetectPlayer()
    {
        Vector3 directionToPlayer = _player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= enemyData.detectionRange)
        {
            RaycastHit hit;
            Vector3 raycastOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); // 高さ1の位置からレイキャストを発射
            if (Physics.Raycast(raycastOrigin, directionToPlayer.normalized, out hit, enemyData.visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isPlayerInSight = true;
                    TransitionToState(new ChaseState());
                    return;
                }
            }
        }
        isPlayerInSight = false;
    }

    /// <summary>
    /// 目標地点に向かって移動するためのメソッド
    /// </summary>
    /// <param name="targetPosition">目標地点の座標</param>
    public void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        _rb.MovePosition(transform.position + direction * enemyData.moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 目標地点に向かって回転するためのメソッド
    /// </summary>
    /// <param name="targetPosition">目標地点の座標</param>
    public void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 360f));
    }

    /// <summary>
    /// アニメーションを終了し、待機状態に遷移するためのメソッド
    /// </summary>
    public void EndAnimation()
    {
        TransitionToState(new IdleState());
    }
}
