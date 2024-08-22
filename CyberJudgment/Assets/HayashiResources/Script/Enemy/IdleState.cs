using UnityEngine;

/// <summary>
/// 敵が待機する状態
/// </summary>
public class IdleState : IEnemyState
{
    private System.Random random = new System.Random();

    /// <summary>
    /// 待機状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        enemy._animator.SetBool("Attack", false);
        enemy._animator.SetBool("StrongAttack", false);
    }

    /// <summary>
    /// 待機状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
    public void UpdateState(EnemyBase enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy._player.position) <= enemy.enemyData.attackRange)
        {
            // 50%の確率で通常攻撃か強攻撃を選ぶ
            if (random.Next(0, 2) == 0)
            {
                enemy.TransitionToState(new AttackState());
            }
            else
            {
                enemy.TransitionToState(new StrongAttackState());
            }
        }
        if (enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new ChaseState());
        }
    }


    /// <summary>
    /// 待機状態を退出する時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態を退出する敵</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
