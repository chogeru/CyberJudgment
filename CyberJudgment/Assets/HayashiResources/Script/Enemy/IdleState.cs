using UnityEngine;

/// <summary>
/// �ҋ@ (�p�g���[��) ���
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

        // Attack�n�g���K�[�����Z�b�g
        enemy._animator.ResetTrigger("Attack");
        enemy._animator.ResetTrigger("StrongAttack");

        Debug.Log($"{enemy.name}: Enter IdleState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        // �p�g���[�����W�b�N (�h���N���X�Ŏ���)
        enemy.Patrol();

        if (enemy._player == null) return;

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // �U���\�͈͂��U���N�[���_�E���I�����U�����łȂ�
        if (distanceToPlayer <= enemy.enemyData.attackRange &&
            !enemy.GetIsAttacking() &&
            enemy.CanAttack())
        {
            // �U���X�e�[�g�֑J��
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

        // �v���C���[�����E���ɓ����Ă���ꍇ��Chase��
        if (enemy.isPlayerInSight && !enemy.GetIsAttacking())
        {
            enemy.TransitionToState(new ChaseState());
            return;
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("Idle", false);
        Debug.Log($"{enemy.name}: Exit IdleState");
    }
}
