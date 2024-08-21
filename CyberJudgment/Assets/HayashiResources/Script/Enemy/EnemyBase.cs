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
    [SerializeField]
    private float _currentHealth;


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
        _currentState.UpdateState(this);
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
        Vector3 direction = (targetPosition - transform.position).normalized;
        _rb.MovePosition(transform.position + direction * enemyData.moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// �ڕW�n�_�Ɍ������ĉ�]���邽�߂̃��\�b�h
    /// </summary>
    /// <param name="targetPosition">�ڕW�n�_�̍��W</param>
    public void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 360f));
    }

    /// <summary>
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�󂯂�_���[�W��</param>
    public virtual void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // �_���[�W���󂯂��ۂ̃A�j���[�V�����⃊�A�N�V�����������ɒǉ�
            _animator.SetBool("TakeDamage",true);
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
        // ���S�A�j���[�V�����Đ�
        _animator.CrossFade("Die",0.05f);
    }

    protected virtual void DieEnd()
    {
        Destroy(this.gameObject);
    }

    /// <summary>
    /// �A�j���[�V�������I�����A�ҋ@��ԂɑJ�ڂ��邽�߂̃��\�b�h
    /// </summary>
    public void EndAnimation()
    {
        TransitionToState(new IdleState());
    }
}
