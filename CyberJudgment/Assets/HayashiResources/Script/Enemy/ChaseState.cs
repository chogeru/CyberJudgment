using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        enemy.animator.SetBool("isMoving", true);
    }

    public void UpdateState(EnemyBase enemy)
    {
        //�U���A�j���[�V�������Đ�����Ă��Ȃ���ΒǐՂ���
        if (!enemy.animator.GetBool("Attack"))
        {
            enemy.MoveTowards(enemy.player.position);
            enemy.RotateTowards(enemy.player.position);
        }
        //�v���C���[���U���͈͓��ł���΍U���X�e�[�g��
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) <= enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new AttackState());
        }
        //�v���C���[�����Ȃ����Idle��
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.animator.SetBool("isMoving", false);
    }
}
