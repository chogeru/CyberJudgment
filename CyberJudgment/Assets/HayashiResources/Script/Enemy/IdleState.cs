using UnityEngine;

/// <summary>
/// 敵が待機する状態
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

        // プレイヤーとの距離をチェック
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // 攻撃範囲内で攻撃中でなければ攻撃ステートに遷移
        if (distanceToPlayer <= enemy.enemyData.attackRange && !enemy.GetIsAttacking())
        {
            int attackChoice = Random.Range(0, 2); // 0 または 1
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

        // プレイヤーが視界に入っていれば ChaseState に遷移
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
