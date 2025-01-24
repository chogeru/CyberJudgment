using UnityEngine;
using System;
using System.Linq;
using AbubuResouse.Singleton;

/// <summary>
/// 敵のベースクラス (基底クラス)
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("敵のデータ")]
    [SerializeField] public EnemyData enemyData;

    [Header("死亡時エフェクト")]
    [SerializeField] private GameObject _dieEffect;

    [Header("被弾時Voice")]
    [SerializeField] private string _getHitVoiceSE;

    [Header("死亡Voice")]
    [SerializeField] private string _dieVoiceSE;

    [Header("SE音量")]
    [SerializeField] private float _volume = 1f;

    public Transform _player { get; private set; }

    public bool isPlayerInSight { get; protected set; }

    protected bool isCollidingWithPlayer = false;

    private Collider _collider;
    public Rigidbody _rb { get; private set; }
    public Animator _animator { get; private set; }

    private IEnemyState _currentState;

    [Header("現在のHP")]
    [SerializeField] public float _currentHealth;
    [SerializeField] private float _maxHealth;
    public event Action<float, float> OnHealthChanged;

    private bool isAttacking = false;

    private bool isDie = false;

    private float _nextAttackTime = 0f;

    [Header("HPバー設定")]
    [SerializeField] private GameObject hpBarPrefab;
    private EnemyHPBar hpBar;

    /// <summary>
    /// 現在攻撃できるか判定
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= _nextAttackTime;
    }

    /// <summary>
    /// 攻撃クールダウンを開始 (現在時刻 + attackCooldown)
    /// </summary>
    public void ResetAttackCooldown()
    {
        _nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Start()
    {
        _currentHealth = UnityEngine.Random.Range(enemyData.minHealth, enemyData.maxHealth + 1);
        _maxHealth = _currentHealth;

        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        if (hpBarPrefab != null)
        {
            hpBar = hpBarPrefab.GetComponent<EnemyHPBar>();
            if (hpBar != null)
            {
                hpBar.UpdateHPBar(_currentHealth, _maxHealth, enemyData.enemyName); // 名前も渡す
                hpBar.SetVisibility(true);
            }
        }
        else
        {
            Debug.LogError("hpBarPrefabがアサインされていません。Inspectorで設定してください。");
        }
        // 開始時は IdleState
        _currentState = new IdleState();
        _currentState.EnterState(this);
    }

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    protected virtual void Update()
    {
        if (!isDie)
        {
            _currentState.UpdateState(this);
        }

        // 攻撃アニメではない場合は isAttacking を解除
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            !_animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            SetIsAttacking(false);
        }

        DetectPlayer();
    }

    /// <summary>
    /// ステート切り替え
    /// </summary>
    public void TransitionToState(IEnemyState newState)
    {
        Debug.Log($"{this.name}: Transition from {_currentState.GetType().Name} to {newState.GetType().Name}");
        _currentState.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    /// <summary>
    /// パトロール動作は派生クラスで実装
    /// </summary>
    public abstract void Patrol();

    /// <summary>
    /// プレイヤーを探知する
    /// </summary>
    protected void DetectPlayer()
    {
        if (_player == null) return;

        Vector3 directionToPlayer = _player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= enemyData.detectionRange)
        {
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position + Vector3.up * 1f;

            // Raycastで視界チェック
            if (Physics.Raycast(raycastOrigin, directionToPlayer.normalized, out hit, enemyData.visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (!isPlayerInSight)
                    {
                        Debug.Log($"{this.name}: Player detected");
                    }
                    isPlayerInSight = true;

                    // まだChase/Attack系ステートでない場合はChaseへ
                    if (!(_currentState is ChaseState) &&
                        !(_currentState is AttackState) &&
                        !(_currentState is StrongAttackState))
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
    /// targetPositionに向かって移動
    /// </summary>
    public void MoveTowards(Vector3 targetPosition)
    {
        if (GetIsAttacking() || isCollidingWithPlayer)
        {
            _animator.SetBool("isMoving", false);
            return;
        }

        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction.magnitude > 0.1f)
            _animator.SetBool("isMoving", true);
        else
            _animator.SetBool("isMoving", false);

        _rb.MovePosition(transform.position + direction * enemyData.moveSpeed * Time.deltaTime);
    }


    /// <summary>
    /// targetPositionの方向へ回転
    /// </summary>
    public void RotateTowards(Vector3 targetPosition)
    {
        if (GetIsAttacking()) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            _rb.MoveRotation(newRotation);
        }
    }

    /// <summary>
    /// 攻撃中フラグ Getter/Setter
    /// </summary>
    public void SetIsAttacking(bool value)
    {
        isAttacking = value;
    }
    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// Attackアニメ終了時に呼ばれる想定(アニメイベントなど)
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
    /// ダメージを受ける
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDie) return;

        _currentHealth -= damage;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (hpBar != null)
        {
            hpBar.UpdateHPBar(_currentHealth, _maxHealth, enemyData.enemyName); // 名前も渡す
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 被弾SE
            if (!string.IsNullOrEmpty(_getHitVoiceSE))
                SEManager.Instance.PlaySound(_getHitVoiceSE, _volume);

            // ダメージアニメ再生用トリガー
            _animator.SetTrigger("TakeDamage");

            // 攻撃フラグ下げ
            SetIsAttacking(false);
            _animator.SetBool("Attack", false);
            _animator.SetBool("StrongAttack", false);
        }
    }

    /// <summary>
    /// ダメージアニメが終わったタイミングで呼ばれる想定
    /// </summary>
    protected virtual void HitEnd()
    {
        // Idleへ
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    protected virtual void Die()
    {
        isDie = true;

        // 死亡SE
        if (!string.IsNullOrEmpty(_dieVoiceSE))
            SEManager.Instance.PlaySound(_dieVoiceSE, _volume);

        // 死亡アニメへ
        _animator.CrossFade("Die", 0.05f);

        // 物理・衝突を無効化
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
    /// 死亡アニメ終了 (アニメイベント) 
    /// </summary>
    protected virtual void DieEnd()
    {
        // エフェクト生成
        if (_dieEffect != null)
            Instantiate(_dieEffect, transform.position, Quaternion.identity);

        // アイテムドロップ
        DropItem();

        // 本体破棄
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 攻撃アニメやダメージアニメ終了後、強制Idleへ
    /// </summary>
    public void EndAnimation()
    {
        SetIsAttacking(false);
        _animator.SetBool("Attack", false);
        _animator.SetBool("StrongAttack", false);
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// Attack/StrongAttackアニメイベント終了時に呼ばれる想定
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        if (_currentState is AttackState || _currentState is StrongAttackState)
        {
            TransitionToState(new RetreatState());
        }
    }

    /// <summary>
    /// アイテムドロップ処理
    /// </summary>
    protected virtual void DropItem()
    {
        // 通常アイテムのドロップ
        foreach (var dropInfo in enemyData.dropItemInfos)
        {
            float randValue = UnityEngine.Random.Range(0f, 100f);
            if (randValue <= dropInfo.dropRate)
            {
                int dropCount = UnityEngine.Random.Range(dropInfo.minDropCount, dropInfo.maxDropCount + 1);
                for (int i = 0; i < dropCount; i++)
                {
                    Vector3 spawnPosition = transform.position + Vector3.up * 1.0f;
                    spawnPosition += UnityEngine.Random.insideUnitSphere * 0.5f;

                    GameObject dropItem = Instantiate(
                        dropInfo.itemPrefab,
                        spawnPosition,
                        Quaternion.identity
                    );

                    Rigidbody rb = dropItem.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 forceDirection = (Vector3.up + new Vector3(
                            UnityEngine.Random.Range(-0.5f, 0.5f),
                            0,
                            UnityEngine.Random.Range(-0.5f, 0.5f))
                        ).normalized;

                        float forceMagnitude = UnityEngine.Random.Range(2f, 5f);
                        rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
                    }
                }
            }
        }

        // 武器ドロップ (レアリティ抽選)
        WeaponDropInfo weaponDrop = enemyData.weaponDropInfo;
        float weaponRandValue = UnityEngine.Random.Range(0f, 100f);
        if (weaponRandValue <= weaponDrop.dropRate)
        {
            WeaponRarity selectedRarity = DetermineWeaponRarity(weaponDrop);

            GameObject rarityOrbPrefab = GetRarityOrbPrefab(selectedRarity);
            if (rarityOrbPrefab != null)
            {
                Vector3 orbSpawnPosition = transform.position + Vector3.up * 1.0f;
                orbSpawnPosition += UnityEngine.Random.insideUnitSphere * 0.5f;

                Instantiate(rarityOrbPrefab, orbSpawnPosition, Quaternion.identity);
                Debug.Log($"レアリティ {selectedRarity} の武器オーブがドロップしました。");
            }
        }
    }

    private WeaponRarity DetermineWeaponRarity(WeaponDropInfo weaponDrop)
    {
        float rand = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;
        foreach (var rarityProb in weaponDrop.rarityProbabilities)
        {
            cumulative += rarityProb.probability;
            if (rand <= cumulative)
                return rarityProb.rarity;
        }
        return WeaponRarity.Common;
    }

    private GameObject GetRarityOrbPrefab(WeaponRarity rarity)
    {
        var rarityData = enemyData.weaponDropInfo.rarityWeaponPrefabs
            .FirstOrDefault(rp => rp.rarity == rarity);
        if (rarityData != null && rarityData.rarityOrbPrefab != null)
        {
            return rarityData.rarityOrbPrefab;
        }
        return null;
    }

    private void OnAnimatorMove()
    {
        // 必要に応じてルートモーション処理
    }

    /// <summary>
    /// プレイヤーとぶつかったらフラグを立てる
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;
        }
    }

    /// <summary>
    /// プレイヤーとの接触が離れたらフラグを下げる
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = false;
        }
    }
    /// <summary>
    /// HPが変化したときに呼び出される
    /// </summary>
    private void OnEnable()
    {
        OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (hpBar != null)
        {
            hpBar.UpdateHPBar(currentHealth, maxHealth, enemyData.enemyName); // 名前も渡す
        }
    }
}
