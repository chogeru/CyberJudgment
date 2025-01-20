using R3;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerController PlayerController { get; private set; }
    public PlayerAnimationController PlayerAnimationController { get; private set; }
    public PlayerAttackController PlayerAttackController { get; private set; }

    public PlayerMP playerMP { get; private set; }

    private bool isAttacking;
    private bool isHit;
    private bool isDead;

    private readonly ReactiveProperty<bool> isGuarding = new ReactiveProperty<bool>(false);

    public bool IsHit => isHit;
    public bool IsGuarding => isGuarding.Value;
    public bool IsDead => isDead;

    private PlayerState currentState;

    private void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        PlayerAnimationController = GetComponent<PlayerAnimationController>();
        PlayerAttackController = GetComponent<PlayerAttackController>();
        playerMP = GetComponent<PlayerMP>();
    }


    /// <summary>
    /// �K�[�h��Ԃ�ݒ肵�܂��B
    /// </summary>
    public void SetGuarding(bool guarding)
    {
        isGuarding.Value = guarding;
        PlayerController.SetMovementEnabled(!guarding);
        PlayerAttackController.SetAttackEnabled(!guarding);
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

    public void SetHitState(bool hit)
    {
        isHit = hit;
        PlayerController.SetMovementEnabled(!hit);
        PlayerAttackController.SetAttackEnabled(!hit);
    }

    public void SetDeadState(bool dead)
    {
        isDead = dead;
        PlayerController.SetMovementEnabled(!dead);
        PlayerAttackController.SetAttackEnabled(!dead);

        if (dead)
        {
            UpdatePlayerState(PlayerState.Dead);
        }
        else
        {
            UpdatePlayerState(PlayerState.Idle);
        }
    }
}
/// <summary>
/// �v���C���[�̏�Ԓ�`
/// </summary>
public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Guard,
    Dead
}

