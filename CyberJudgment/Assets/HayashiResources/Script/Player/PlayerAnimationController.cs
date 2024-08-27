using AbubuResouse.Singleton;
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

    private UIPresenter _uiPresenter;
    private bool isUIOpen = false;

    private void Start()
    {
        _uiPresenter = UIPresenter.Instance;

        playerState
            .DistinctUntilChanged()
            .Where(_ => !isUIOpen) // UI���J���Ă��Ȃ��Ƃ��̂ݏ�Ԃɉ������A�j���[�V�������X�V
            .Subscribe(UpdateAnimator)
            .AddTo(this);

        BindAnimations();
        ObserveUIState();
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
    /// UI�̊J��Ԃ��Ď����A��p�A�j���[�V�����̍Đ����Ǘ�����
    /// </summary>
    private void ObserveUIState()
    {
        Observable.EveryUpdate()
            .Where(_ => _uiPresenter.IsMenuOpen != isUIOpen)
            .Subscribe(_ => ToggleUIAnimation(_uiPresenter.IsMenuOpen))
            .AddTo(this);
    }

    /// <summary>
    /// UI�̊J�ɉ�������p�A�j���[�V�����̍Đ�
    /// </summary>
    /// <param name="isUIOpen">UI���J���Ă��邩�ǂ���</param>
    private void ToggleUIAnimation(bool isUIOpen)
    {
        this.isUIOpen = isUIOpen;

        if (isUIOpen)
        {
            // UI���J���Ă���Ԃ͐�p�̃A�j���[�V�������Đ�
            m_Animator.CrossFade("UIOpen",0.2f);
        }
        else
        {
            // UI�������猻�݂̃v���C���[��ԂɊ�Â����A�j���[�V�����ɑJ��
            if (playerState.Value == PlayerState.Idle)
            {
                m_Animator.CrossFade("Idle", 0.2f);
            }
            else if (playerState.Value == PlayerState.Walk)
            {
                m_Animator.CrossFade("Walk", 0.05f);
            }
            else if (playerState.Value == PlayerState.Run)
            {
                m_Animator.CrossFade("Run", 0.05f);
            }
        }
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