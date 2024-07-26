using UnityEngine;

/// <summary>
/// 敵がプレイヤーを攻撃する状態
/// </summary>
public class AttackState : IEnemyState
{

    /// <summary>
    /// 攻撃状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("Attack", true);
    }

    /// <summary>
    /// 攻撃状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
    public void UpdateState(EnemyBase enemy)
    {

        if (!enemy.isPlayerInSight || Vector3.Distance(enemy.transform.position, enemy._player.position) > enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new ChaseState());
        }
        else if (enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    /// <summary>
    /// 攻撃状態を退出する時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態を退出する敵</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
