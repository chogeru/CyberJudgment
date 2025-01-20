using UnityEngine;

/// <summary>
/// �v���C���[��ǐՂ�����
/// </summary>
public class ChaseState : IEnemyState
{
    private float originalAnimatorSpeed;

    public void EnterState(EnemyBase enemy)
    {
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;

        enemy._animator.SetBool("isMoving", true);
        enemy._animator.SetBool("TakeDamage", false);

        Debug.Log($"{enemy.name}: Enter ChaseState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (enemy._player == null) return;

        // �U�����łȂ���΃v���C���[�Ɍ������Ĉړ�
        if (!enemy.GetIsAttacking())
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // �U���\�͈� & �N�[���_�E���I�� & �U��������Ȃ�
        if (distanceToPlayer <= enemy.enemyData.attackRange &&
            enemy.CanAttack() &&
            !enemy.GetIsAttacking())
        {
            // �U���X�e�[�g��
            int attackChoice = Random.Range(0, 2);
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

        // �v���C���[�����E�O�ɏ������� Idle��
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
            return;
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        enemy._animator.speed = originalAnimatorSpeed;

        Debug.Log($"{enemy.name}: Exit ChaseState");
    }
}
