using UnityEngine;
using System;
using System.Linq;
using AbubuResouse.Singleton;

/// <summary>
/// �G�̃x�[�X�N���X (���N���X)
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("�G�̃f�[�^")]
    [SerializeField] public EnemyData enemyData;

    [Header("���S���G�t�F�N�g")]
    [SerializeField] private GameObject _dieEffect;

    [Header("��e��Voice")]
    [SerializeField] private string _getHitVoiceSE;

    [Header("���SVoice")]
    [SerializeField] private string _dieVoiceSE;

    [Header("SE����")]
    [SerializeField] private float _volume = 1f;

    public Transform _player { get; private set; }

    public bool isPlayerInSight { get; protected set; }

    protected bool isCollidingWithPlayer = false;

    private Collider _collider;
    public Rigidbody _rb { get; private set; }
    public Animator _animator { get; private set; }

    private IEnemyState _currentState;

    [Header("���݂�HP")]
    [SerializeField] public float _currentHealth;
    [SerializeField] private float _maxHealth;
    public event Action<float, float> OnHealthChanged;

    private bool isAttacking = false;

    private bool isDie = false;

    private float _nextAttackTime = 0f;

    [Header("HP�o�[�ݒ�")]
    [SerializeField] private GameObject hpBarPrefab;
    private EnemyHPBar hpBar;

    /// <summary>
    /// ���ݍU���ł��邩����
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= _nextAttackTime;
    }

    /// <summary>
    /// �U���N�[���_�E�����J�n (���ݎ��� + attackCooldown)
    /// </summary>
    public void ResetAttackCooldown()
    {
        _nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    /// <summary>
    /// ������
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
                hpBar.UpdateHPBar(_currentHealth, _maxHealth, enemyData.enemyName); // ���O���n��
                hpBar.SetVisibility(true);
            }
        }
        else
        {
            Debug.LogError("hpBarPrefab���A�T�C������Ă��܂���BInspector�Őݒ肵�Ă��������B");
        }
        // �J�n���� IdleState
        _currentState = new IdleState();
        _currentState.EnterState(this);
    }

    /// <summary>
    /// ���t���[���X�V
    /// </summary>
    protected virtual void Update()
    {
        if (!isDie)
        {
            _currentState.UpdateState(this);
        }

        // �U���A�j���ł͂Ȃ��ꍇ�� isAttacking ������
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            !_animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            SetIsAttacking(false);
        }

        DetectPlayer();
    }

    /// <summary>
    /// �X�e�[�g�؂�ւ�
    /// </summary>
    public void TransitionToState(IEnemyState newState)
    {
        Debug.Log($"{this.name}: Transition from {_currentState.GetType().Name} to {newState.GetType().Name}");
        _currentState.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    /// <summary>
    /// �p�g���[������͔h���N���X�Ŏ���
    /// </summary>
    public abstract void Patrol();

    /// <summary>
    /// �v���C���[��T�m����
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

            // Raycast�Ŏ��E�`�F�b�N
            if (Physics.Raycast(raycastOrigin, directionToPlayer.normalized, out hit, enemyData.visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (!isPlayerInSight)
                    {
                        Debug.Log($"{this.name}: Player detected");
                    }
                    isPlayerInSight = true;

                    // �܂�Chase/Attack�n�X�e�[�g�łȂ��ꍇ��Chase��
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
    /// targetPosition�Ɍ������Ĉړ�
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
    /// targetPosition�̕����։�]
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
    /// �U�����t���O Getter/Setter
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
    /// Attack�A�j���I�����ɌĂ΂��z��(�A�j���C�x���g�Ȃ�)
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
    /// �_���[�W���󂯂�
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDie) return;

        _currentHealth -= damage;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (hpBar != null)
        {
            hpBar.UpdateHPBar(_currentHealth, _maxHealth, enemyData.enemyName); // ���O���n��
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ��eSE
            if (!string.IsNullOrEmpty(_getHitVoiceSE))
                SEManager.Instance.PlaySound(_getHitVoiceSE, _volume);

            // �_���[�W�A�j���Đ��p�g���K�[
            _animator.SetTrigger("TakeDamage");

            // �U���t���O����
            SetIsAttacking(false);
            _animator.SetBool("Attack", false);
            _animator.SetBool("StrongAttack", false);
        }
    }

    /// <summary>
    /// �_���[�W�A�j�����I������^�C�~���O�ŌĂ΂��z��
    /// </summary>
    protected virtual void HitEnd()
    {
        // Idle��
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// ���S����
    /// </summary>
    protected virtual void Die()
    {
        isDie = true;

        // ���SSE
        if (!string.IsNullOrEmpty(_dieVoiceSE))
            SEManager.Instance.PlaySound(_dieVoiceSE, _volume);

        // ���S�A�j����
        _animator.CrossFade("Die", 0.05f);

        // �����E�Փ˂𖳌���
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
    /// ���S�A�j���I�� (�A�j���C�x���g) 
    /// </summary>
    protected virtual void DieEnd()
    {
        // �G�t�F�N�g����
        if (_dieEffect != null)
            Instantiate(_dieEffect, transform.position, Quaternion.identity);

        // �A�C�e���h���b�v
        DropItem();

        // �{�̔j��
        Destroy(this.gameObject);
    }

    /// <summary>
    /// �U���A�j����_���[�W�A�j���I����A����Idle��
    /// </summary>
    public void EndAnimation()
    {
        SetIsAttacking(false);
        _animator.SetBool("Attack", false);
        _animator.SetBool("StrongAttack", false);
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// Attack/StrongAttack�A�j���C�x���g�I�����ɌĂ΂��z��
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        if (_currentState is AttackState || _currentState is StrongAttackState)
        {
            TransitionToState(new RetreatState());
        }
    }

    /// <summary>
    /// �A�C�e���h���b�v����
    /// </summary>
    protected virtual void DropItem()
    {
        // �ʏ�A�C�e���̃h���b�v
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

        // ����h���b�v (���A���e�B���I)
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
                Debug.Log($"���A���e�B {selectedRarity} �̕���I�[�u���h���b�v���܂����B");
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
        // �K�v�ɉ����ă��[�g���[�V��������
    }

    /// <summary>
    /// �v���C���[�ƂԂ�������t���O�𗧂Ă�
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;
        }
    }

    /// <summary>
    /// �v���C���[�Ƃ̐ڐG�����ꂽ��t���O��������
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = false;
        }
    }
    /// <summary>
    /// HP���ω������Ƃ��ɌĂяo�����
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
            hpBar.UpdateHPBar(currentHealth, maxHealth, enemyData.enemyName); // ���O���n��
        }
    }
}
