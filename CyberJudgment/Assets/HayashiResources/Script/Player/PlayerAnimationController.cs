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
    [SerializeField]
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
            .Select(_ => _uiPresenter.IsMenuOpen)
            .DistinctUntilChanged() // 状態が変化したときだけ反応
            .Subscribe(isMenuOpen =>
            {
                ToggleUIAnimation(isMenuOpen);
            })
            .AddTo(this);
    }

    /// <summary>
    /// UIの開閉に応じた専用アニメーションの再生
    /// </summary>
    /// <param name="isUIOpen">UIが開いているかどうか</param>
    private void ToggleUIAnimation(bool isUIOpen)
    {
        if (this.isUIOpen == isUIOpen)
        {
            // 状態が変更されていない場合は何もしない
            return;
        }

        this.isUIOpen = isUIOpen;

        if (isUIOpen)
        {
            // UIが開いている間は専用のアニメーションを再生
            m_Animator.CrossFade("UIOpen", 0.2f);
            m_Animator.SetBool("Idle", false);

        }
        else
        {
            // UIが閉じたときにアニメーションをリセット
            Observable.Timer(System.TimeSpan.FromSeconds(0.2)) // UI専用アニメーションのクロスフェード時間
                .Subscribe(_ =>
                {
                    UpdateAnimator(playerState.Value); // プレイヤーの現在の状態に基づいてアニメーションを再設定
                }).AddTo(this);
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