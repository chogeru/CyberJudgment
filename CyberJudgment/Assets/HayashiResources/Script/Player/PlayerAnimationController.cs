using AbubuResouse.Singleton;
using R3;
using R3.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField, Header("アニメーター")]
    private Animator m_Animator;

    private readonly ReactiveProperty<PlayerState> playerState = new ReactiveProperty<PlayerState>(PlayerState.Idle);

    private UIPresenter _uiPresenter;
    private bool isUIOpen = false;

    private void Start()
    {
        _uiPresenter = UIPresenter.Instance;

        playerState
            .DistinctUntilChanged()
            .Where(_ => !isUIOpen) // UIが開いていないときのみ状態に応じたアニメーションを更新
            .Subscribe(UpdateAnimator)
            .AddTo(this);

        BindAnimations();
        ObserveUIState();
    }

    /// <summary>
    /// プレイヤーの状態に応じてアニメーターのパラメーター更新
    /// </summary>
    /// <param name="state"></param>
    private void UpdateAnimator(PlayerState state)
    {
        m_Animator.SetBool("Idle", state == PlayerState.Idle);
        m_Animator.SetBool("Walk", state == PlayerState.Walk);
        m_Animator.SetBool("Run", state == PlayerState.Run);
    }

    /// <summary>
    /// UIの開閉状態を監視し、専用アニメーションの再生を管理する
    /// </summary>
    private void ObserveUIState()
    {
        Observable.EveryUpdate()
            .Where(_ => _uiPresenter.IsMenuOpen != isUIOpen)
            .Subscribe(_ => ToggleUIAnimation(_uiPresenter.IsMenuOpen))
            .AddTo(this);
    }

    /// <summary>
    /// UIの開閉に応じた専用アニメーションの再生
    /// </summary>
    /// <param name="isUIOpen">UIが開いているかどうか</param>
    private void ToggleUIAnimation(bool isUIOpen)
    {
        this.isUIOpen = isUIOpen;

        if (isUIOpen)
        {
            // UIが開いている間は専用のアニメーションを再生
            m_Animator.CrossFade("UIOpen",0.2f);
        }
        else
        {
            // UIが閉じたら現在のプレイヤー状態に基づいたアニメーションに遷移
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
    /// 入力によりアニメーション更新処理
    /// </summary>
    private void BindAnimations()
    {
        this.UpdateAsObservable()
            .Select(_ => Input.GetAxis("Horizontal"))
            .Subscribe(horizontal =>
            {
                m_Animator.SetFloat("左右", horizontal);
                m_Animator.SetFloat("走り左右", horizontal);
            }).AddTo(this);

        this.UpdateAsObservable()
            .Select(_ => Input.GetAxis("Vertical"))
            .Subscribe(vertical =>
            {
                m_Animator.SetFloat("前後", vertical);
                m_Animator.SetFloat("走り前後", vertical);
            }).AddTo(this);
    }

    /// <summary>
    /// プレイヤーの状態更新
    /// </summary>
    /// <param name="state">状態</param>
    public void UpdateState(PlayerState state)
    {
        playerState.Value = state;
    }
}