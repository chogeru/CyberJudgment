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
    /// �v���C���[�̏�ԍX�V
    /// </summary>
    /// <param name="state"></param>
    public void UpdatePlayerState(PlayerState state)
    {
        PlayerAnimationController.UpdateState(state);
    }

    /// <summary>
    /// �v���C���[�̌��݂̏�Ԃ��擾
    /// </summary>
    public PlayerState CurrentState => currentState;

    /// <summary>
    /// �U�����t���O�̐ݒ�
    /// </summary>
    /// <param name="isAttacking"></param>
    public void SetAttacking(bool isAttacking)
    {
        this.isAttacking = isAttacking;
        PlayerController.SetMovementEnabled(!isAttacking);
    }

    /// <summary>
    /// �U�����ł��邩�ǂ������m�F
    /// </summary>
    /// <returns></returns>
    public bool IsAttacking()
    {
        return isAttacking;
    }
}
/// <summary>
/// �v���C���[�̏�Ԓ�`
/// </summary>
public enum PlayerState
{
    Idle,
    Walk,
    Run
}

