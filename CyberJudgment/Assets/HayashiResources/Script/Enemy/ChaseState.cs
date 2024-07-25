using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        enemy.animator.SetBool("isMoving", true);
    }

    public void UpdateState(EnemyBase enemy)
    {
        //攻撃アニメーションが再生されていなければ追跡する
        if (!enemy.animator.GetBool("Attack"))
        {
            enemy.MoveTowards(enemy.player.position);
            enemy.RotateTowards(enemy.player.position);
        }
        //プレイヤーが攻撃範囲内であれば攻撃ステートに
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) <= enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new AttackState());
        }
        //プレイヤーが居なければIdleに
        if (!enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.animator.SetBool("isMoving", false);
    }
}
