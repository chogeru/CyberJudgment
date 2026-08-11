using UniRx;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerController PlayerController { get; private set; }
    public PlayerAnimationController PlayerAnimationController { get; private set; }
    public PlayerAttackController PlayerAttackController { get; private set; }
    public PlayerMP playerMP { get; private set; }

    private bool isAttacking;
    private bool isHit;
    private bool isDead;
    private readonly ReactiveProperty<bool> isGuarding = new ReactiveProperty<bool>(false);

    public bool IsHit => isHit;
    public bool IsGuarding => isGuarding.Value;
    public bool IsDead => isDead;

    private PlayerState currentState;
    private PlayerState previousState; // 前の状態を記録（デバッグ用）
    private float lastStateChangeTime = 0f; // 最後の状態変更時間

    // ジャンプ関連の追加フラグ
    private bool isInJumpSequence = false;
    private float jumpSequenceStartTime = 0f;

    private void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        PlayerAnimationController = GetComponent<PlayerAnimationController>();
        PlayerAttackController = GetComponent<PlayerAttackController>();
        playerMP = GetComponent<PlayerMP>();

        currentState = PlayerState.Idle;
        previousState = PlayerState.Idle;
        lastStateChangeTime = Time.time;
        isInJumpSequence = false;
        jumpSequenceStartTime = 0f;
    }

    /// <summary>
    /// ガード状態を設定します。
    /// </summary>
    public void SetGuarding(bool guarding)
    {
        isGuarding.Value = guarding;
        PlayerController.SetMovementEnabled(!guarding);
        PlayerAttackController.SetAttackEnabled(!guarding);
    }

    /// <summary>
    /// プレイヤーの状態更新 - 改善版（ジャンプ優先処理＋着地判定改善）
    /// </summary>
    /// <param name="state"></param>
    public void UpdatePlayerState(PlayerState state)
    {
        // 同じ状態への重複更新を防ぐ
        if (currentState == state)
        {
            return;
        }

        // 状態遷移のログ出力（デバッグ用）
        Debug.Log($"PlayerManager状態遷移: {currentState} → {state}");

        // ジャンプ状態の特別な処理
        if (IsJumpState(state))
        {
            // ジャンプシーケンス開始の管理
            if (!isInJumpSequence)
            {
                isInJumpSequence = true;
                jumpSequenceStartTime = Time.time;
                Debug.Log("PlayerManager: ジャンプシーケンス開始");
            }

            // ジャンプ状態は最優先で処理（遷移チェックをスキップ）
            ExecuteStateChange(state);
            return;
        }

        // ジャンプシーケンス終了の判定
        if (isInJumpSequence && !IsJumpState(state))
        {
            isInJumpSequence = false;
            Debug.Log($"PlayerManager: ジャンプシーケンス終了 → {state}");
        }

        // 不正な状態遷移をチェック（ジャンプ以外）
        if (!IsValidStateTransition(currentState, state))
        {
            Debug.LogWarning($"不正な状態遷移を検出: {currentState} → {state}");
            return;
        }

        ExecuteStateChange(state);
    }

    /// <summary>
    /// 状態変更の実行 - 新規メソッド
    /// </summary>
    private void ExecuteStateChange(PlayerState state)
    {
        previousState = currentState;
        currentState = state;
        lastStateChangeTime = Time.time;

        // アニメーションコントローラーに状態を通知
        PlayerAnimationController.UpdateState(state);

        Debug.Log($"PlayerManager状態変更実行完了: {previousState} → {currentState}");
    }

    /// <summary>
    /// 状態遷移が有効かどうかをチェック - 改善版
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private bool IsValidStateTransition(PlayerState from, PlayerState to)
    {
        // 死亡状態からは死亡状態またはアイドル状態にのみ遷移可能
        if (from == PlayerState.Dead && to != PlayerState.Dead && to != PlayerState.Idle)
        {
            return false;
        }

        // ジャンプ状態は別途処理されるため、ここでは常に有効
        if (IsJumpState(to))
        {
            return true;
        }

        // ジャンプ中の状態遷移制限をチェック（緩和版）
        bool isFromJumpState = IsJumpState(from);
        bool isToGroundState = IsGroundMovementState(to);

        // ジャンプ状態から地上移動状態への直接遷移の制限を緩和
        if (isFromJumpState && isToGroundState)
        {
            // JumpEndからの遷移は常に許可（着地完了として扱う）
            if (from == PlayerState.JumpEnd)
            {
                return true;
            }

            // JumpStartやJumpLoopからの直接遷移は基本的に無効だが、
            // 緊急時のリセットなどを考慮して警告レベルにとどめる
            Debug.LogWarning($"ジャンプ中から地上状態への遷移: {from} → {to}");
            return false;
        }

        // ガード状態の遷移制限（緩和版）
        if (to == PlayerState.Guard)
        {
            // ジャンプ中はガードできないが、JumpEndは着地扱いなので許可
            if (isFromJumpState && from != PlayerState.JumpEnd)
            {
                return false;
            }
        }

        return true;
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
    /// 地上移動状態かどうかを判定
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
    /// プレイヤーの現在の状態を取得
    /// </summary>
    public PlayerState CurrentState => currentState;

    /// <summary>
    /// 前の状態を取得（デバッグ用）
    /// </summary>
    public PlayerState PreviousState => previousState;

    /// <summary>
    /// 最後の状態変更からの経過時間を取得
    /// </summary>
    public float TimeSinceLastStateChange => Time.time - lastStateChangeTime;

    /// <summary>
    /// ジャンプ中かどうかを判定 - 改善版
    /// </summary>
    /// <returns></returns>
    public bool IsJumping()
    {
        return IsJumpState(currentState);
    }

    /// <summary>
    /// ジャンプシーケンス中かどうかを判定 - 新規追加
    /// </summary>
    /// <returns></returns>
    public bool IsInJumpSequence()
    {
        return isInJumpSequence;
    }

    /// <summary>
    /// ジャンプシーケンスの経過時間を取得 - 新規追加
    /// </summary>
    /// <returns></returns>
    public float GetJumpSequenceElapsedTime()
    {
        if (!isInJumpSequence) return 0f;
        return Time.time - jumpSequenceStartTime;
    }

    /// <summary>
    /// 空中にいるかどうかを判定
    /// </summary>
    /// <returns></returns>
    public bool IsAirborne()
    {
        return IsJumping();
    }

    /// <summary>
    /// 地上にいるかどうかを判定 - 改善版
    /// </summary>
    /// <returns></returns>
    public bool IsOnGround()
    {
        return IsGroundMovementState(currentState) || currentState == PlayerState.Guard;
    }

    /// <summary>
    /// ジャンプ開始状態かどうかを判定 - 新規メソッド
    /// </summary>
    /// <returns></returns>
    public bool IsJumpStarting()
    {
        return currentState == PlayerState.JumpStart;
    }

    /// <summary>
    /// ジャンプループ状態かどうかを判定 - 新規メソッド
    /// </summary>
    /// <returns></returns>
    public bool IsJumpLooping()
    {
        return currentState == PlayerState.JumpLoop;
    }

    /// <summary>
    /// 着地中かどうかを判定 - 新規メソッド
    /// </summary>
    /// <returns></returns>
    public bool IsLanding()
    {
        return currentState == PlayerState.JumpEnd;
    }

    /// <summary>
    /// 着地完了可能かどうかを判定 - 新規メソッド
    /// </summary>
    /// <returns></returns>
    public bool CanCompleteLanding()
    {
        if (!IsLanding()) return false;

        // アニメーションの進行度をチェック
        if (PlayerAnimationController != null)
        {
            float progress = PlayerAnimationController.GetJumpEndProgress();
            return progress >= 0.7f; // 7割再生で着地完了可能
        }

        return false;
    }

    /// <summary>
    /// 着地後クールタイム中かどうかを判定 - 新規追加
    /// </summary>
    /// <returns></returns>
    public bool IsInLandingCooldown()
    {
        return PlayerController.IsInLandingCooldown();
    }

    /// <summary>
    /// 残りクールタイム時間を取得 - 新規追加
    /// </summary>
    /// <returns></returns>
    public float GetRemainingLandingCooldown()
    {
        return PlayerController.GetRemainingLandingCooldown();
    }

    /// <summary>
    /// 攻撃中フラグの設定
    /// </summary>
    /// <param name="isAttacking"></param>
    public void SetAttacking(bool isAttacking)
    {
        this.isAttacking = isAttacking;
        PlayerController.SetMovementEnabled(!isAttacking);
    }

    /// <summary>
    /// 攻撃中であるかどうかを確認
    /// </summary>
    /// <returns></returns>
    public bool IsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// ヒット状態の設定
    /// </summary>
    /// <param name="hit"></param>
    public void SetHitState(bool hit)
    {
        isHit = hit;
        PlayerController.SetMovementEnabled(!hit);
        PlayerAttackController.SetAttackEnabled(!hit);
    }

    /// <summary>
    /// 死亡状態の設定
    /// </summary>
    /// <param name="dead"></param>
    public void SetDeadState(bool dead)
    {
        isDead = dead;
        PlayerController.SetMovementEnabled(!dead);
        PlayerAttackController.SetAttackEnabled(!dead);

        if (dead)
        {
            // 死亡時はジャンプシーケンスも強制終了
            isInJumpSequence = false;
            PlayerAnimationController.EndJumpSequence();
            UpdatePlayerState(PlayerState.Dead);
        }
        else
        {
            UpdatePlayerState(PlayerState.Idle);
        }
    }

    /// <summary>
    /// 状態を強制的にリセット - 修正版（クールタイムリセット追加）
    /// </summary>
    public void ForceResetState()
    {
        // すべての状態をリセット
        isAttacking = false;
        isHit = false;
        isGuarding.Value = false;
        isInJumpSequence = false;
        jumpSequenceStartTime = 0f;

        // クールタイムもリセット
        PlayerController.ResetLandingCooldown();

        // アニメーションも含めて完全リセット
        PlayerAnimationController.ResetJumpState();

        // 移動と攻撃を有効化
        PlayerController.SetMovementEnabled(true);
        PlayerAttackController.SetAttackEnabled(true);

        // アイドル状態に設定
        UpdatePlayerState(PlayerState.Idle);

        Debug.Log("プレイヤー状態を強制リセット（クールタイム含む）");
    }

    /// <summary>
    /// ジャンプ状態を強制的にリセット - 改善版
    /// </summary>
    public void ForceResetJumpState()
    {
        // ジャンプ関連の状態をリセット
        isInJumpSequence = false;
        jumpSequenceStartTime = 0f;
        PlayerAnimationController.ResetJumpState();

        // 適切な地上状態に遷移
        if (IsGuarding)
        {
            UpdatePlayerState(PlayerState.Guard);
        }
        else
        {
            UpdatePlayerState(PlayerState.Idle);
        }

        Debug.Log("ジャンプ状態を強制リセット");
    }

    /// <summary>
    /// 着地処理の強制実行 - 新規メソッド
    /// </summary>
    public void ForceCompleteLanding()
    {
        if (IsJumping())
        {
            Debug.Log("着地処理を強制実行");

            // ジャンプシーケンス終了
            isInJumpSequence = false;
            PlayerAnimationController.EndJumpSequence();

            // 適切な地上状態に遷移
            if (IsGuarding)
            {
                UpdatePlayerState(PlayerState.Guard);
            }
            else
            {
                UpdatePlayerState(PlayerState.Idle);
            }
        }
    }

    /// <summary>
    /// 現在の状態情報を詳細に出力 - 修正版（クールタイム情報追加）
    /// </summary>
    public void LogPlayerStateInfo()
    {
        Debug.Log($"=== プレイヤー状態情報 ===");
        Debug.Log($"現在の状態: {currentState}");
        Debug.Log($"前の状態: {previousState}");
        Debug.Log($"状態変更からの経過時間: {TimeSinceLastStateChange:F2}秒");
        Debug.Log($"ジャンプ中: {IsJumping()}");
        Debug.Log($"ジャンプシーケンス中: {IsInJumpSequence()}");
        Debug.Log($"ジャンプシーケンス経過時間: {GetJumpSequenceElapsedTime():F2}秒");

        // クールタイム情報
        Debug.Log($"着地後クールタイム中: {IsInLandingCooldown()}");
        if (IsInLandingCooldown())
        {
            Debug.Log($"残りクールタイム: {GetRemainingLandingCooldown():F1}秒");
        }

        Debug.Log($"ジャンプ開始: {IsJumpStarting()}");
        Debug.Log($"ジャンプループ: {IsJumpLooping()}");
        Debug.Log($"着地中: {IsLanding()}");
        Debug.Log($"着地完了可能: {CanCompleteLanding()}");
        Debug.Log($"空中: {IsAirborne()}");
        Debug.Log($"地上: {IsOnGround()}");
        Debug.Log($"攻撃中: {isAttacking}");
        Debug.Log($"ヒット中: {isHit}");
        Debug.Log($"ガード中: {IsGuarding}");
        Debug.Log($"死亡: {isDead}");
        Debug.Log($"==================");

        // アニメーション情報も出力
        PlayerAnimationController.LogCurrentAnimationState();
    }

    /// <summary>
    /// ジャンプ状態遷移の詳細ログ - 改善版
    /// </summary>
    public void LogJumpStateTransition(PlayerState newState)
    {
        if (IsJumpState(newState) || IsJumpState(currentState))
        {
            Debug.Log($"=== ジャンプ状態遷移ログ ===");
            Debug.Log($"遷移: {currentState} → {newState}");
            Debug.Log($"時間: {Time.time:F3}");
            Debug.Log($"経過時間: {TimeSinceLastStateChange:F3}秒");
            Debug.Log($"ジャンプシーケンス中: {IsInJumpSequence()}");
            Debug.Log($"ジャンプシーケンス経過: {GetJumpSequenceElapsedTime():F3}秒");
            Debug.Log($"アニメーションシーケンス中: {PlayerAnimationController.IsInJumpSequence()}");
            Debug.Log($"アニメーションロック中: {PlayerAnimationController.IsAnimationLocked()}");

            if (newState == PlayerState.JumpEnd)
            {
                Debug.Log($"JumpEnd進行度: {PlayerAnimationController.GetJumpEndProgress():F3}");
                Debug.Log($"着地完了可能: {CanCompleteLanding()}");
            }

            Debug.Log($"========================");
        }
    }

    /// <summary>
    /// 状態遷移の妥当性を検証（デバッグ用） - 新規メソッド
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void ValidateStateTransition(PlayerState from, PlayerState to)
    {
        bool isValid = IsValidStateTransition(from, to);

        if (!isValid)
        {
            Debug.LogError($"無効な状態遷移が検出されました: {from} → {to}");
            Debug.LogError($"現在時刻: {Time.time:F3}, 前回変更からの経過: {TimeSinceLastStateChange:F3}秒");
            Debug.LogError($"ジャンプシーケンス中: {IsInJumpSequence()}");

            // スタックトレースも出力
            Debug.LogError($"スタックトレース:\n{System.Environment.StackTrace}");
        }
        else
        {
            Debug.Log($"有効な状態遷移: {from} → {to}");
        }
    }

    /// <summary>
    /// ジャンプアニメーションと物理の同期状態をチェック - 改善版
    /// </summary>
    public void CheckJumpSynchronization()
    {
        bool managerJumping = IsJumping();
        bool managerSequence = IsInJumpSequence();
        bool animationJumping = PlayerAnimationController.IsPlayingJumpAnimation();
        bool animationSequence = PlayerAnimationController.IsInJumpSequence();

        Debug.Log($"=== ジャンプ同期状態チェック ===");
        Debug.Log($"Manager ジャンプ中: {managerJumping}");
        Debug.Log($"Manager シーケンス中: {managerSequence}");
        Debug.Log($"Animation ジャンプ中: {animationJumping}");
        Debug.Log($"Animation シーケンス中: {animationSequence}");
        Debug.Log($"現在の状態: {currentState}");
        Debug.Log($"アニメーションロック中: {PlayerAnimationController.IsAnimationLocked()}");

        // 同期が取れていない場合は警告
        if (managerJumping != animationJumping)
        {
            Debug.LogWarning($"ジャンプ状態の同期が取れていません！Manager: {managerJumping}, Animation: {animationJumping}");
        }
        else if (managerSequence != animationSequence)
        {
            Debug.LogWarning($"ジャンプシーケンスの同期が取れていません！Manager: {managerSequence}, Animation: {animationSequence}");
        }
        else
        {
            Debug.Log("ジャンプ状態は正常に同期しています");
        }

        Debug.Log($"========================");
    }

    /// <summary>
    /// プレイヤーの全体的な状態を強制的に同期 - 改善版
    /// </summary>
    public void ForceSynchronizeState()
    {
        Debug.Log("プレイヤー状態の強制同期を実行");

        // ジャンプシーケンスを強制終了
        isInJumpSequence = false;
        jumpSequenceStartTime = 0f;

        // アニメーションコントローラーの状態をリセット
        PlayerAnimationController.ResetJumpState();

        // 現在の状態に基づいて再設定
        PlayerState targetState = currentState;

        // ジャンプ状態の場合は地上状態にリセット
        if (IsJumpState(currentState))
        {
            if (IsGuarding)
            {
                targetState = PlayerState.Guard;
            }
            else
            {
                targetState = PlayerState.Idle;
            }
        }

        // 状態を強制的に再設定
        currentState = PlayerState.Dead; // 一時的に異なる状態にして確実に更新
        UpdatePlayerState(targetState);

        Debug.Log($"強制同期完了: {targetState}");
    }

    /// <summary>
    /// デバッグ用：ジャンプ状態を手動で設定 - 改善版
    /// </summary>
    /// <param name="jumpState"></param>
    public void DebugSetJumpState(PlayerState jumpState)
    {
        if (!IsJumpState(jumpState))
        {
            Debug.LogError($"指定された状態はジャンプ状態ではありません: {jumpState}");
            return;
        }

        Debug.Log($"デバッグ：ジャンプ状態を手動設定 → {jumpState}");

        // ジャンプシーケンスを開始
        if (!isInJumpSequence)
        {
            isInJumpSequence = true;
            jumpSequenceStartTime = Time.time;
        }

        if (!PlayerAnimationController.IsInJumpSequence())
        {
            PlayerAnimationController.StartJumpSequence();
        }

        // 状態を設定
        UpdatePlayerState(jumpState);
    }

    /// <summary>
    /// パフォーマンス監視：状態変更頻度のチェック - 改善版
    /// </summary>
    private float[] recentStateChangeTimes = new float[10]; // 最近の10回の状態変更時間を記録
    private int stateChangeIndex = 0;

    public void MonitorStateChangeFrequency()
    {
        // 現在の時間を記録
        recentStateChangeTimes[stateChangeIndex] = Time.time;
        stateChangeIndex = (stateChangeIndex + 1) % recentStateChangeTimes.Length;

        // 1秒間の状態変更回数をカウント
        int changesInLastSecond = 0;
        float oneSecondAgo = Time.time - 1f;

        for (int i = 0; i < recentStateChangeTimes.Length; i++)
        {
            if (recentStateChangeTimes[i] > oneSecondAgo)
            {
                changesInLastSecond++;
            }
        }

        // 状態変更が多すぎる場合は警告
        if (changesInLastSecond > 8) // しきい値を少し緩和
        {
            Debug.LogWarning($"状態変更が頻繁すぎます: 1秒間に{changesInLastSecond}回");

            // 詳細情報も出力
            LogPlayerStateInfo();
        }
    }

    /// <summary>
    /// 着地判定の統合チェック - 新規メソッド
    /// </summary>
    /// <returns></returns>
    public bool ShouldCompleteLanding()
    {
        if (!IsLanding()) return false;

        // 複数の条件を統合的にチェック
        bool animationReady = CanCompleteLanding(); // 7割再生完了
        bool timeElapsed = GetJumpSequenceElapsedTime() > 1.0f; // 長時間経過

        // PlayerControllerの地面接触状態も確認できれば理想的
        // （現在はprivateなので直接アクセスできない）

        return animationReady || timeElapsed;
    }

    /// <summary>
    /// ジャンプシーケンスのタイムアウトチェック - 新規メソッド
    /// </summary>
    void LateUpdate()
    {
        // ジャンプシーケンスのタイムアウトチェック
        if (isInJumpSequence && GetJumpSequenceElapsedTime() > 3.0f)
        {
            Debug.LogWarning("ジャンプシーケンスがタイムアウトしました。強制終了します。");
            ForceCompleteLanding();
        }

        // 状態変更頻度の監視
        if (Time.time - lastStateChangeTime < 0.1f) // 0.1秒以内の変更をカウント
        {
            MonitorStateChangeFrequency();
        }
    }
}

/// <summary>
/// プレイヤーの状態定義（改善版）
/// </summary>
public enum PlayerState
{
    Idle,       // アイドル
    Walk,       // 歩行
    Run,        // 走行
    Guard,      // ガード
    Dead,       // 死亡
    JumpStart,  // ジャンプ開始（地面を蹴る瞬間）
    JumpLoop,   // ジャンプ中（空中にいる間）
    JumpEnd     // 着地準備（着地アニメーション）
}