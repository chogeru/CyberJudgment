using AbubuResouse.Singleton;
using R3;
using R3.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using AbubuResouse.MVP.Presenter;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    private Animator m_Animator;

    private readonly ReactiveProperty<PlayerState> playerState = new ReactiveProperty<PlayerState>(PlayerState.Idle);

    private UIPresenter _uiPresenter;
    [SerializeField]
    private bool isUIOpen = false;

    private PlayerManager _playerManager;

    private void Start()
    {
        _playerManager = GetComponent<PlayerManager>();

        _uiPresenter = UIPresenter.Instance;

        playerState
            .DistinctUntilChanged()
            .Where(_ => !isUIOpen && !_playerManager.IsDead) 
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
        if (state == PlayerState.Dead)
        {
            m_Animator.SetBool("Dead", true);
            m_Animator.SetBool("Guard", false);
            m_Animator.SetBool("Idle", false);
            m_Animator.SetBool("Walk", false);
            m_Animator.SetBool("Run", false);
            m_Animator.ResetTrigger("NormalAttack");
            m_Animator.ResetTrigger("StrongAttack");
            return;
        }

        // Guard��Ԃ��ŗD��ɏ���
        if (state == PlayerState.Guard)
        {
            m_Animator.SetBool("Guard", true);
            m_Animator.SetBool("Idle", false);
            m_Animator.SetBool("Walk", false);
            m_Animator.SetBool("Run", false);
            m_Animator.SetBool("Dead", false);
        }
        else
        {
            // Guard�ȊO�̏�Ԃ�ݒ�
            m_Animator.SetBool("Guard", false);
            m_Animator.SetBool("Idle", state == PlayerState.Idle);
            m_Animator.SetBool("Walk", state == PlayerState.Walk);
            m_Animator.SetBool("Run", state == PlayerState.Run);
            m_Animator.SetBool("Dead", state == PlayerState.Dead);
        }
    }


    /// <summary>
    /// UI�̊J��Ԃ��Ď����A��p�A�j���[�V�����̍Đ����Ǘ�����
    /// </summary>
    private void ObserveUIState()
    {
        Observable.EveryUpdate()
            .Select(_ => _uiPresenter.IsMenuOpen)
            .DistinctUntilChanged() 
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
            return;
        }

        this.isUIOpen = isUIOpen;

        if (isUIOpen)
        {
            m_Animator.CrossFade("UIOpen", 0.2f);
            m_Animator.SetBool("Idle", false);
        }
        else
        {
            Observable.Timer(System.TimeSpan.FromSeconds(0.2)) // UI��p�A�j���[�V�����̃N���X�t�F�[�h����
                .Subscribe(_ =>
                {
                    if (!_playerManager.IsDead) // ���S���Ă��Ȃ��ꍇ�̂ݍX�V
                    {
                        UpdateAnimator(playerState.Value); // �v���C���[�̌��݂̏�ԂɊ�Â��ăA�j���[�V�������Đݒ�
                    }
                }).AddTo(this);
        }
    }

    /// <summary>
    /// ���͂ɂ��A�j���[�V�����X�V����
    /// </summary>
    private void BindAnimations()
    {
        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding) // �K�[�h���łȂ��ꍇ�̂ݎ��s
            .Select(_ => Input.GetAxis("Horizontal"))
            .Subscribe(horizontal =>
            {
                m_Animator.SetFloat("���E", horizontal);
                m_Animator.SetFloat("���荶�E", horizontal);
            }).AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding) // �K�[�h���łȂ��ꍇ�̂ݎ��s
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
