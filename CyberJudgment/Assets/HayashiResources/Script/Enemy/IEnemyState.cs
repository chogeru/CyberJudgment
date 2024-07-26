/// <summary>
/// 敵の状態を定義するインターフェース
/// </summary>
public interface IEnemyState
{
    /// <summary>
    /// この状態に入る時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態に入る敵</param>
    void EnterState(EnemyBase enemy);
    /// <summary>
    /// この状態を更新するために呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態にある敵</param>
    void UpdateState(EnemyBase enemy);
    /// <summary>
    /// この状態を退出する時に呼び出されるメソッド
    /// </summary>
    /// <param name="enemy">この状態を退出する敵</param>
    void ExitState(EnemyBase enemy);
}
