using UnityEngine;

/// <summary>
/// プレイヤーを追跡する状態
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

        // 攻撃中でなければプレイヤーに向かって移動
        if (!enemy.GetIsAttacking())
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy._player.position);

        // 攻撃可能範囲 & クールダウン終了 & 攻撃中じゃない
        if (distanceToPlayer <= enemy.enemyData.attackRange &&
            enemy.CanAttack() &&
            !enemy.GetIsAttacking())
        {
            // 攻撃ステートへ
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

        // プレイヤーが視界外に消えたら Idleへ
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
