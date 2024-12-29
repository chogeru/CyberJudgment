using UnityEngine;

/// <summary>
/// �G���ҋ@������
/// </summary>
public class IdleState : IEnemyState
{
    private float originalAnimatorSpeed;
    public void EnterState(EnemyBase enemy)
    {
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;
        enemy._animator.SetBool("Idle", true);
        enemy._animator.SetBool("isMoving", false);
        enemy._animator.SetBool("isRetreating", false);
        enemy._animator.ResetTrigger("Attack");
        enemy._animator.ResetTrigger("StrongAttack");
        Debug.Log($"{enemy.name}: Enter IdleState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        enemy.Patrol();

        // �v���C���[�Ƃ̋������`�F�b�N
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

        // �v���C���[�����E�ɓ����Ă���� ChaseState �ɑJ��
        if (enemy.isPlayerInSight && !enemy.GetIsAttacking())
        {
            enemy.TransitionToState(new ChaseState());
            return;
        }

    }

    public void ExitState(EnemyBase enemy)
    {
        Debug.Log($"{enemy.name}: Exit IdleState");
    }
}
