using UnityEngine;

/// <summary>
/// 待機 (パトロール) 状態
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

        // Attack系トリガーをリセット
        enemy._animator.ResetTrigger("Attack");
        enemy._animator.ResetTrigger("StrongAttack");

        Debug.Log($"{enemy.name}: Enter IdleState");
    }

    public void UpdateState(EnemyBase enemy)
    {
        // パトロールロジック (派生クラスで実装)
        enemy.Patrol();

        if (enemy._player == null) return;

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // 攻撃可能範囲かつ攻撃クールダウン終了かつ攻撃中でない
        if (distanceToPlayer <= enemy.enemyData.attackRange &&
            !enemy.GetIsAttacking() &&
            enemy.CanAttack())
        {
            // 攻撃ステートへ遷移
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

        // プレイヤーが視界内に入っている場合はChaseへ
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
