using AbubuResouse.Singleton;
using System;
using UnityEngine;

/// <summary>
/// 敵の基本動作を定義する抽象クラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("敵のデータ")]
    [SerializeField] public EnemyData enemyData;

    [Header("死亡時エフェクト")]
    [SerializeField] private GameObject _dieEffect;

    [Header("被ダメージ時のVoice")]
    [SerializeField] private string _getHitVoiceSE;

    [Header("死亡時Voice")]
    [SerializeField] private string _dieVoiceSE;

    [Header("音量")]
    [SerializeField] private float _volume = 1f;

    // プレイヤーのTransform
    public Transform _player { get; private set; }

    // プレイヤーが視界に入っているか
    public bool isPlayerInSight { get; private set; }

    // Rigidbody と Animator

    private Collider _collider;
    public Rigidbody _rb { get; private set; }
    public Animator _animator { get; private set; }

    // 現在のステート
    private IEnemyState _currentState;

    [Header("現在のHp")]
    [SerializeField] public float _currentHealth;
    public event Action<float, float> OnHealthChanged;

    // 攻撃フラグ
    private bool isAttacking = false;

    // 死亡フラグ
    private bool isDie = false;

    /// <summary>
    /// 敵の初期設定
    /// </summary>
    protected virtual void Start()
    {
        // 初期HP設定
        _currentHealth = enemyData.health;

        // プレイヤーの取得
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        // RigidbodyとAnimatorの取得
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _animator = GetComponent<Animator>();

        // 初期ステートの設定
        _currentState = new IdleState();
        _currentState.EnterState(this);
    }

    /// <summary>
    /// 毎フレームの更新処理
    /// </summary>
    protected virtual void Update()
    {
        if (!isDie)
        {
            _currentState.UpdateState(this);
        }

        // 攻撃アニメーションが再生されていなければ攻撃状態を解除
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            !_animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            SetIsAttacking(false);
        }

        // プレイヤーの検出
        DetectPlayer();
    }

    /// <summary>
    /// ステートを遷移するメソッド
    /// </summary>
    /// <param name="newState">遷移する新しいステート</param>
    public void TransitionToState(IEnemyState newState)
    {
        Debug.Log($"{this.name}: Transitioning from {_currentState.GetType().Name} to {newState.GetType().Name}");
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
            Vector3 raycastOrigin = transform.position + Vector3.up * 1f; // 高さ1の位置からレイキャストを発射

            if (Physics.Raycast(raycastOrigin, directionToPlayer.normalized, out hit, enemyData.visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (!isPlayerInSight)
                    {
                        Debug.Log($"{this.name}: Player detected");
                    }
                    isPlayerInSight = true;

                    // 既に追跡中、攻撃中でなければ ChaseState に遷移
                    if (!(_currentState is ChaseState) && !(_currentState is AttackState) && !(_currentState is StrongAttackState))
                    {
                        TransitionToState(new ChaseState());
                    }
                    return;
                }
            }
        }

        if (isPlayerInSight)
        {
            Debug.Log($"{this.name}: Player lost");
        }
        isPlayerInSight = false;
    }

    /// <summary>
    /// 目標地点に向かって移動するメソッド
    /// </summary>
    /// <param name="targetPosition">目標地点の座標</param>
    public void MoveTowards(Vector3 targetPosition)
    {
        if (GetIsAttacking())
        {
            return;
        }

        Vector3 direction = (targetPosition - transform.position).normalized;
        _rb.MovePosition(transform.position + direction * enemyData.moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 目標地点に向かって回転するメソッド
    /// </summary>
    /// <param name="targetPosition">目標地点の座標</param>
    public void RotateTowards(Vector3 targetPosition)
    {
        if (GetIsAttacking())
        {
            return;
        }

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            _rb.MoveRotation(newRotation);
        }
    }

    /// <summary>
    /// 攻撃フラグを設定するメソッド
    /// </summary>
    /// <param name="value">設定する値</param>
    public void SetIsAttacking(bool value)
    {
        isAttacking = value;
    }

    /// <summary>
    /// 攻撃フラグを取得するメソッド
    /// </summary>
    /// <returns>攻撃フラグの値</returns>
    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// 攻撃が完了した時の処理
    /// </summary>
    public void AttackFinished()
    {
        float distanceToPlayer = Vector3.Distance(_player.position, transform.position);

        if (distanceToPlayer <= enemyData.attackRange)
        {
            TransitionToState(new ChaseState());
        }
        else
        {
            TransitionToState(new IdleState());
        }
    }

    /// <summary>
    /// ダメージを受けた時の処理
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public virtual void TakeDamage(float damage)
    {
        if (isDie)
            return;

        _currentHealth -= damage;
        OnHealthChanged?.Invoke(_currentHealth, enemyData.health);

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            SEManager.Instance.PlaySound(_getHitVoiceSE, _volume);
            _animator.SetBool("TakeDamage", true);
            SetIsAttacking(false);
            _animator.SetBool("Attack", false);
            _animator.SetTrigger("StrongAttack");
        }
    }

    /// <summary>
    /// ダメージアニメーション終了後に呼び出す
    /// </summary>
    protected virtual void HitEnd()
    {
        // アニメーションイベントで呼び出す
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// 敵が死亡した際の処理
    /// </summary>
    protected virtual void Die()
    {
        isDie = true;
        SEManager.Instance.PlaySound(_dieVoiceSE, _volume);
        _animator.CrossFade("Die", 0.05f);
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.detectCollisions = false;
        }

        if (_collider != null)
        {
            _collider.enabled = false;
        }

    }

    /// <summary>
    /// 死亡アニメーション終了後に呼び出す
    /// </summary>
    protected virtual void DieEnd()
    {
        Instantiate(_dieEffect, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// アニメーション終了後に呼び出し、IdleStateに遷移するメソッド
    /// </summary>
    public void EndAnimation()
    {
        SetIsAttacking(false);
        _animator.SetBool("Attack", false);
        _animator.SetBool("StrongAttack", false);
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// アニメーションイベントから呼び出すメソッド
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        if (_currentState is AttackState || _currentState is StrongAttackState)
        {
            TransitionToState(new RetreatState());
        }
    }

    /// <summary>
    /// アイテムドロップの実装
    /// </summary>
    protected virtual void DropItem()
    {
    }

    private void OnAnimatorMove()
    {
    }
}
