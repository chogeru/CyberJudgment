using AbubuResouse.Singleton;
using System;
using UnityEngine;

/// <summary>
/// �G�̊�{������`���钊�ۃN���X
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("�G�̃f�[�^")]
    [SerializeField] public EnemyData enemyData;

    [Header("���S���G�t�F�N�g")]
    [SerializeField] private GameObject _dieEffect;

    [Header("��_���[�W����Voice")]
    [SerializeField] private string _getHitVoiceSE;

    [Header("���S��Voice")]
    [SerializeField] private string _dieVoiceSE;

    [Header("����")]
    [SerializeField] private float _volume = 1f;

    // �v���C���[��Transform
    public Transform _player { get; private set; }

    // �v���C���[�����E�ɓ����Ă��邩
    public bool isPlayerInSight { get; private set; }

    // Rigidbody �� Animator

    private Collider _collider;
    public Rigidbody _rb { get; private set; }
    public Animator _animator { get; private set; }

    // ���݂̃X�e�[�g
    private IEnemyState _currentState;

    [Header("���݂�Hp")]
    [SerializeField] public float _currentHealth;
    public event Action<float, float> OnHealthChanged;

    // �U���t���O
    private bool isAttacking = false;

    // ���S�t���O
    private bool isDie = false;

    /// <summary>
    /// �G�̏����ݒ�
    /// </summary>
    protected virtual void Start()
    {
        // ����HP�ݒ�
        _currentHealth = enemyData.health;

        // �v���C���[�̎擾
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        // Rigidbody��Animator�̎擾
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _animator = GetComponent<Animator>();

        // �����X�e�[�g�̐ݒ�
        _currentState = new IdleState();
        _currentState.EnterState(this);
    }

    /// <summary>
    /// ���t���[���̍X�V����
    /// </summary>
    protected virtual void Update()
    {
        if (!isDie)
        {
            _currentState.UpdateState(this);
        }

        // �U���A�j���[�V�������Đ�����Ă��Ȃ���΍U����Ԃ�����
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            !_animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            SetIsAttacking(false);
        }

        // �v���C���[�̌��o
        DetectPlayer();
    }

    /// <summary>
    /// �X�e�[�g��J�ڂ��郁�\�b�h
    /// </summary>
    /// <param name="newState">�J�ڂ���V�����X�e�[�g</param>
    public void TransitionToState(IEnemyState newState)
    {
        Debug.Log($"{this.name}: Transitioning from {_currentState.GetType().Name} to {newState.GetType().Name}");
        _currentState.ExitState(this);
        _currentState = newState;
        _currentState.EnterState(this);
    }

    /// <summary>
    /// ���񓮍���������钊�ۃ��\�b�h
    /// </summary>
    public abstract void Patrol();

    /// <summary>
    /// �v���C���[�����o���邽�߂̃��\�b�h
    /// </summary>
    protected void DetectPlayer()
    {
        Vector3 directionToPlayer = _player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= enemyData.detectionRange)
        {
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position + Vector3.up * 1f; // ����1�̈ʒu���烌�C�L���X�g�𔭎�

            if (Physics.Raycast(raycastOrigin, directionToPlayer.normalized, out hit, enemyData.visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (!isPlayerInSight)
                    {
                        Debug.Log($"{this.name}: Player detected");
                    }
                    isPlayerInSight = true;

                    // ���ɒǐՒ��A�U�����łȂ���� ChaseState �ɑJ��
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
    /// �ڕW�n�_�Ɍ������Ĉړ����郁�\�b�h
    /// </summary>
    /// <param name="targetPosition">�ڕW�n�_�̍��W</param>
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
    /// �ڕW�n�_�Ɍ������ĉ�]���郁�\�b�h
    /// </summary>
    /// <param name="targetPosition">�ڕW�n�_�̍��W</param>
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
    /// �U���t���O��ݒ肷�郁�\�b�h
    /// </summary>
    /// <param name="value">�ݒ肷��l</param>
    public void SetIsAttacking(bool value)
    {
        isAttacking = value;
    }

    /// <summary>
    /// �U���t���O���擾���郁�\�b�h
    /// </summary>
    /// <returns>�U���t���O�̒l</returns>
    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// �U���������������̏���
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
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�󂯂�_���[�W��</param>
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
    /// �_���[�W�A�j���[�V�����I����ɌĂяo��
    /// </summary>
    protected virtual void HitEnd()
    {
        // �A�j���[�V�����C�x���g�ŌĂяo��
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// �G�����S�����ۂ̏���
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
    /// ���S�A�j���[�V�����I����ɌĂяo��
    /// </summary>
    protected virtual void DieEnd()
    {
        Instantiate(_dieEffect, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// �A�j���[�V�����I����ɌĂяo���AIdleState�ɑJ�ڂ��郁�\�b�h
    /// </summary>
    public void EndAnimation()
    {
        SetIsAttacking(false);
        _animator.SetBool("Attack", false);
        _animator.SetBool("StrongAttack", false);
        TransitionToState(new IdleState());
    }

    /// <summary>
    /// �A�j���[�V�����C�x���g����Ăяo�����\�b�h
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        if (_currentState is AttackState || _currentState is StrongAttackState)
        {
            TransitionToState(new RetreatState());
        }
    }

    /// <summary>
    /// �A�C�e���h���b�v�̎���
    /// </summary>
    protected virtual void DropItem()
    {
    }

    private void OnAnimatorMove()
    {
    }
}
