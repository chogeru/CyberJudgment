using UnityEngine;

/// <summary>
/// 敵がプレイヤーを強攻撃する状態
/// </summary>
public class StrongAttackState : IEnemyState
{
    private bool animationCompleted = false;
    private float originalAnimatorSpeed;
    public void EnterState(EnemyBase enemy)
    {
        originalAnimatorSpeed = enemy._animator.speed;
        enemy._animator.speed = 1f;
        enemy.SetIsAttacking(true);
        enemy._animator.SetBool("StrongAttack", true);
        Debug.Log($"{enemy.name}: Enter StrongAttackState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // プレイヤーが攻撃範囲外または視界から外れた場合は ChaseState に遷移
        if (distanceToPlayer > enemy.enemyData.attackRange || !enemy.isPlayerInSight)
        {
            enemy._animator.SetBool("StrongAttack", false);
            enemy.TransitionToState(new ChaseState());
            return;
        }

        // アニメーションが終了したかを確認
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
        Debug.Log($"{enemy.name}: Exit StrongAttackState");
    }
}
