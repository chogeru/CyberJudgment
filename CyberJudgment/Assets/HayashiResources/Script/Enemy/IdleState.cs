using UnityEngine;
public class IdleState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        enemy.animator.SetBool("isMoving", false);
        enemy.animator.SetBool("Attack", false);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) <= enemy.enemyData.attackRange)
        {
            enemy.TransitionToState(new AttackState());
        }
        if (enemy.isPlayerInSight)
        {
            enemy.TransitionToState(new ChaseState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
    }
}
