using UnityEngine;

public class AttackState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        enemy.animator.SetBool("Attack", true);
    }

    public void UpdateState(EnemyBase enemy)
    {

        if (!enemy.isPlayerInSight || Vector3.Distance(enemy.transform.position, enemy.player.position) > enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new ChaseState());
        }
        else if (enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new IdleState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
    }
}
