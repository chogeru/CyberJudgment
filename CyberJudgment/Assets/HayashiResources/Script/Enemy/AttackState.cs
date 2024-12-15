using UnityEngine;

/// <summary>
/// 敵がプレイヤーを攻撃する状態
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

        // プレイヤーが攻撃範囲外または視界から外れた場合は ChaseState に遷移
        if (distanceToPlayer > enemy.enemyData.attackRange || !enemy.isPlayerInSight)
        {
            enemy._animator.SetBool("Attack", false);
            enemy.TransitionToState(new ChaseState());
            return;
        }

        // アニメーションが終了したかを確認
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
