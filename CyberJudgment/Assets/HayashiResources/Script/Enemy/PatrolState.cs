/// <summary>
/// 敵が設定されたポイント間を巡回する状態
/// </summary>
public class PatrolState : IEnemyState
{

    /// <summary>
    /// 巡回状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    public void EnterState(EnemyBase enemy)
    {
        enemy._animator.SetBool("isMoving", true);
    }

    /// <summary>
    /// 巡回状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
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

    /// <summary>
    /// 巡回状態を退出する時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態を退出する敵</param>
    public void ExitState(EnemyBase enemy)
    {
    }
}
