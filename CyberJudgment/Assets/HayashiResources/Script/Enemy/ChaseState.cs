using UnityEngine;

/// <summary>
/// �G���v���C���[��ǐՂ�����
/// </summary>
public class ChaseState : IEnemyState
{
    private float originalAnimatorSpeed;
    public void EnterState(EnemyBase enemy)
    {
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;
        enemy._animator.SetBool("isMoving", true);
        Debug.Log($"{enemy.name}: Enter ChaseState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        // �U�����łȂ���Βǐ�
        if (!enemy.GetIsAttacking())
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // �U���͈͓��ōU�����łȂ���΍U���X�e�[�g�ɑJ��
        if (distanceToPlayer <= enemy.enemyData.attackRange && !enemy.GetIsAttacking())
        {
            int attackChoice = Random.Range(0, 2); // 0 �܂��� 1
            if (attackChoice == 0)
            {
                enemy.TransitionToState(new AttackState());
            }
            else
            {
                enemy.TransitionToState(new StrongAttackState());
            }
            return;
        }

        // �v���C���[�����E�ɓ����Ă��Ȃ���� IdleState �ɑJ��
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
            return;
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        Debug.Log($"{enemy.name}: Exit ChaseState");
    }
}
