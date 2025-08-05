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

    // アニメーション制御用のフラグ
    private bool isInJumpSequence = false;
    private PlayerState lastGroundState = PlayerState.Idle; // 最後の地上状態を記録

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
    /// プレイヤーの状態に応じてアニメーターのパラメーター更新（ジャンプ保護強化版）
    /// </summary>
    /// <param name="state"></param>
    private void UpdateAnimator(PlayerState state)
    {
        Debug.Log($"UpdateAnimator called with state: {state}");

        if (state == PlayerState.Dead)
        {
            SetAllAnimationBools(false);
            m_Animator.SetBool("Dead", true);
            m_Animator.ResetTrigger("NormalAttack");
            m_Animator.ResetTrigger("StrongAttack");
            isInJumpSequence = false;
            return;
        }

        // ジャンプシーケンスの管理
        bool isJumpState = IsJumpState(state);

        if (isJumpState && !isInJumpSequence)
        {
            // ジャンプシーケンス開始：現在の地上状態を記録
            if (!IsJumpState(playerState.Value))
            {
                lastGroundState = playerState.Value;
            }
            isInJumpSequence = true;
            Debug.Log($"Jump sequence started, last ground state: {lastGroundState}");
        }
        else if (!isJumpState && isInJumpSequence)
        {
            // ジャンプシーケンス終了
            isInJumpSequence = false;
            Debug.Log("Jump sequence ended");
        }

        // ★修正：ジャンプ中は地上の移動状態への遷移を完全にブロック
        if (isInJumpSequence && IsGroundMovementState(state))
        {
            Debug.Log($"Blocking ground movement state {state} during jump sequence");
            return;
        }

        // JumpEnd中は他のジャンプ状態からの上書きを防ぐ
        if (playerState.Value == PlayerState.JumpEnd && state != PlayerState.JumpEnd && IsJumpState(state))
        {
            Debug.Log($"Preventing overwrite of JumpEnd with {state}");
            return;
        }

        // すべてのBoolパラメーターをリセット
        SetAllAnimationBools(false);

        // 状態に応じてアニメーションを設定
        switch (state)
        {
            case PlayerState.Idle:
                m_Animator.SetBool("Idle", true);
                break;
            case PlayerState.Walk:
                m_Animator.SetBool("Walk", true);
                break;
            case PlayerState.Run:
                m_Animator.SetBool("Run", true);
                break;
            case PlayerState.Guard:
                m_Animator.SetBool("Guard", true);
                break;
            case PlayerState.JumpStart:
                m_Animator.SetBool("JumpStart", true);
                Debug.Log("JumpStart animation activated");
                break;
            case PlayerState.JumpLoop:
                m_Animator.SetBool("JumpLoop", true);
                Debug.Log("JumpLoop animation activated");
                break;
            case PlayerState.JumpEnd:
                m_Animator.SetBool("JumpEnd", true);
                Debug.Log("JumpEnd animation activated - PROTECTED");
                break;
        }
    }

    /// <summary>
    /// ★新規追加：地上の移動状態かどうかを判定
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool IsGroundMovementState(PlayerState state)
    {
        return state == PlayerState.Idle ||
               state == PlayerState.Walk ||
               state == PlayerState.Run;
    }

    /// <summary>
    /// ジャンプ関連の状態かどうかを判定
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool IsJumpState(PlayerState state)
    {
        return state == PlayerState.JumpStart ||
               state == PlayerState.JumpLoop ||
               state == PlayerState.JumpEnd;
    }

    /// <summary>
    /// すべてのアニメーションBoolパラメーターをリセット
    /// </summary>
    private void SetAllAnimationBools(bool value)
    {
        m_Animator.SetBool("Idle", value);
        m_Animator.SetBool("Walk", value);
        m_Animator.SetBool("Run", value);
        m_Animator.SetBool("Guard", value);
        m_Animator.SetBool("Dead", value);
        m_Animator.SetBool("JumpStart", value);
        m_Animator.SetBool("JumpLoop", value);
        m_Animator.SetBool("JumpEnd", value);
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
            SetAllAnimationBools(false);
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
    /// 入力によりアニメーション更新処理（ジャンプ中は制限）
    /// </summary>
    private void BindAnimations()
    {
        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding && !isInJumpSequence) // ガード中またはジャンプ中でない場合のみ実行
            .Select(_ => Input.GetAxis("Horizontal"))
            .Subscribe(horizontal =>
            {
                m_Animator.SetFloat("左右", horizontal);
                m_Animator.SetFloat("走り左右", horizontal);
            }).AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding && !isInJumpSequence) // ガード中またはジャンプ中でない場合のみ実行
            .Select(_ => Input.GetAxis("Vertical"))
            .Subscribe(vertical =>
            {
                m_Animator.SetFloat("前後", vertical);
                m_Animator.SetFloat("走り前後", vertical);
            }).AddTo(this);
    }

    /// <summary>
    /// プレイヤーの状態更新（ジャンプ保護強化版）
    /// </summary>
    /// <param name="state">状態</param>
    public void UpdateState(PlayerState state)
    {
        // ★修正：ジャンプ中の状態遷移制限を強化
        if (isInJumpSequence)
        {
            // ジャンプ中は地上の移動状態への遷移を完全にブロック
            if (IsGroundMovementState(state))
            {
                Debug.Log($"Blocked ground state {state} during jump sequence");
                return;
            }

            // ジャンプ中でもガード状態への遷移はブロック（ジャンプ中はガードできない仕様）
            if (state == PlayerState.Guard)
            {
                Debug.Log($"Blocked Guard state during jump sequence");
                return;
            }
        }

        playerState.Value = state;
    }

    /// <summary>
    /// ジャンプ開始アニメーション完了時の処理（使用しない - フレーム数で制御）
    /// </summary>
    public void OnJumpStartComplete()
    {
        // アニメーションイベントは使用せず、PlayerControllerでフレーム数制御
        Debug.Log("JumpStart animation event (not used)");
    }

    /// <summary>
    /// 着地アニメーション完了時の処理（使用しない - 即座に遷移）
    /// </summary>
    public void OnJumpEndComplete()
    {
        // アニメーションイベントは使用せず、着地と同時に即座に状態遷移
        Debug.Log("JumpEnd animation event (not used)");
    }

    /// <summary>
    /// 現在ジャンプ関連のアニメーションを再生中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsPlayingJumpAnimation()
    {
        return _playerManager.IsJumping() || isInJumpSequence;
    }

    /// <summary>
    /// ジャンプシーケンス中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsInJumpSequence()
    {
        return isInJumpSequence;
    }

    /// <summary>
    /// ★新規追加：ジャンプシーケンスを強制終了（着地時用）
    /// </summary>
    public void EndJumpSequence()
    {
        isInJumpSequence = false;
        Debug.Log("Jump sequence ended manually");
    }

    /// <summary>
    /// 最後の地上状態を取得
    /// </summary>
    /// <returns></returns>
    public PlayerState GetLastGroundState()
    {
        return lastGroundState;
    }

    /// <summary>
    /// アニメーターのデバッグ情報を取得
    /// </summary>
    /// <returns></returns>
    public string GetAnimatorDebugInfo()
    {
        if (m_Animator == null) return "Animator is null";

        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        return $"Current State: {stateInfo.shortNameHash}, Time: {stateInfo.normalizedTime:F2}, Jump Sequence: {isInJumpSequence}";
    }

    /// <summary>
    /// 強制的にジャンプシーケンスを終了する（デバッグ用）
    /// </summary>
    public void ForceEndJumpSequence()
    {
        isInJumpSequence = false;
        Debug.Log("Jump sequence forcefully ended");
    }
}