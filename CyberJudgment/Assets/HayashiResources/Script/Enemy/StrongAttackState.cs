using UnityEngine;

/// <summary>
/// �G���v���C���[�����U��������
/// </summary>
public class StrongAttackState : IEnemyState
{
    private System.Random random = new System.Random();

    /// <summary>
    /// ���U����Ԃɓ��鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɓ���G</param>
    public void EnterState(EnemyBase enemy)
    {
        if (!enemy.GetIsAttacking())
        {
            enemy.SetIsAttacking(true);
            enemy._animator.SetBool("StrongAttack", true);
        }
    }

    /// <summary>
    /// ���U����Ԃ��X�V���邽�߂ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɂ���G</param>
    public void UpdateState(EnemyBase enemy)
    {
        if (!enemy.isPlayerInSight || Vector3.Distance(enemy.transform.position, enemy._player.position) > enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new ChaseState());
        }
        else if (enemy.isPlayerInSight && !enemy.GetIsAttacking())
        {
            if (random.Next(0, 2) == 0)
            {
                enemy.TransitionToState(new AttackState());
            }
            else
            {
                enemy.TransitionToState(new StrongAttackState());
            }
        }
    }

    /// <summary>
    /// ���U����Ԃ�ޏo���鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃ�ޏo����G</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
