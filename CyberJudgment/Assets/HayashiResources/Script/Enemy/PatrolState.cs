public class PatrolState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        enemy.animator.SetBool("isMoving", true);
    }

    public void UpdateState(EnemyBase enemy)
    {
        Enemy specificEnemy = enemy as Enemy;
        if (specificEnemy != null)
        {
            specificEnemy.Patrol();
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
