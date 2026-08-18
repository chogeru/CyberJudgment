using UnityEngine;

/// <summary>
/// 強攻撃ステート
/// </summary>
public class StrongAttackState : IEnemyState
{
    private float originalAnimatorSpeed;

    public void EnterState(EnemyBase enemy)
    {
        // ■攻撃開始時にクールダウンをリセット
        enemy.ResetAttackCooldown();

        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;

        enemy.SetIsAttacking(true);
        enemy._animator.SetBool("StrongAttack", true);

        Debug.Log($"{enemy.name}: Enter StrongAttackState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (enemy._player == null) return;

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // 射程外 or 視界外ならChaseへ
        if (distanceToPlayer > enemy.enemyData.attackRange || !enemy.isPlayerInSight)
        {
            enemy._animator.SetBool("StrongAttack", false);
            enemy.TransitionToState(new ChaseState());
            return;
        }

        // 強攻撃アニメが最後まで再生されたら後退ステートへ
        AnimatorStateInfo stateInfo = enemy._animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("StrongAttack") && stateInfo.normalizedTime >= 1.0f && enemy.GetIsAttacking())
        {
            enemy._animator.SetBool("StrongAttack", false);
            enemy.TransitionToState(new RetreatState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetIsAttacking(false);
        enemy._animator.SetBool("StrongAttack", false);
        enemy._animator.speed = originalAnimatorSpeed;

        Debug.Log($"{enemy.name}: Exit StrongAttackState");
    }
}
