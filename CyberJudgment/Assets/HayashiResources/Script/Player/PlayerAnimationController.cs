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
    [SerializeField, Header("アニメーター")]
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
    /// プレイヤーの状態に応じてアニメーターのパラメーター更新
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

        // Guard状態を最優先に処理
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
            // Guard以外の状態を設定
            m_Animator.SetBool("Guard", false);
            m_Animator.SetBool("Idle", state == PlayerState.Idle);
            m_Animator.SetBool("Walk", state == PlayerState.Walk);
            m_Animator.SetBool("Run", state == PlayerState.Run);
            m_Animator.SetBool("Dead", state == PlayerState.Dead);
        }
    }


    /// <summary>
    /// UIの開閉状態を監視し、専用アニメーションの再生を管理する
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
    /// UIの開閉に応じた専用アニメーションの再生
    /// </summary>
    /// <param name="isUIOpen">UIが開いているかどうか</param>
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
            Observable.Timer(System.TimeSpan.FromSeconds(0.2)) // UI専用アニメーションのクロスフェード時間
                .Subscribe(_ =>
                {
                    if (!_playerManager.IsDead) // 死亡していない場合のみ更新
                    {
                        UpdateAnimator(playerState.Value); // プレイヤーの現在の状態に基づいてアニメーションを再設定
                    }
                }).AddTo(this);
        }
    }

    /// <summary>
    /// 入力によりアニメーション更新処理
    /// </summary>
    private void BindAnimations()
    {
        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding) // ガード中でない場合のみ実行
            .Select(_ => Input.GetAxis("Horizontal"))
            .Subscribe(horizontal =>
            {
                m_Animator.SetFloat("左右", horizontal);
                m_Animator.SetFloat("走り左右", horizontal);
            }).AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding) // ガード中でない場合のみ実行
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
