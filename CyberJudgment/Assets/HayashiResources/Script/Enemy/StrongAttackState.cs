using UnityEngine;

/// <summary>
/// 敵がプレイヤーを強攻撃する状態
/// </summary>
public class StrongAttackState : IEnemyState
{
    private System.Random random = new System.Random();

    /// <summary>
    /// 強攻撃状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    public void EnterState(EnemyBase enemy)
    {
        if (!enemy.GetIsAttacking())
        {
            enemy.SetIsAttacking(true);
            enemy._animator.SetBool("StrongAttack", true);
        }
    }

    /// <summary>
    /// 強攻撃状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
    public void UpdateState(EnemyBase enemy)
    {
        if (!enemy.isPlayerInSight || Vector3.Distance(enemy.transform.position, enemy._player.position) > enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new ChaseState());
        }
        else if (enemy.isPlayerInSight && !enemy.GetIsAttacking())
        {
            if (random.Next(0, 2) == 0)
            {
                enemy.TransitionToState(new AttackState());
            }
            else
            {
                enemy.TransitionToState(new StrongAttackState());
            }
        }
    }

    /// <summary>
    /// 強攻撃状態を退出する時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態を退出する敵</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
