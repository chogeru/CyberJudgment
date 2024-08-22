using UnityEngine;

/// <summary>
/// �G���ҋ@������
/// </summary>
public class IdleState : IEnemyState
{
    private System.Random random = new System.Random();

    /// <summary>
    /// �ҋ@��Ԃɓ��鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɓ���G</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        enemy._animator.SetBool("Attack", false);
        enemy._animator.SetBool("StrongAttack", false);
    }

    /// <summary>
    /// �ҋ@��Ԃ��X�V���邽�߂ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɂ���G</param>
    public void UpdateState(EnemyBase enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy._player.position) <= enemy.enemyData.attackRange)
        {
            // 50%�̊m���Œʏ�U�������U����I��
            if (random.Next(0, 2) == 0)
            {
                enemy.TransitionToState(new AttackState());
            }
            else
            {
                enemy.TransitionToState(new StrongAttackState());
            }
        }
        if (enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new ChaseState());
        }
    }


    /// <summary>
    /// �ҋ@��Ԃ�ޏo���鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃ�ޏo����G</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
