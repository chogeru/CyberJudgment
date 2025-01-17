using UnityEngine;

/// <summary>
/// 敵がプレイヤーを追跡する状態
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
        // 攻撃中でなければ追跡
        if (!enemy.GetIsAttacking())
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }

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

        // プレイヤーが視界に入っていなければ IdleState に遷移
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
