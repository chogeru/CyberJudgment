using AbubuResouse.Singleton;
using UniRx;
using UniRx.Triggers;
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

    // アニメーション制御用のフラグ - 完全同期版
    private bool isInJumpSequence = false;
    private PlayerState lastGroundState = PlayerState.Idle; // 最後の地上状態を記録
    private float jumpSequenceStartTime = 0f; // ジャンプシーケンス開始時間

    // アニメーション遷移管理 - 改善版
    private PlayerState currentAnimationState = PlayerState.Idle;
    private float lastStateChangeTime = 0f;
    private const float MIN_STATE_DURATION = 0.05f; // 最短状態維持時間

    // ジャンプアニメーション専用管理 - 修正版
    private bool isJumpAnimationLocked = false;
    private float jumpAnimationLockTime = 0f;
    private const float JUMP_LOCK_DURATION = 0.02f; // ★修正: 0.05秒から0.02秒に短縮

    // ★追加: 緊急時の強制アニメーション更新フラグ
    private bool forceAnimationUpdate = false;

    // JumpEndアニメーション進行監視用
    private bool isMonitoringJumpEnd = false;
    private float jumpEndStartTime = 0f;

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

    void Update()
    {
        UpdateJumpAnimationLock();
        MonitorJumpEndAnimation();
    }

    /// <summary>
    /// ジャンプアニメーションロックの更新 - 修正版
    /// </summary>
    private void UpdateJumpAnimationLock()
    {
        if (isJumpAnimationLocked)
        {
            float lockElapsed = Time.time - jumpAnimationLockTime;
            // ★修正: ロック時間を0.02秒に短縮
            if (lockElapsed >= 0.02f)
            {
                isJumpAnimationLocked = false;
                Debug.Log("短縮ジャンプアニメーションロック解除");
            }
        }
    }

    /// <summary>
    /// JumpEndアニメーションの進行を監視 - 新規追加
    /// </summary>
    private void MonitorJumpEndAnimation()
    {
        if (!isMonitoringJumpEnd || m_Animator == null) return;

        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

        // JumpEndアニメーションかどうかをチェック
        if (stateInfo.IsTag("JumpEnd") || stateInfo.shortNameHash == Animator.StringToHash("JumpEnd"))
        {
            // 7割再生で着地完了可能フラグを立てる（PlayerControllerで参照される）
            if (stateInfo.normalizedTime >= 0.7f)
            {
                Debug.Log($"JumpEndアニメーション7割再生完了: {stateInfo.normalizedTime:F2}");
            }
        }
        else
        {
            // JumpEndアニメーションが終了した場合は監視停止
            isMonitoringJumpEnd = false;
        }
    }

    /// <summary>
    /// プレイヤーの状態に応じてアニメーターのパラメーター更新 - 修正版
    /// </summary>
    private void UpdateAnimator(PlayerState state)
    {
        Debug.Log($"UpdateAnimator called: {state}, Current: {currentAnimationState}, InJumpSeq: {isInJumpSequence}");

        // 死亡状態の特別処理
        if (state == PlayerState.Dead)
        {
            ForceEndJumpSequence();
            ForceUpdateAnimator(state);
            return;
        }

        // ジャンプシーケンスの管理
        bool isJumpState = IsJumpState(state);

        if (isJumpState)
        {
            if (!isInJumpSequence)
            {
                StartJumpSequenceInternal();
            }

            if (state == PlayerState.JumpEnd)
            {
                isMonitoringJumpEnd = true;
                jumpEndStartTime = Time.time;
            }

            ForceUpdateAnimatorForJump(state);
        }
        else
        {
            if (isInJumpSequence)
            {
                EndJumpSequenceInternal();
            }

            // ★修正: ジャンプ終了後は短時間で通常状態に遷移
            if (!isInJumpSequence)
            {
                // ★追加: ジャンプ終了直後は即座に更新
                if (isJumpAnimationLocked)
                {
                    float lockElapsed = Time.time - jumpAnimationLockTime;
                    if (lockElapsed < 0.02f) // 0.02秒以内なら即座に更新
                    {
                        Debug.Log($"ジャンプ終了直後のため即座に{state}に遷移");
                        isJumpAnimationLocked = false; // ロックを解除
                        ForceUpdateAnimator(state);
                        return;
                    }
                }

                UpdateAnimatorNormal(state);
            }
            else if (isJumpAnimationLocked)
            {
                Debug.Log($"短縮ジャンプアニメーションロック中のため {state} への遷移を遅延");
                StartCoroutine(DelayedStateUpdate(state, 0.02f)); // 0.02秒で遅延
            }
        }
    }

    /// <summary>
    /// ジャンプ用の即座アニメーション更新
    /// </summary>
    private void ForceUpdateAnimatorForJump(PlayerState state)
    {
        // ジャンプアニメーションは最優先で即座に適用
        currentAnimationState = state;
        lastStateChangeTime = Time.time;

        // すべてのBoolパラメーターをリセット
        SetAllAnimationBools(false);

        // ジャンプ状態に応じてアニメーションを設定
        switch (state)
        {
            case PlayerState.JumpStart:
                m_Animator.SetBool("JumpStart", true);
                Debug.Log("JumpStartアニメーション即座適用");
                break;
            case PlayerState.JumpLoop:
                m_Animator.SetBool("JumpLoop", true);
                Debug.Log("JumpLoopアニメーション即座適用");
                break;
            case PlayerState.JumpEnd:
                m_Animator.SetBool("JumpEnd", true);
                Debug.Log("JumpEndアニメーション即座適用");
                break;
        }
    }

    /// <summary>
    /// 通常状態のアニメーション更新
    /// </summary>
    private void UpdateAnimatorNormal(PlayerState state)
    {
        // 状態変化の最小間隔チェック
        float timeSinceLastChange = Time.time - lastStateChangeTime;
        if (timeSinceLastChange < MIN_STATE_DURATION && currentAnimationState != PlayerState.Idle)
        {
            Debug.Log($"状態変化が早すぎるため遅延: {currentAnimationState} → {state}");
            StartCoroutine(DelayedStateUpdate(state, MIN_STATE_DURATION - timeSinceLastChange));
            return;
        }

        ForceUpdateAnimator(state);
    }

    /// <summary>
    /// 遅延状態更新のコルーチン
    /// </summary>
    private IEnumerator DelayedStateUpdate(PlayerState state, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 遅延後も同じ状態が要求されているかチェック
        if (playerState.Value == state && !isInJumpSequence)
        {
            ForceUpdateAnimator(state);
        }
    }

    /// <summary>
    /// ジャンプシーケンス開始（内部処理）
    /// </summary>
    private void StartJumpSequenceInternal()
    {
        if (!IsJumpState(currentAnimationState))
        {
            lastGroundState = currentAnimationState;
        }

        isInJumpSequence = true;
        jumpSequenceStartTime = Time.time;

        Debug.Log($"ジャンプシーケンス開始, 最後の地上状態: {lastGroundState}");
    }

    /// <summary>
    /// ジャンプシーケンス終了（内部処理） - 修正版
    /// </summary>
    private void EndJumpSequenceInternal()
    {
        isInJumpSequence = false;
        isMonitoringJumpEnd = false;

        // ★修正: アニメーションロック時間を大幅短縮（0.02秒）
        isJumpAnimationLocked = true;
        jumpAnimationLockTime = Time.time;

        // ★追加: JumpEndアニメーションを明示的に無効化
        m_Animator.SetBool("JumpEnd", false);

        Debug.Log("ジャンプシーケンス終了、短縮アニメーションロック開始");
    }

    /// <summary>
    /// 強制的にアニメーター更新
    /// </summary>
    private void ForceUpdateAnimator(PlayerState state)
    {
        currentAnimationState = state;
        lastStateChangeTime = Time.time;

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
                Debug.Log("JumpStartアニメーション強制設定");
                break;
            case PlayerState.JumpLoop:
                m_Animator.SetBool("JumpLoop", true);
                Debug.Log("JumpLoopアニメーション強制設定");
                break;
            case PlayerState.JumpEnd:
                m_Animator.SetBool("JumpEnd", true);
                Debug.Log("JumpEndアニメーション強制設定");
                break;
            case PlayerState.Dead:
                m_Animator.SetBool("Dead", true);
                m_Animator.ResetTrigger("NormalAttack");
                m_Animator.ResetTrigger("StrongAttack");
                break;
        }

        Debug.Log($"アニメーション強制更新完了: {state}");
    }

    /// <summary>
    /// 地上の移動状態かどうかを判定
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
    /// すべてのアニメーションBoolパラメーターをリセット - 修正版
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

        // ★追加: JumpEndを確実に無効化
        if (!value)
        {
            Debug.Log("JumpEndアニメーションを明示的に無効化");
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
            SetAllAnimationBools(false);
        }
        else
        {
            Observable.Timer(System.TimeSpan.FromSeconds(0.2))
                .Subscribe(_ =>
                {
                    if (!_playerManager.IsDead)
                    {
                        ForceUpdateAnimator(playerState.Value);
                    }
                }).AddTo(this);
        }
    }

    /// <summary>
    /// 入力によりアニメーション更新処理
    /// </summary>
    private void BindAnimations()
    {
        // 水平入力の処理
        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding && !ShouldBlockInputAnimation())
            .Select(_ => Input.GetAxis("Horizontal"))
            .Subscribe(horizontal =>
            {
                m_Animator.SetFloat("左右", horizontal);
                m_Animator.SetFloat("走り左右", horizontal);
            }).AddTo(this);

        // 垂直入力の処理
        this.UpdateAsObservable()
            .Where(_ => !_playerManager.IsGuarding && !ShouldBlockInputAnimation())
            .Select(_ => Input.GetAxis("Vertical"))
            .Subscribe(vertical =>
            {
                m_Animator.SetFloat("前後", vertical);
                m_Animator.SetFloat("走り前後", vertical);
            }).AddTo(this);
    }

    /// <summary>
    /// 入力アニメーションをブロックすべきかどうかを判定
    /// </summary>
    /// <returns></returns>
    private bool ShouldBlockInputAnimation()
    {
        // ジャンプアニメーションロック中は入力アニメーションをブロック
        if (isJumpAnimationLocked)
        {
            return true;
        }

        // ジャンプシーケンス中も入力アニメーションは制限（JumpEnd以外）
        if (isInJumpSequence && currentAnimationState != PlayerState.JumpEnd)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// プレイヤーの状態更新 - 即座反映版
    /// </summary>
    /// <param name="state">状態</param>
    public void UpdateState(PlayerState state)
    {
        Debug.Log($"UpdateState called: {state}, Current: {playerState.Value}");

        // 同じ状態への重複更新を防ぐ
        if (playerState.Value == state)
        {
            Debug.Log($"同じ状態のため更新スキップ: {state}");
            return;
        }

        // ジャンプ状態は最優先で処理
        if (IsJumpState(state))
        {
            playerState.Value = state;
            Debug.Log($"ジャンプ状態更新完了: {state}");
            return;
        }

        // ジャンプアニメーションロック中は地上状態の遷移を遅延（短縮）
        if (isJumpAnimationLocked && IsGroundMovementState(state))
        {
            Debug.Log($"ジャンプアニメーションロック中のため地上状態 {state} への遷移を遅延");
            StartCoroutine(DelayedStateUpdate(state, 0.02f)); // 0.02秒で遅延
            return;
        }

        // ジャンプ中の状態遷移制限
        if (isInJumpSequence)
        {
            // ジャンプ中は地上の移動状態への遷移を完全にブロック
            if (IsGroundMovementState(state))
            {
                Debug.Log($"ジャンプシーケンス中のため地上状態 {state} への遷移をブロック");
                return;
            }

            // ジャンプ中でもガード状態への遷移はブロック
            if (state == PlayerState.Guard)
            {
                Debug.Log($"ジャンプシーケンス中のためガード状態への遷移をブロック");
                return;
            }
        }

        // 状態更新実行
        playerState.Value = state;
        Debug.Log($"状態更新完了: {state}");
    }

    /// <summary>
    /// ジャンプシーケンス開始通知（PlayerControllerから呼ばれる）
    /// </summary>
    public void StartJumpSequence()
    {
        Debug.Log("ジャンプシーケンス開始通知受信");

        // 現在の地上状態を記録
        if (!isInJumpSequence && IsGroundMovementState(playerState.Value))
        {
            lastGroundState = playerState.Value;
        }

        // ジャンプアニメーションを最優先で適用するため、ロックを一時解除
        isJumpAnimationLocked = false;
    }

    /// <summary>
    /// ジャンプシーケンスを手動終了（着地時用） - 修正版
    /// </summary>
    public void EndJumpSequence()
    {
        if (isInJumpSequence)
        {
            Debug.Log("ジャンプシーケンス手動終了");
            EndJumpSequenceInternal();

            // ★追加: JumpEndアニメーションを即座に停止
            m_Animator.SetBool("JumpEnd", false);

            // ★追加: 次回の状態更新を強制的に実行
            forceAnimationUpdate = true;
        }
    }

    /// <summary>
    /// ジャンプシーケンスを強制終了（緊急時用）
    /// </summary>
    private void ForceEndJumpSequence()
    {
        if (isInJumpSequence)
        {
            isInJumpSequence = false;
            isJumpAnimationLocked = false;
            isMonitoringJumpEnd = false;
            Debug.Log("ジャンプシーケンス強制終了");
        }
    }

    /// <summary>
    /// アニメーションロックを強制解除 - 修正版
    /// </summary>
    public void ForceUnlockAnimation()
    {
        isJumpAnimationLocked = false;
        forceAnimationUpdate = true;

        // ★追加: すべてのジャンプアニメーションを確実に停止
        m_Animator.SetBool("JumpStart", false);
        m_Animator.SetBool("JumpLoop", false);
        m_Animator.SetBool("JumpEnd", false);

        Debug.Log("アニメーションロック強制解除 + ジャンプアニメーション停止");
    }

    /// <summary>
    /// 強制的にアニメーション更新（着地完了時専用） - 新規追加
    /// </summary>
    public void ForceUpdateAnimationImmediate(PlayerState state)
    {
        Debug.Log($"強制アニメーション更新: {state}");

        // ★重要: アニメーションロックを一時的に無視
        bool wasLocked = isJumpAnimationLocked;
        isJumpAnimationLocked = false;

        // 現在の状態を強制更新
        currentAnimationState = state;
        lastStateChangeTime = Time.time;

        // すべてのBoolパラメーターを確実にリセット
        SetAllAnimationBools(false);

        // 新しい状態を即座に設定
        switch (state)
        {
            case PlayerState.Idle:
                m_Animator.SetBool("Idle", true);
                Debug.Log("強制的にIdleアニメーション設定");
                break;
            case PlayerState.Walk:
                m_Animator.SetBool("Walk", true);
                Debug.Log("強制的にWalkアニメーション設定");
                break;
            case PlayerState.Run:
                m_Animator.SetBool("Run", true);
                Debug.Log("強制的にRunアニメーション設定");
                break;
            case PlayerState.Guard:
                m_Animator.SetBool("Guard", true);
                Debug.Log("強制的にGuardアニメーション設定");
                break;
        }

        // ReactivePropertyも強制更新
        playerState.Value = state;

        Debug.Log($"強制アニメーション更新完了: {state}");
    }

    /// <summary>
    /// JumpEndアニメーション状態の詳細チェック（デバッグ用）
    /// </summary>
    public void CheckJumpEndAnimationState()
    {
        if (m_Animator == null) return;

        bool jumpEndParam = m_Animator.GetBool("JumpEnd");
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);
        bool isPlayingJumpEnd = currentState.IsTag("JumpEnd") || currentState.shortNameHash == Animator.StringToHash("JumpEnd");

        Debug.Log($"=== JumpEndアニメーション状態チェック ===");
        Debug.Log($"JumpEndパラメーター: {jumpEndParam}");
        Debug.Log($"JumpEndアニメーション再生中: {isPlayingJumpEnd}");
        Debug.Log($"現在のアニメーション: {currentState.shortNameHash}");
        Debug.Log($"正規化時間: {currentState.normalizedTime:F3}");
        Debug.Log($"ジャンプシーケンス中: {isInJumpSequence}");
        Debug.Log($"アニメーションロック中: {isJumpAnimationLocked}");
        Debug.Log($"現在の状態: {currentAnimationState}");
        Debug.Log($"ReactiveProperty状態: {playerState.Value}");
        Debug.Log($"===============================");

        // JumpEndが残っている場合は強制修正
        if (jumpEndParam && !isInJumpSequence && currentAnimationState != PlayerState.JumpEnd)
        {
            Debug.LogWarning("JumpEndパラメーターが残っています。強制修正します。");
            m_Animator.SetBool("JumpEnd", false);
            ForceUpdateAnimationImmediate(PlayerState.Idle);
        }
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
    /// アニメーションロック中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsAnimationLocked()
    {
        return isJumpAnimationLocked;
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
    /// ジャンプシーケンスの経過時間を取得
    /// </summary>
    /// <returns></returns>
    public float GetJumpSequenceElapsedTime()
    {
        if (!isInJumpSequence) return 0f;
        return Time.time - jumpSequenceStartTime;
    }

    /// <summary>
    /// Animatorの参照を取得（PlayerControllerで使用）
    /// </summary>
    /// <returns></returns>
    public Animator GetAnimator()
    {
        return m_Animator;
    }

    /// <summary>
    /// JumpEndアニメーションの進行度を取得（PlayerControllerで使用）
    /// </summary>
    /// <returns></returns>
    public float GetJumpEndProgress()
    {
        if (m_Animator == null) return 0f;

        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

        // JumpEndアニメーションかどうかをチェック
        if (stateInfo.IsTag("JumpEnd") || stateInfo.shortNameHash == Animator.StringToHash("JumpEnd"))
        {
            return stateInfo.normalizedTime;
        }

        return 0f;
    }

    /// <summary>
    /// JumpEndアニメーションが7割再生されているかチェック
    /// </summary>
    /// <returns></returns>
    public bool IsJumpEndReady()
    {
        return GetJumpEndProgress() >= 0.7f;
    }

    /// <summary>
    /// アニメーターのデバッグ情報を取得
    /// </summary>
    /// <returns></returns>
    public string GetAnimatorDebugInfo()
    {
        if (m_Animator == null) return "Animator is null";

        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        return $"Current State: {stateInfo.shortNameHash}, Time: {stateInfo.normalizedTime:F2}, " +
               $"Jump Sequence: {isInJumpSequence}, Anim Locked: {isJumpAnimationLocked}, " +
               $"Current Anim State: {currentAnimationState}, Sequence Time: {GetJumpSequenceElapsedTime():F2}";
    }

    /// <summary>
    /// ジャンプ状態のリセット（緊急時用） - 修正版
    /// </summary>
    public void ResetJumpState()
    {
        isInJumpSequence = false;
        isJumpAnimationLocked = false;
        jumpSequenceStartTime = 0f;
        isMonitoringJumpEnd = false;
        jumpEndStartTime = 0f;

        // 適切な地上状態に戻す
        if (_playerManager.IsGuarding)
        {
            playerState.Value = PlayerState.Guard;
        }
        else
        {
            playerState.Value = PlayerState.Idle;
        }

        // アニメーターも強制更新
        ForceUpdateAnimator(playerState.Value);

        Debug.Log("ジャンプ状態を完全にリセット");
    }

    /// <summary>
    /// 現在のアニメーション状態の詳細情報を出力（デバッグ用）
    /// </summary>
    public void LogCurrentAnimationState()
    {
        if (m_Animator == null)
        {
            Debug.Log("Animator が null です");
            return;
        }

        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextState = m_Animator.GetNextAnimatorStateInfo(0);

        Debug.Log($"=== アニメーション状態詳細 ===");
        Debug.Log($"現在の状態: {currentState.shortNameHash} (正規化時間: {currentState.normalizedTime:F3})");

        if (m_Animator.IsInTransition(0))
        {
            Debug.Log($"遷移中: → {nextState.shortNameHash} (正規化時間: {nextState.normalizedTime:F3})");
        }

        Debug.Log($"プレイヤー状態: {playerState.Value}");
        Debug.Log($"アニメーション状態: {currentAnimationState}");
        Debug.Log($"ジャンプシーケンス: {isInJumpSequence}");
        Debug.Log($"アニメーションロック: {isJumpAnimationLocked}");
        Debug.Log($"JumpEnd監視中: {isMonitoringJumpEnd}");
        Debug.Log($"JumpEnd進行度: {GetJumpEndProgress():F3}");
        Debug.Log($"シーケンス経過時間: {GetJumpSequenceElapsedTime():F2}秒");
        Debug.Log($"最後の状態変更時間: {Time.time - lastStateChangeTime:F2}秒前");
        Debug.Log($"========================");
    }

    /// <summary>
    /// アニメーターパラメーターの現在値を確認（デバッグ用）
    /// </summary>
    public void LogAnimatorParameters()
    {
        if (m_Animator == null)
        {
            Debug.Log("Animator が null です");
            return;
        }

        Debug.Log($"=== アニメーターパラメーター ===");
        Debug.Log($"Idle: {m_Animator.GetBool("Idle")}");
        Debug.Log($"Walk: {m_Animator.GetBool("Walk")}");
        Debug.Log($"Run: {m_Animator.GetBool("Run")}");
        Debug.Log($"Guard: {m_Animator.GetBool("Guard")}");
        Debug.Log($"JumpStart: {m_Animator.GetBool("JumpStart")}");
        Debug.Log($"JumpLoop: {m_Animator.GetBool("JumpLoop")}");
        Debug.Log($"JumpEnd: {m_Animator.GetBool("JumpEnd")}");
        Debug.Log($"左右: {m_Animator.GetFloat("左右"):F2}");
        Debug.Log($"前後: {m_Animator.GetFloat("前後"):F2}");
        Debug.Log($"========================");
    }

    /// <summary>
    /// 完全なアニメーション同期リセット（緊急時用） - 修正版
    /// </summary>
    public void EmergencyAnimationReset()
    {
        // すべての状態をリセット
        isInJumpSequence = false;
        isJumpAnimationLocked = false;
        jumpSequenceStartTime = 0f;
        lastStateChangeTime = 0f;
        currentAnimationState = PlayerState.Idle;
        isMonitoringJumpEnd = false;
        jumpEndStartTime = 0f;

        // アニメーターをクリア
        SetAllAnimationBools(false);

        // アイドル状態に強制設定
        m_Animator.SetBool("Idle", true);
        playerState.Value = PlayerState.Idle;

        Debug.Log("緊急アニメーション同期リセット実行");
    }

    /// <summary>
    /// ジャンプアニメーションの即座遷移テスト（デバッグ用）
    /// </summary>
    public void TestImmediateJumpTransition()
    {
        Debug.Log("ジャンプアニメーション即座遷移テスト開始");

        // JumpStart → JumpLoop → JumpEnd の順番でテスト
        StartCoroutine(JumpAnimationTestSequence());
    }

    /// <summary>
    /// ジャンプアニメーションテストシーケンス
    /// </summary>
    private IEnumerator JumpAnimationTestSequence()
    {
        // JumpStart
        ForceUpdateAnimatorForJump(PlayerState.JumpStart);
        yield return new WaitForSeconds(0.2f);

        // JumpLoop
        ForceUpdateAnimatorForJump(PlayerState.JumpLoop);
        yield return new WaitForSeconds(0.5f);

        // JumpEnd
        ForceUpdateAnimatorForJump(PlayerState.JumpEnd);
        yield return new WaitForSeconds(0.3f);

        // Idle
        ForceUpdateAnimator(PlayerState.Idle);

        Debug.Log("ジャンプアニメーション即座遷移テスト完了");
    }
}