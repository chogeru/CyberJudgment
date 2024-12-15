using UnityEngine;

/// <summary>
/// �G�̎����N���X
/// </summary>
public class Enemy : EnemyBase
{
    [Header("����|�C���g�̔z��")]
    [SerializeField] private Transform[] _patrolPoints;

    // ���݂̏���|�C���g
    private int _currentPatrolIndex = 0;

    // ����|�C���g�őҋ@����
    private float _patrolTimer = 0f;

    /// <summary>
    /// �G�̏����ݒ�
    /// </summary>
    protected override void Start()
    {
        base.Start();
        _currentPatrolIndex = 0;
        _patrolTimer = 0f;
    }

    /// <summary>
    /// ���񓮍�̎������\�b�h
    /// </summary>
    public override void Patrol()
    {
        if (_patrolPoints.Length == 0) return;

        Transform targetPatrolPoint = _patrolPoints[_currentPatrolIndex];
        MoveTowards(targetPatrolPoint.position);
        RotateTowards(targetPatrolPoint.position);

        // �ڕW�n�_�ɋ߂Â�����ҋ@
        if (Vector3.Distance(transform.position, targetPatrolPoint.position) < 0.5f)
        {
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= enemyData.patrolPointWaitTime)
            {
                _patrolTimer = 0f;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
            }
        }
    }
}
