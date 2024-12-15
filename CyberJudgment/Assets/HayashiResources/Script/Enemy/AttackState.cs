using UnityEngine;

/// <summary>
/// �G���v���C���[���U��������
/// </summary>
public class AttackState : IEnemyState
{
    private bool animationCompleted = false;
    private float originalAnimatorSpeed;
    public void EnterState(EnemyBase enemy)
    {
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;
        enemy.SetIsAttacking(true);
        enemy._animator.SetBool("Attack", true);
        Debug.Log($"{enemy.name}: Enter AttackState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // �v���C���[���U���͈͊O�܂��͎��E����O�ꂽ�ꍇ�� ChaseState �ɑJ��
        if (distanceToPlayer > enemy.enemyData.attackRange || !enemy.isPlayerInSight)
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new ChaseState());
            return;
        }

        // �A�j���[�V�������I�����������m�F
        AnimatorStateInfo stateInfo = enemy._animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f && enemy.GetIsAttacking())
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new RetreatState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetIsAttacking(false);
        enemy._animator.SetBool("Attack", false);
        Debug.Log($"{enemy.name}: Exit AttackState");
    }
}
