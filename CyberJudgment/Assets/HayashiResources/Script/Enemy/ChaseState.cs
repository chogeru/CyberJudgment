using UnityEngine;

/// <summary>
/// 敵がプレイヤーを追跡する状態
/// </summary>
public class ChaseState : IEnemyState
{

    /// <summary>
    /// 追跡状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", true);
    }

    /// <summary>
    /// 追跡状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
    public void UpdateState(EnemyBase enemy)
    {
        //攻撃アニメーションが再生されていなければ追跡する
        if (!enemy._animator.GetBool("Attack"))
        {
            enemy.MoveTowards(enemy._player.position);
            enemy.RotateTowards(enemy._player.position);
        }
        //プレイヤーが攻撃範囲内であれば攻撃ステートに
        if (Vector3.Distance(enemy.transform.position, enemy._player.position) <= enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new AttackState());
        }
        //プレイヤーが居なければIdleに
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    /// <summary>
    /// 追跡状態を退出する時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態を退出する敵</param>
    public void ExitState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", false);
    }
}
