using UnityEngine;

/// <summary>
/// �G�̎����N���X
/// </summary>
public class Enemy : EnemyBase
{
    [SerializeField,Header("����|�C���g�̔z��")]
    private Transform[] _patrolPoints;
    //���݂̏���|�C���g
    private int _currentPatrolIndex;
    //����|�C���g�őҋ@����
    private float _patrolTimer;

    /// <summary>
    /// �G�̏����ݒ�
    /// </summary>
    protected override void Start()
    {
        base.Start();
        _currentPatrolIndex = 0;
        _patrolTimer = 0f;
        TransitionToState(new IdleState());
    }

    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// ���񓮍�̎������\�b�h
    /// </summary>
    public override void Patrol()
    {
        if (_patrolPoints.Length == 0) return;

        Transform targetPatrolPoint = _patrolPoints[_currentPatrolIndex];
        MoveTowards(targetPatrolPoint.position);

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
