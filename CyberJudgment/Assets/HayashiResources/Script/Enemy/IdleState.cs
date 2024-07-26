using UnityEngine;

/// <summary>
/// 敵が待機する状態
/// </summary>
public class IdleState : IEnemyState
{

    /// <summary>
    /// 待機状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
        enemy._animator.SetBool("Attack", false);
    }

    /// <summary>
    /// 待機状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
    public void UpdateState(EnemyBase enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy._player.position) <= enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new AttackState());
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
