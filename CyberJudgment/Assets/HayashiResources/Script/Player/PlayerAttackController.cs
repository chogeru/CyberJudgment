using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    private Animator m_Animator;

    [SerializeField, Header("�U���ҋ@����")]
    private float m_AttackCoolDown = 0.5f;

    private bool isAttack = true;

    private void Start()
    {
        BindAttackInput();
    }

    /// <summary>
    /// ���͂ɂ��U���A�j���[�V�������Đ����鏈��
    /// </summary>
    private void BindAttackInput()
    {
        // ���N���b�N�ɂ��ʏ�U��
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("NormalAttack"));

        // �E�N���b�N�ɂ�鋭�U��
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(1))
            .Where(_ => isAttack)
            .Subscribe(_ => ExecuteAttack("StrongAttack"));

        // �}�E�X�{�^���̃����[�X�����m����Idle�Ɉڍs
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            .Subscribe(_ => CancelAttack());
    }

    /// <summary>
    /// �U���A�j���[�V�����̍Đ�
    /// </summary>
    /// <param name="animationTrigger">�Đ�����A�j���[�V�����̃g���K�[��</param>
    private void ExecuteAttack(string animationTrigger)
    {
        var playerManager = GetComponent<PlayerManager>();
        playerManager.SetAttacking(true);

        m_Animator.Play(animationTrigger);
        isAttack = false;
        ResetAttackCooldown().Forget();
    }

    /// <summary>
    /// �U�����L�����Z������Idle��ԂɈڍs
    /// </summary>
    private void CancelAttack()
    {
        // ���݂̃A�j���[�V�����X�e�[�g���U���ł��邩���m�F
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("NormalAttack") ||
            m_Animator.GetCurrentAnimatorStateInfo(0).IsName("StrongAttack"))
        {
            // �U�����L�����Z������Idle�Ɉڍs
            m_Animator.CrossFade("Idle", 0.05f);
            isAttack = true;

            // �v���C���[�̏�Ԃ�Idle�ɍX�V
            var playerManager = GetComponent<PlayerManager>();
            playerManager.UpdatePlayerState(PlayerState.Idle);
            playerManager.SetAttacking(false);

            // �ړ��L�[��������Ă��邩���m�F���A�Ή�����A�j���[�V�����ƃX�e�[�g���X�V
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            if (movement != Vector3.zero)
            {
                // ���肩�����̏�ԂɑJ��
                playerManager.UpdatePlayerState(isRunning ? PlayerState.Run : PlayerState.Walk);
                m_Animator.SetBool("Run", isRunning);
                m_Animator.SetBool("Walk", !isRunning);
            }
            else
            {
                // Idle�ɖ߂�
                playerManager.UpdatePlayerState(PlayerState.Idle);
            }
        }
    }

    /// <summary>
    /// �N�[���_�E�����o�čĂэU�����\�ɂ���
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ResetAttackCooldown()
    {
        await UniTask.Delay((int)(m_AttackCoolDown * 1000));
        isAttack = true;
    }
}

