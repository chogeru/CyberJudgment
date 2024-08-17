using R3;
using R3.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    private Animator m_Animator;

    private readonly ReactiveProperty<PlayerState> playerState = new ReactiveProperty<PlayerState>(PlayerState.Idle);

    private void Start()
    {
        playerState
            .DistinctUntilChanged()
            .Subscribe(UpdateAnimator)
            .AddTo(this);

        BindAnimations();
    }

    /// <summary>
    /// �v���C���[�̏�Ԃɉ����ăA�j���[�^�[�̃p�����[�^�[�X�V
    /// </summary>
    /// <param name="state"></param>
    private void UpdateAnimator(PlayerState state)
    {
        m_Animator.SetBool("Idle", state == PlayerState.Idle);
        m_Animator.SetBool("Walk", state == PlayerState.Walk);
        m_Animator.SetBool("Run", state == PlayerState.Run);
    }

    /// <summary>
    /// ���͂ɂ��A�j���[�V�����X�V����
    /// </summary>
    private void BindAnimations()
    {
        this.UpdateAsObservable()
            .Select(_ => Input.GetAxis("Horizontal"))
            .Subscribe(horizontal =>
            {
                m_Animator.SetFloat("���E", horizontal);
                m_Animator.SetFloat("���荶�E", horizontal);
            }).AddTo(this);

        this.UpdateAsObservable()
            .Select(_ => Input.GetAxis("Vertical"))
            .Subscribe(vertical =>
            {
                m_Animator.SetFloat("�O��", vertical);
                m_Animator.SetFloat("����O��", vertical);
            }).AddTo(this);
    }

    /// <summary>
    /// �v���C���[�̏�ԍX�V
    /// </summary>
    /// <param name="state">���</param>
    public void UpdateState(PlayerState state)
    {
        playerState.Value = state;
    }
}