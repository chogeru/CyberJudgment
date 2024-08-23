using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// �G�̊�{������`���钊�ۃN���X
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField,Header("�G�̃f�[�^")] 
    public EnemyData enemyData;

    [SerializeField]
    public Transform _player { get; private set; }
    [SerializeField]
    public bool isPlayerInSight { get; private set; }
    public Rigidbody _rb { get; private set; }
    public Animator _animator { get; private set; }

    private IEnemyState _currentState;
    [SerializeField,Header("���݂�Hp")]
    private float _currentHealth;

    [SerializeField, Header("��_���[�W����Voice")]
    private string _getHitVoiceSE;
    [SerializeField,Header("���S��Voice")]
    private string _dieVoiceSE;
    [SerializeField, Header("����")]
    private float _volume;

    [SerializeField, Header("���S���G�t�F�N�g")]
    private GameObject _dieEffect;

    [SerializeField]
    private bool isAttacking = false;
    private bool isDie=false;

    /// <summary>
    /// �G�̏����ݒ�
    /// </summary>
    protected virtual void Start()
    {
        _currentHealth = enemyData.health;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _currentState = new IdleState();
        _currentState.EnterState(this);
    }

    protected virtual void Update()
    {
        if (!isDie) 
        {
            _currentState.UpdateState(this);
        }
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
          !_animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            SetIsAttacking(false);
        }
        DetectPlayer();
    }

    /// <summary>
    /// ��Ԃ�J�ڂ��邽�߂̃��\�b�h
    /// </summary>
    /// <param name="newState">�J�ڂ���V�������</param>
    public void TransitionToState(IEnemyState newState)
    {
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
            Vector3 raycastOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); // ����1�̈ʒu���烌�C�L���X�g�𔭎�
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
    /// �ڕW�n�_�Ɍ������Ĉړ����邽�߂̃��\�b�h
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
    /// �ڕW�n�_�Ɍ������ĉ�]���邽�߂̃��\�b�h
    /// </summary>
    /// <param name="targetPosition">�ڕW�n�_�̍��W</param>
    public void RotateTowards(Vector3 targetPosition)
    {
        if (GetIsAttacking())
        {
            return;
        }
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 360f));
    }
    public void SetIsAttacking(bool value)
    {
        isAttacking = value;
    }

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    // �U����Ԃ𐧌䂷�邽�߂̊֐�
    public void AttackFinished()
    {
        SetIsAttacking(false);
        TransitionToState(new IdleState());
    }
    /// <summary>
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�󂯂�_���[�W��</param>
    public virtual void TakeDamage(float damage)
    {
        if(isDie)
        { return; }
        _currentHealth -= damage;
        
        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            SEManager.Instance.PlaySound(_getHitVoiceSE, _volume);
            _animator.SetBool("TakeDamage",true);

            SetIsAttacking(false);
            _animator.SetBool("Attack", false);
            _animator.SetBool("StrongAttack", false);
        }
    }

    protected virtual void HitEnd()
    {
        _animator.SetBool("TakeDamage", false);
    }
    /// <summary>
    /// �G�����S�����ۂ̏���
    /// </summary>
    protected virtual void Die()
    {
        isDie = true;
        SEManager.Instance.PlaySound(_dieVoiceSE, _volume);
        _animator.CrossFade("Die",0.05f);
    }

    protected virtual void DieEnd()
    {
        Instantiate(_dieEffect, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// �A�j���[�V�������I�����A�ҋ@��ԂɑJ�ڂ��邽�߂̃��\�b�h
    /// </summary>
    public void EndAnimation()
    {
        SetIsAttacking(false);
        _animator.SetBool("Attack", false);
        _animator.SetBool("StrongAttack", false);
        TransitionToState(new IdleState());
    }
}
