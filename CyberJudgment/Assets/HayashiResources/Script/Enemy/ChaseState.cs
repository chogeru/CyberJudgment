using UnityEngine;

/// <summary>
/// �G���v���C���[��ǐՂ�����
/// </summary>
public class ChaseState : IEnemyState
{
    private System.Random random = new System.Random();

    /// <summary>
    /// �ǐՏ�Ԃɓ��鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɓ���G</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", true);
    }

    /// <summary>
    /// �ǐՏ�Ԃ��X�V���邽�߂ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃɂ���G</param>
    public void UpdateState(EnemyBase enemy)
    {
        //�U���A�j���[�V�������Đ�����Ă��Ȃ���ΒǐՂ���
        if (!enemy.GetIsAttacking())
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }

        // �v���C���[���U���͈͓��ł���΍U���X�e�[�g�ɑJ��
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
        // �v���C���[�����E�ɓ����Ă��Ȃ����Idle�ɑJ��
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    /// <summary>
    /// �ǐՏ�Ԃ�ޏo���鎞�ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="enemy">���̏�Ԃ�ޏo����G</param>
    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
    }
}
