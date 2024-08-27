using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerController PlayerController { get; private set; }
    public PlayerAnimationController PlayerAnimationController { get; private set; }
    public PlayerAttackController PlayerAttackController { get; private set; }

    private bool isAttacking;
    private PlayerState currentState;

    private void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        PlayerAnimationController = GetComponent<PlayerAnimationController>();
        PlayerAttackController = GetComponent<PlayerAttackController>();
    }

    /// <summary>
    /// プレイヤーの状態更新
    /// </summary>
    /// <param name="state"></param>
    public void UpdatePlayerState(PlayerState state)
    {
        PlayerAnimationController.UpdateState(state);
    }

    /// <summary>
    /// プレイヤーの現在の状態を取得
    /// </summary>
    public PlayerState CurrentState => currentState;

    /// <summary>
    /// 攻撃中フラグの設定
    /// </summary>
    /// <param name="isAttacking"></param>
    public void SetAttacking(bool isAttacking)
    {
        this.isAttacking = isAttacking;
        PlayerController.SetMovementEnabled(!isAttacking);
    }

    /// <summary>
    /// 攻撃中であるかどうかを確認
    /// </summary>
    /// <returns></returns>
    public bool IsAttacking()
    {
        return isAttacking;
    }
}
/// <summary>
/// プレイヤーの状態定義
/// </summary>
public enum PlayerState
{
    Idle,
    Walk,
    Run
}

