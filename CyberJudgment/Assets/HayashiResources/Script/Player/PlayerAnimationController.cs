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
    [SerializeField]
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
            .Select(_ => _uiPresenter.IsMenuOpen)
            .DistinctUntilChanged() // ��Ԃ��ω������Ƃ���������
            .Subscribe(isMenuOpen =>
            {
                ToggleUIAnimation(isMenuOpen);
            })
            .AddTo(this);
    }

    /// <summary>
    /// UI�̊J�ɉ�������p�A�j���[�V�����̍Đ�
    /// </summary>
    /// <param name="isUIOpen">UI���J���Ă��邩�ǂ���</param>
    private void ToggleUIAnimation(bool isUIOpen)
    {
        if (this.isUIOpen == isUIOpen)
        {
            // ��Ԃ��ύX����Ă��Ȃ��ꍇ�͉������Ȃ�
            return;
        }

        this.isUIOpen = isUIOpen;

        if (isUIOpen)
        {
            // UI���J���Ă���Ԃ͐�p�̃A�j���[�V�������Đ�
            m_Animator.CrossFade("UIOpen", 0.2f);
            m_Animator.SetBool("Idle", false);

        }
        else
        {
            // UI�������Ƃ��ɃA�j���[�V���������Z�b�g
            Observable.Timer(System.TimeSpan.FromSeconds(0.2)) // UI��p�A�j���[�V�����̃N���X�t�F�[�h����
                .Subscribe(_ =>
                {
                    UpdateAnimator(playerState.Value); // �v���C���[�̌��݂̏�ԂɊ�Â��ăA�j���[�V�������Đݒ�
                }).AddTo(this);
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