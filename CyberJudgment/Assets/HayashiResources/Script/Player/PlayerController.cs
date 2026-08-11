using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using AbubuResouse.Singleton;
using AbubuResouse.Log;
using AbubuResouse.Singleton;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region プレイヤー設定
    [FoldoutGroup("プレイヤー設定")]
    [SerializeField, LabelText("歩き速度")]
    private float m_WalkSpeed = 5.0f;

    [FoldoutGroup("プレイヤー設定")]
    [SerializeField, LabelText("走りスピード")]
    private float m_RunSpeed = 10.0f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("ジャンプ力"), Range(3f, 20f)]
    public float m_JumpForce = 12f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("ジャンプクールダウン"), Range(0.1f, 0.5f)]
    private float m_JumpCooldown = 0.2f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("着地後クールタイム"), Range(0.5f, 3.0f)]
    private float m_LandingCooldown = 1.5f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("コヨーテタイム"), Range(0.1f, 0.3f)]
    private float m_CoyoteTime = 0.15f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("空中制御力"), Range(0.1f, 1f)]
    private float m_AirControl = 0.7f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("ジャンプ時重力係数"), Range(0.5f, 2f)]
    private float m_JumpGravityMultiplier = 0.8f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("落下時重力係数"), Range(1f, 3f)]
    private float m_FallGravityMultiplier = 2.0f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("入力バッファー時間"), Range(0.1f, 0.5f)]
    private float m_JumpBufferTime = 0.1f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("ジャンプ開始時間"), Range(0.05f, 0.2f)]
    private float m_JumpStartDuration = 0.1f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("上昇判定速度閾値"), Range(1f, 5f)]
    private float m_RisingVelocityThreshold = 2f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("着地予測距離"), Range(0.1f, 1.0f)]
    private float m_LandingPredictionDistance = 0.4f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("着地アニメーション開始距離"), Range(0.5f, 2.0f)]
    private float m_LandingAnimationDistance = 1.2f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("着地確認時間"), Range(0.05f, 0.2f)]
    private float m_LandingConfirmTime = 0.05f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("地面接触必要フレーム数"), Range(1, 3)]
    private int m_GroundedRequiredFrames = 1;

    [FoldoutGroup("プレイヤー設定")]
    [SerializeField, LabelText("重力係数")]
    private float m_GravityMultiplier = 15f;

    [FoldoutGroup("プレイヤー設定")]
    [SerializeField, LabelText("最大落下速度")]
    private float m_MaxFallSpeed = 30.0f;

    [FoldoutGroup("プレイヤー設定")]
    [SerializeField, LabelText("走行のしきい値")]
    private float m_RunThreshold = 0.7f;

    [FoldoutGroup("プレイヤー設定")]
    [SerializeField, LabelText("ゲームパッドデッドゾーン")]
    private float m_GamepadDeadZone = 0.2f;

    [FoldoutGroup("プレイヤー設定/デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("スティック入力値 (ゲームパッド)")]
    private Vector2 currentGamepadInput;

    [FoldoutGroup("プレイヤー設定/デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("スティック入力値 (キーボード)")]
    private Vector3 currentKeyboardInput;

    [FoldoutGroup("プレイヤー設定/デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("結合されたスティック入力値")]
    private Vector3 currentCombinedInput;

    [FoldoutGroup("プレイヤー設定/デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("スティック入力の大きさ")]
    private float currentInputMagnitude;
    #endregion

    #region シールド設定
    [FoldoutGroup("シールド設定")]
    [SerializeField, LabelText("シールドobject")]
    private GameObject shieldObject;

    [FoldoutGroup("シールド設定/エフェクト")]
    [SerializeField, LabelText("シールド展開エフェクトのプレハブ")]
    private GameObject shieldActivateEffectPrefab;

    [FoldoutGroup("シールド設定/エフェクト")]
    [SerializeField, LabelText("シールド解除エフェクトのプレハブ")]
    private GameObject shieldDeactivateEffectPrefab;

    [FoldoutGroup("シールド設定")]
    [SerializeField, LabelText("シールド時非アクティブオブジェクト")]
    private GameObject shieldDeactiveObject;

    [FoldoutGroup("シールド設定")]
    [SerializeField, LabelText("シールドアクティブ時に表示するオブジェクト")]
    private GameObject shieldActiveObject;

    [FoldoutGroup("シールド設定/マナ設定")]
    [SerializeField, LabelText("シールド展開時消費マナ")]
    private float guardInitialCost = 10f;

    [FoldoutGroup("シールド設定/マナ設定")]
    [SerializeField, LabelText("シールド維持マナ消費/秒")]
    private float guardSustainCostPerSecond = 3f;

    [FoldoutGroup("シールド設定/マナ設定")]
    [SerializeField, LabelText("マナ回復量/秒")]
    private float manaRecoveryPerSecond = 3f;

    [FoldoutGroup("シールド設定/音声")]
    [SerializeField, LabelText("ガード開始音")]
    private string guardStartSound;

    [FoldoutGroup("シールド設定/音声")]
    [SerializeField, LabelText("ガード停止音")]
    private string guardStopSound;

    [FoldoutGroup("シールド設定/マウス設定")]
    [SerializeField, LabelText("マウスホイール感度"), Range(0.1f, 2.0f)]
    private float mouseWheelSensitivity = 1.0f;

    [FoldoutGroup("シールド設定/被弾リアクション")]
    [SerializeField, LabelText("シールド被弾時拡大率"), Range(1.1f, 2.0f)]
    private float shieldHitScaleMultiplier = 1.3f;

    [FoldoutGroup("シールド設定/被弾リアクション")]
    [SerializeField, LabelText("シールド拡大時間"), Range(0.05f, 0.3f)]
    private float shieldHitScaleDuration = 0.15f;

    [FoldoutGroup("シールド設定/被弾リアクション")]
    [SerializeField, LabelText("シールド振動強度"), Range(0.1f, 1.0f)]
    private float shieldHitShakeIntensity = 0.5f;

    [FoldoutGroup("シールド設定/被弾リアクション")]
    [SerializeField, LabelText("シールド振動時間"), Range(0.1f, 0.5f)]
    private float shieldHitShakeDuration = 0.2f;

    [FoldoutGroup("シールド設定/デバッグ"), ReadOnly]
    [SerializeField, LabelText("マウスホイール押下中")]
    private bool isMouseWheelPressed = false;

    [FoldoutGroup("シールド設定/デバッグ"), ReadOnly]
    [SerializeField, LabelText("シールド被弾中")]
    private bool isShieldHitActive = false;
    #endregion

    #region 各コンポーネント
    [BoxGroup("各コンポーネント")]
    [SerializeField, LabelText("プレイヤーのCharacterController")]
    private CharacterController m_CharacterController;

    [BoxGroup("各コンポーネント")]
    [SerializeField, LabelText("プレイヤーマネージャー"), ReadOnly]
    private PlayerManager playerManager;
    #endregion

    #region 音声
    [FoldoutGroup("音声設定")]
    [SerializeField, LabelText("歩き音クリップ")]
    private AudioClip walkClip;

    [FoldoutGroup("音声設定")]
    [SerializeField, LabelText("走り音クリップ")]
    private AudioClip runClip;

    [FoldoutGroup("音声設定")]
    [SerializeField, LabelText("歩き時のAudio Pitch(速度)"), Range(0.5f, 3f)]
    private float walkPitch = 1f;

    [FoldoutGroup("音声設定")]
    [SerializeField, LabelText("走り時のAudio Pitch(速度)"), Range(0.5f, 3f)]
    private float runPitch = 1.2f;

    [FoldoutGroup("音声設定")]
    [SerializeField, LabelText("フットステップ用AudioSource")]
    private AudioSource footstepAudioSource;
    #endregion

    #region カメラと衝突設定
    [FoldoutGroup("カメラと衝突設定")]
    [SerializeField, LabelText("自身のカメラ")]
    private Transform m_CameraTransform;

    [FoldoutGroup("カメラと衝突設定")]
    [SerializeField, LabelText("衝突検出用レイヤーマスク")]
    private LayerMask m_LayerMask;

    [FoldoutGroup("カメラと衝突設定")]
    [SerializeField, LabelText("乗り越えられる段差の高さ")]
    private float m_MaxStepHeight = 0.3f;
    #endregion

    #region 内部変数
    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("地面に接触中")]
    private bool isGrounded;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("移動可能")]
    private bool canMove = true;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("現在の垂直速度")]
    private float verticalVelocity;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("ジャンプ中")]
    private bool isJumping;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("最後にジャンプした時間")]
    private float lastJumpTime;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("最後に着地した時間")]
    private float lastLandingTime;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("着地後クールタイム中")]
    private bool isInLandingCooldown;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("最後に地面から離れた時間")]
    private float lastGroundedTime;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("コヨーテジャンプ可能")]
    private bool canCoyoteJump;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("ジャンプ入力バッファー時間")]
    private float jumpInputBuffer;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("ジャンプ開始時間")]
    private float jumpStartTime;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("着地予測中")]
    private bool isPreparingToLand;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("ジャンプ状態")]
    private JumpPhase currentJumpPhase = JumpPhase.None;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("着地確認時間")]
    private float landingConfirmStartTime;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("地面接触フレーム数")]
    private int groundedFrameCount;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("前フレームの垂直速度")]
    private float previousVerticalVelocity;

    [FoldoutGroup("デバッグ情報"), ReadOnly]
    [SerializeField, LabelText("JumpEnd開始時間")]
    private float jumpEndStartTime;
    #endregion

    // シールド被弾用の内部変数
    private Vector3 originalShieldScale;
    private Vector3 originalShieldPosition;
    private Coroutine shieldHitCoroutine;

    // ジャンプの段階を管理するenum
    private enum JumpPhase
    {
        None,           // ジャンプしていない
        Start,          // ジャンプ開始
        Rising,         // 上昇中
        Falling,        // 落下中
        Landing,        // 着地準備
        Confirming      // 着地確認中
    }

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerManager.UpdatePlayerState(PlayerState.Idle);
        m_CharacterController = GetComponent<CharacterController>();

        // シールドの初期スケールと位置を記録
        if (shieldObject != null)
        {
            originalShieldScale = shieldObject.transform.localScale;
            originalShieldPosition = shieldObject.transform.localPosition;
        }

        // 初期化
        verticalVelocity = 0f;
        lastJumpTime = -m_JumpCooldown;
        lastLandingTime = -m_LandingCooldown;
        isInLandingCooldown = false;
        lastGroundedTime = 0f;
        canCoyoteJump = false;
        jumpInputBuffer = 0f;
        jumpStartTime = 0f;
        isPreparingToLand = false;
        currentJumpPhase = JumpPhase.None;
        landingConfirmStartTime = 0f;
        groundedFrameCount = 0;
        previousVerticalVelocity = 0f;
        jumpEndStartTime = 0f;
        isShieldHitActive = false;

        InitializeMovement().Forget();
        InitializeGuarding().Forget();
    }

    private async UniTaskVoid InitializeMovement()
    {
        var moveStream = this.UpdateAsObservable()
            .Select(_ => GetCurrentInput())
            .Share();

        // RunCommandのサブスクリプション
        moveStream
            .Where(input =>
                !playerManager.IsGuarding &&
                !IsInJumpTransition() &&
                ShouldRun(input.Magnitude) &&
                input.Movement != Vector3.zero
            )
            .Subscribe(input =>
            {
                var runCommand = new RunCommand(this, input.Movement);
                if (runCommand.CanExecute())
                {
                    runCommand.Execute();
                }
            })
            .AddTo(this);

        // WalkCommandのサブスクリプション
        moveStream
            .Where(input =>
                !playerManager.IsGuarding &&
                !IsInJumpTransition() &&
                ShouldWalk(input.Magnitude) &&
                input.Movement != Vector3.zero
            )
            .Subscribe(input =>
            {
                var walkCommand = new WalkCommand(this, input.Movement);
                if (walkCommand.CanExecute())
                {
                    walkCommand.Execute();
                }
            })
            .AddTo(this);

        // Idleのサブスクリプション
        moveStream
            .Where(input =>
                !playerManager.IsGuarding &&
                !IsInJumpTransition() &&
                (input.Movement == Vector3.zero || input.Magnitude < 0.01f)
            )
            .Subscribe(_ =>
            {
                if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
                {
                    playerManager.UpdatePlayerState(PlayerState.Idle);
                }

                if (footstepAudioSource != null && footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Stop();
                }
            })
            .AddTo(this);

        // ジャンプ入力の監視
        this.UpdateAsObservable()
            .Where(_ => !StopManager.Instance.IsStopped)
            .Where(_ =>
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetButtonDown("Jump") ||
                (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false)
            )
            .Subscribe(_ => RegisterJumpInput())
            .AddTo(this);

        await UniTask.Yield();
    }

    private bool IsInJumpTransition()
    {
        return currentJumpPhase == JumpPhase.Landing ||
               currentJumpPhase == JumpPhase.Confirming;
    }

    private (Vector3 Movement, float Magnitude) GetCurrentInput()
    {
        Vector2 gamepadInput = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;

        if (gamepadInput.magnitude < m_GamepadDeadZone)
        {
            gamepadInput = Vector2.zero;
        }
        else
        {
            gamepadInput = gamepadInput.normalized * ((gamepadInput.magnitude - m_GamepadDeadZone) / (1 - m_GamepadDeadZone));
        }

        Vector3 keyboardInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (keyboardInput.magnitude < 0.1f)
        {
            keyboardInput = Vector3.zero;
        }

        Vector3 combinedInput = (gamepadInput != Vector2.zero) ? new Vector3(gamepadInput.x, 0, gamepadInput.y) : keyboardInput;

        if (combinedInput.magnitude < 0.01f)
        {
            combinedInput = Vector3.zero;
        }

        float magnitude = combinedInput.magnitude;
        currentGamepadInput = gamepadInput;
        currentKeyboardInput = keyboardInput;
        currentCombinedInput = combinedInput;
        currentInputMagnitude = magnitude;

        return (combinedInput, magnitude);
    }

    private bool ShouldRun(float magnitude)
    {
        if (Gamepad.current != null)
        {
            return magnitude >= m_RunThreshold || Gamepad.current.leftTrigger.isPressed;
        }
        else
        {
            return magnitude > 0.01f && Input.GetKey(KeyCode.LeftShift);
        }
    }

    private bool ShouldWalk(float magnitude)
    {
        if (Gamepad.current != null)
        {
            return magnitude >= 0.2f && magnitude < m_RunThreshold && !Gamepad.current.leftTrigger.isPressed;
        }
        else
        {
            return magnitude > 0.01f && !Input.GetKey(KeyCode.LeftShift);
        }
    }

    private async UniTaskVoid InitializeGuarding()
    {
        // マウスホイール押下の監視（ガード開始）
        this.UpdateAsObservable()
            .Where(_ => !StopManager.Instance.IsStopped)
            .Select(_ => IsMouseWheelPressed())
            .DistinctUntilChanged()
            .Subscribe(isPressed =>
            {
                isMouseWheelPressed = isPressed;

                if (isPressed)
                {
                    TryStartGuard();
                }
                else
                {
                    StopGuard();
                }
            })
            .AddTo(this);

        // ガード中のマナ消費処理
        while (true)
        {
            if (playerManager.IsGuarding)
            {
                if (playerManager.playerMP.ConsumeMP(guardSustainCostPerSecond * Time.deltaTime))
                {
                    // ガード継続中
                }
                else
                {
                    StopGuard();
                }
            }
            else
            {
                playerManager.playerMP.RecoverMP(manaRecoveryPerSecond * Time.deltaTime);
            }

            await UniTask.Yield();
        }
    }

    /// <summary>
    /// マウスホイールが押されているかどうかを判定
    /// </summary>
    /// <returns>マウスホイールが押されているかどうか</returns>
    private bool IsMouseWheelPressed()
    {
        // マウスの中ボタン（ホイールクリック）の判定
        bool mouseWheelPressed = Input.GetMouseButton(2);

        // ゲームパッドの場合の代替入力（右スティック押し込み）
        bool gamepadAlternative = false;
        if (Gamepad.current != null)
        {
            // 正しいInput Systemの書き方で右スティック押し込みを検出
            gamepadAlternative = Gamepad.current.rightStickButton.isPressed;
        }

        return mouseWheelPressed || gamepadAlternative;
    }

    private void TryStartGuard()
    {
        // ジャンプ中や遷移中はガード不可
        if (isJumping || IsInJumpTransition())
        {
            Debug.Log("ジャンプ中のためガードできません");
            return;
        }

        if (playerManager.playerMP.ConsumeMP(guardInitialCost))
        {
            playerManager.SetGuarding(true);
            ActivateShield();
            playerManager.UpdatePlayerState(PlayerState.Guard);
            PlayGuardSound(true);

            if (shieldActivateEffectPrefab != null && shieldObject != null)
            {
                EffectManager.Instance.PlayEffect(shieldActivateEffectPrefab, shieldObject.transform.position, Quaternion.identity);
            }

            if (shieldDeactiveObject != null)
                shieldDeactiveObject.SetActive(false);

            Debug.Log("ガード開始（マウスホイール長押し）");
        }
        else
        {
            Debug.Log("MPが不足しているため、ガードできません。");
        }
    }

    private void StopGuard()
    {
        if (playerManager.IsGuarding)
        {
            playerManager.SetGuarding(false);
            DeactivateShield();

            if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
            }

            PlayGuardSound(false);

            if (shieldDeactivateEffectPrefab != null && shieldObject != null)
            {
                EffectManager.Instance.PlayEffect(shieldDeactivateEffectPrefab, shieldObject.transform.position, Quaternion.identity);
            }

            if (shieldDeactiveObject != null)
                shieldDeactiveObject.SetActive(true);

            Debug.Log("ガード終了（マウスホイール解放）");
        }
    }

    private void ActivateShield()
    {
        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
        }
        if (shieldActiveObject != null)
        {
            shieldActiveObject.SetActive(true);
        }
    }

    private void DeactivateShield()
    {
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
        if (shieldActiveObject != null)
        {
            shieldActiveObject.SetActive(false);
        }
    }

    private void PlayGuardSound(bool isGuarding)
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySound(isGuarding ? guardStartSound : guardStopSound, 1);
        }
    }

    /// <summary>
    /// シールド被弾時の処理
    /// </summary>
    /// <param name="hitPosition">攻撃を受けた位置</param>
    public void TriggerShieldHit(Vector3 hitPosition)
    {
        if (!playerManager.IsGuarding || isShieldHitActive) return;

        Debug.Log("シールド被弾エフェクト開始");

        // 被弾中フラグを立てる
        isShieldHitActive = true;

        // 既存のコルーチンがあれば停止
        if (shieldHitCoroutine != null)
        {
            StopCoroutine(shieldHitCoroutine);
        }

        // シールド被弾エフェクトを開始
        shieldHitCoroutine = StartCoroutine(ShieldHitEffect(hitPosition));
    }

    /// <summary>
    /// シールド被弾エフェクトのコルーチン
    /// </summary>
    /// <param name="hitPosition">攻撃を受けた位置</param>
    private IEnumerator ShieldHitEffect(Vector3 hitPosition)
    {
        if (shieldObject == null)
        {
            isShieldHitActive = false;
            yield break;
        }

        Transform shieldTransform = shieldObject.transform;

        // 初期状態を保存
        Vector3 initialScale = shieldTransform.localScale;
        Vector3 initialPosition = shieldTransform.localPosition;

        // 拡大目標値を計算
        Vector3 targetScale = initialScale * shieldHitScaleMultiplier;

        // シールド拡大アニメーション
        float elapsedTime = 0f;
        float halfDuration = shieldHitScaleDuration * 0.5f;

        // 拡大フェーズ
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / halfDuration;

            // スケールのイージング（拡大）
            float easeProgress = Mathf.Sin(progress * Mathf.PI * 0.5f); // sin ease out
            Vector3 currentScale = Vector3.Lerp(initialScale, targetScale, easeProgress);
            shieldTransform.localScale = currentScale;

            // 振動エフェクト
            Vector3 shakeOffset = Random.insideUnitSphere * shieldHitShakeIntensity * (1f - progress);
            shakeOffset.y *= 0.5f; // Y軸の振動を抑制
            shieldTransform.localPosition = initialPosition + shakeOffset;

            yield return null;
        }

        // 縮小フェーズ
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / halfDuration;

            // スケールのイージング（縮小）
            float easeProgress = 1f - Mathf.Cos(progress * Mathf.PI * 0.5f); // cos ease in
            Vector3 currentScale = Vector3.Lerp(targetScale, initialScale, easeProgress);
            shieldTransform.localScale = currentScale;

            // 振動エフェクト（徐々に減衰）
            float shakeIntensity = shieldHitShakeIntensity * (1f - progress);
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            shakeOffset.y *= 0.3f;
            shieldTransform.localPosition = initialPosition + shakeOffset;

            yield return null;
        }

        // 最終的に元の状態に戻す
        shieldTransform.localScale = initialScale;
        shieldTransform.localPosition = initialPosition;

        // 被弾フラグをリセット
        isShieldHitActive = false;

        Debug.Log("シールド被弾エフェクト完了");
    }

    /// <summary>
    /// シールドの状態をリセット（緊急時用）
    /// </summary>
    public void ResetShieldState()
    {
        if (shieldObject != null)
        {
            shieldObject.transform.localScale = originalShieldScale;
            shieldObject.transform.localPosition = originalShieldPosition;
        }

        if (shieldHitCoroutine != null)
        {
            StopCoroutine(shieldHitCoroutine);
            shieldHitCoroutine = null;
        }

        isShieldHitActive = false;
        Debug.Log("シールド状態をリセット");
    }

    void Update()
    {
        UpdateLandingCooldown();
        previousVerticalVelocity = verticalVelocity;
        HandleGravityAndMovement();
        HandleJumpBuffer();
        HandleJumpStateMachine();

        // ★追加: デバッグチェック
#if UNITY_EDITOR
        CheckLandingDebug();
#endif
    }

    /// <summary>
    /// 着地後クールタイムの更新
    /// </summary>
    private void UpdateLandingCooldown()
    {
        if (isInLandingCooldown)
        {
            float timeSinceLastLanding = Time.time - lastLandingTime;
            if (timeSinceLastLanding >= m_LandingCooldown)
            {
                isInLandingCooldown = false;
                Debug.Log("着地後クールタイム終了 - ジャンプ可能");
            }
        }
    }

    /// <summary>
    /// 着地処理のデバッグチェック（Update内で呼び出し）
    /// </summary>
    private void CheckLandingDebug()
    {
        // JumpEndアニメーションが意図せず再生されていないかチェック
        if (!isJumping && currentJumpPhase == JumpPhase.None && isGrounded)
        {
            var animController = GetComponent<PlayerAnimationController>();
            if (animController != null)
            {
                // 定期的にJumpEndアニメーション状態をチェック
                if (Time.time % 2.0f < 0.02f) // 2秒ごとにチェック
                {
                    animController.CheckJumpEndAnimationState();
                }
            }
        }
    }

    /// <summary>
    /// ジャンプ入力を登録（バッファー機能付き） - 修正版
    /// </summary>
    private void RegisterJumpInput()
    {
        Debug.Log($"ジャンプ入力受信 - 現在の状態: {currentJumpPhase}, 地面接触: {isGrounded}");

        // 着地後クールタイム中の詳細表示
        if (isInLandingCooldown)
        {
            float remainingCooldown = m_LandingCooldown - (Time.time - lastLandingTime);
            Debug.Log($"着地後クールタイム中のため入力無効: 残り{remainingCooldown:F1}秒");
            return;
        }

        if (CanJump())
        {
            ExecuteJump();
        }
        else
        {
            jumpInputBuffer = Time.time + m_JumpBufferTime;
            Debug.Log("ジャンプをバッファーに登録");
        }
    }

    /// <summary>
    /// ジャンプ入力バッファーの処理 - 修正版
    /// </summary>
    private void HandleJumpBuffer()
    {
        if (jumpInputBuffer > Time.time && jumpInputBuffer > 0f)
        {
            // バッファー処理時もクールタイムをチェック
            if (!isInLandingCooldown && CanJump())
            {
                ExecuteJump();
                jumpInputBuffer = 0f;
            }
            else if (isInLandingCooldown)
            {
                // クールタイム中はバッファーをクリア
                jumpInputBuffer = 0f;
                Debug.Log("着地後クールタイム中のためバッファークリア");
            }
        }
    }

    /// <summary>
    /// ジャンプステートマシン - 修正版
    /// </summary>
    private void HandleJumpStateMachine()
    {
        // 地面接触フレーム数をカウント
        if (IsGroundedPhysically())
        {
            groundedFrameCount++;
        }
        else
        {
            groundedFrameCount = 0;
        }

        // 安定した地面接触を判定
        bool stableGrounded = groundedFrameCount >= m_GroundedRequiredFrames;
        bool previousGrounded = isGrounded;
        isGrounded = stableGrounded;

        // 地面状態変化の検出
        if (!previousGrounded && isGrounded)
        {
            Debug.Log("地面に接触しました");
        }
        else if (previousGrounded && !isGrounded)
        {
            Debug.Log("地面から離れました");
            lastGroundedTime = Time.time;
        }

        // ジャンプ状態に応じた処理
        switch (currentJumpPhase)
        {
            case JumpPhase.None:
                HandleNonePhase();
                break;
            case JumpPhase.Start:
                HandleStartPhase();
                break;
            case JumpPhase.Rising:
                HandleRisingPhase();
                break;
            case JumpPhase.Falling:
                HandleFallingPhase();
                break;
            case JumpPhase.Landing:
                HandleLandingPhase();
                break;
            case JumpPhase.Confirming:
                HandleConfirmingPhase();
                break;
        }

        // コヨーテジャンプの更新
        canCoyoteJump = !isGrounded && !isJumping &&
                       (Time.time - lastGroundedTime) <= m_CoyoteTime;
    }

    private void HandleNonePhase()
    {
        // 通常状態では特別な処理なし
    }

    private void HandleStartPhase()
    {
        float timeSinceStart = Time.time - jumpStartTime;

        // JumpStartの最低維持時間経過後、速度に基づいて遷移判定
        if (timeSinceStart >= m_JumpStartDuration)
        {
            // 十分に上昇していれば上昇フェーズへ
            if (verticalVelocity > m_RisingVelocityThreshold)
            {
                TransitionToPhase(JumpPhase.Rising);
            }
            // 上昇が弱い場合は直接落下フェーズへ
            else if (verticalVelocity <= 0.5f)
            {
                TransitionToPhase(JumpPhase.Falling);
            }
        }
    }

    private void HandleRisingPhase()
    {
        // 上昇速度が十分に低下したら落下フェーズへ
        if (verticalVelocity <= 0.5f)
        {
            TransitionToPhase(JumpPhase.Falling);
        }
    }

    private void HandleFallingPhase()
    {
        // 着地予測または直接地面接触で着地フェーズへ
        if (ShouldStartLandingAnimation() || (isGrounded && verticalVelocity <= 0))
        {
            TransitionToPhase(JumpPhase.Landing);
            jumpEndStartTime = Time.time; // JumpEnd開始時間を記録
        }
    }

    private void HandleLandingPhase()
    {
        // JumpEndアニメーションの7割再生または地面接触で確認フェーズへ
        var animController = GetComponent<PlayerAnimationController>();
        bool animProgress = false;

        if (animController != null)
        {
            // より安全なアプローチでアニメーション進行度をチェック
            try
            {
                animProgress = animController.GetJumpEndProgress() >= 0.7f;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"アニメーション進行度取得でエラー: {e.Message}");
                animProgress = false;
            }
        }

        // 7割再生完了、または確実な着地、または一定時間経過で確認フェーズへ
        bool timeElapsed = Time.time - jumpEndStartTime >= 0.3f; // 最大0.3秒で強制遷移
        bool groundedAndSlow = isGrounded && verticalVelocity <= 0.5f;

        if (animProgress || groundedAndSlow || timeElapsed)
        {
            Debug.Log($"JumpEnd → Confirming: アニメ進行{animProgress}, 着地{groundedAndSlow}, 時間経過{timeElapsed}");
            TransitionToPhase(JumpPhase.Confirming);
            landingConfirmStartTime = Time.time;
        }
    }

    private void HandleConfirmingPhase()
    {
        float confirmTime = Time.time - landingConfirmStartTime;

        // 短い確認時間または確実に着地
        if (confirmTime >= m_LandingConfirmTime || (isGrounded && verticalVelocity <= 0))
        {
            CompleteLanding();
        }
    }

    /// <summary>
    /// フェーズ遷移処理 - 即座にアニメーション更新
    /// </summary>
    private void TransitionToPhase(JumpPhase newPhase)
    {
        if (currentJumpPhase == newPhase) return;

        Debug.Log($"ジャンプフェーズ遷移: {currentJumpPhase} → {newPhase}");
        currentJumpPhase = newPhase;

        // フェーズに応じて即座にアニメーション更新
        UpdateAnimationForPhase(newPhase);
    }

    /// <summary>
    /// フェーズに応じたアニメーション更新 - 改善版
    /// </summary>
    private void UpdateAnimationForPhase(JumpPhase phase)
    {
        switch (phase)
        {
            case JumpPhase.None:
                // 通常状態は別途制御
                break;
            case JumpPhase.Start:
                playerManager.UpdatePlayerState(PlayerState.JumpStart);
                Debug.Log("JumpStartアニメーション開始（即座）");
                break;
            case JumpPhase.Rising:
            case JumpPhase.Falling:
                playerManager.UpdatePlayerState(PlayerState.JumpLoop);
                Debug.Log($"JumpLoopアニメーション開始（{phase}）");
                break;
            case JumpPhase.Landing:
                playerManager.UpdatePlayerState(PlayerState.JumpEnd);
                Debug.Log("JumpEndアニメーション開始（即座）");
                break;
            case JumpPhase.Confirming:
                // 着地確認中はJumpEndを維持
                break;
        }
    }

    /// <summary>
    /// 着地アニメーションを開始すべきかどうかを判定 - 改善版
    /// </summary>
    private bool ShouldStartLandingAnimation()
    {
        // 既に着地フェーズなら false
        if (currentJumpPhase == JumpPhase.Landing || currentJumpPhase == JumpPhase.Confirming)
            return false;

        // 下降中でない場合は false
        if (verticalVelocity > 0) return false;

        // 地面との距離をチェック
        return PredictImmediateLanding();
    }

    /// <summary>
    /// 着地を予測する（即座の着地判定用）- 改善版
    /// </summary>
    private bool PredictImmediateLanding()
    {
        float characterRadius = m_CharacterController.radius;
        float characterHeight = m_CharacterController.height;
        Vector3 bottomCenter = transform.position - Vector3.up * (characterHeight * 0.5f - characterRadius);

        // 着地アニメーション用の判定距離
        float checkDistance = m_LandingAnimationDistance;

        // レイキャストで地面との距離をチェック
        RaycastHit hit;
        bool willLand = Physics.Raycast(bottomCenter, Vector3.down, out hit, checkDistance, m_LayerMask);

        // 追加：落下速度を考慮した予測時間計算
        if (willLand && verticalVelocity < 0)
        {
            float timeToLand = hit.distance / Mathf.Abs(verticalVelocity);
            // 0.3秒以内に着地する場合は着地アニメーション開始
            willLand = timeToLand <= 0.3f;
        }

        // デバッグ用
        Debug.DrawLine(bottomCenter, bottomCenter + Vector3.down * checkDistance,
                      willLand ? Color.yellow : Color.blue, 0.1f);

        return willLand;
    }

    private void HandleGravityAndMovement()
    {
        // 重力の適用
        ApplyGravity();

        // 垂直移動を常に適用
        Vector3 verticalMovement = Vector3.up * verticalVelocity * Time.deltaTime;
        m_CharacterController.Move(verticalMovement);
    }

    /// <summary>
    /// 着地完了処理 - 修正版（着地時間記録追加）
    /// </summary>
    private void CompleteLanding()
    {
        Debug.Log("着地完了");

        // 着地時間を記録してクールタイムを開始
        lastLandingTime = Time.time;
        isInLandingCooldown = true;
        Debug.Log($"着地後クールタイム開始: {m_LandingCooldown}秒");

        // ジャンプ状態を完全にリセット
        isJumping = false;
        isPreparingToLand = false;
        currentJumpPhase = JumpPhase.None;
        verticalVelocity = -2f; // 地面に軽く押し付ける
        landingConfirmStartTime = 0f;
        jumpEndStartTime = 0f;

        // アニメーションコントローラーのジャンプシーケンスを終了（アニメーションロックを無視）
        var animController = GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            animController.EndJumpSequence();
            // 着地完了時は強制的にアニメーションロックを解除
            animController.ForceUnlockAnimation();
        }

        // 着地後の状態遷移を即座に実行（アニメーションロックを無視）
        TransitionToAppropriateStateImmediate();
    }

    /// <summary>
    /// 着地後の適切な状態に即座に遷移 - 修正版（強制実行）
    /// </summary>
    private void TransitionToAppropriateStateImmediate()
    {
        var currentInput = GetCurrentInput();
        Debug.Log($"着地後の状態遷移 - 入力: {currentInput.Movement}, 大きさ: {currentInput.Magnitude}");

        // PlayerAnimationControllerに対して強制的に状態更新を指示
        var animController = GetComponent<PlayerAnimationController>();

        // ガード状態を最優先でチェック（マウスホイール押下状態も確認）
        if (playerManager.IsGuarding || isMouseWheelPressed)
        {
            // 強制的にアニメーション更新
            if (animController != null)
            {
                animController.ForceUpdateAnimationImmediate(PlayerState.Guard);
            }
            playerManager.UpdatePlayerState(PlayerState.Guard);
            Debug.Log("着地後: ガード状態に遷移");
        }
        // 移動入力がある場合
        else if (currentInput.Movement != Vector3.zero && currentInput.Magnitude > 0.01f)
        {
            PlayerState targetState;

            // 走り判定
            if (ShouldRun(currentInput.Magnitude))
            {
                targetState = PlayerState.Run;
                Debug.Log("着地後: 走り状態に遷移");
            }
            else if (ShouldWalk(currentInput.Magnitude))
            {
                targetState = PlayerState.Walk;
                Debug.Log("着地後: 歩き状態に遷移");
            }
            else
            {
                targetState = PlayerState.Idle;
                Debug.Log("着地後: アイドル状態に遷移（入力が小さい）");
            }

            // 強制的にアニメーション更新
            if (animController != null)
            {
                animController.ForceUpdateAnimationImmediate(targetState);
            }
            playerManager.UpdatePlayerState(targetState);
        }
        else
        {
            // 強制的にアニメーション更新
            if (animController != null)
            {
                animController.ForceUpdateAnimationImmediate(PlayerState.Idle);
            }
            playerManager.UpdatePlayerState(PlayerState.Idle);
            Debug.Log("着地後: アイドル状態に遷移（入力なし）");
        }
    }

    /// <summary>
    /// 重力の適用
    /// </summary>
    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity <= 0 && currentJumpPhase == JumpPhase.None)
        {
            verticalVelocity = -2f; // 地面に軽く押し付ける
        }
        else
        {
            // ジャンプ中（上昇中）と落下中で重力を変える
            float currentGravityMultiplier;
            if (verticalVelocity > 0)
            {
                // 上昇中：軽い重力
                currentGravityMultiplier = m_GravityMultiplier * m_JumpGravityMultiplier;
            }
            else
            {
                // 落下中：強い重力
                currentGravityMultiplier = m_GravityMultiplier * m_FallGravityMultiplier;
            }

            verticalVelocity -= currentGravityMultiplier * Time.deltaTime;

            // 最大落下速度を制限
            if (verticalVelocity < -m_MaxFallSpeed)
            {
                verticalVelocity = -m_MaxFallSpeed;
            }
        }
    }

    /// <summary>
    /// ジャンプが可能かどうかを判定 - 修正版（着地後クールタイム追加）
    /// </summary>
    private bool CanJump()
    {
        // 着地後クールタイムをチェック
        float timeSinceLastLanding = Time.time - lastLandingTime;
        if (timeSinceLastLanding < m_LandingCooldown)
        {
            Debug.Log($"着地後クールタイム中: 残り{m_LandingCooldown - timeSinceLastLanding:F1}秒");
            return false;
        }

        // クールダウン中は不可
        if (Time.time - lastJumpTime < m_JumpCooldown)
        {
            return false;
        }

        // プレイヤーが動けない状態では不可
        if (!canMove || playerManager.IsHit || playerManager.IsDead || playerManager.IsGuarding)
        {
            return false;
        }

        // 既にジャンプ中またはジャンプ遷移中は不可
        if (isJumping || IsInJumpTransition())
        {
            return false;
        }

        // 地面にいるか、コヨーテタイム内であればジャンプ可能
        return isGrounded || canCoyoteJump;
    }

    /// <summary>
    /// ジャンプの実行 - 改善版
    /// </summary>
    private void ExecuteJump()
    {
        // ジャンプの初速度を計算
        verticalVelocity = Mathf.Sqrt(2.0f * m_JumpForce * m_GravityMultiplier);
        isJumping = true;
        jumpStartTime = Time.time;
        lastJumpTime = Time.time;
        canCoyoteJump = false; // コヨーテジャンプを使ったので無効化
        isPreparingToLand = false;
        groundedFrameCount = 0; // ジャンプ開始時にリセット
        jumpEndStartTime = 0f;

        // 即座にジャンプ開始フェーズに遷移
        TransitionToPhase(JumpPhase.Start);

        // アニメーションコントローラーにジャンプシーケンス開始を通知
        var animController = GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            animController.StartJumpSequence();
        }

        DebugUtility.Log($"ジャンプ実行！ 速度: {verticalVelocity}, 力: {m_JumpForce}");
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    public void Move(Vector3 movement, float speed)
    {
        if (!canMove || playerManager.IsHit || playerManager.IsDead || playerManager.IsGuarding)
        {
            return;
        }

        // ジャンプ遷移中は移動を制限
        if (IsInJumpTransition())
        {
            return;
        }

        if (StopManager.Instance.IsStopped)
        {
            if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
            }
            return;
        }

        // 入力値がゼロなら状態に応じて処理
        if (movement == Vector3.zero || movement.magnitude < 0.01f)
        {
            if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
                GetComponentInChildren<PlayerCameraController>().OnActionEnd();
            }
            return; // 移動処理をスキップ
        }

        // 移動と回転を処理
        HandleMovement(movement, speed);
        HandleRotation(movement);

        // アニメーション状態の更新（地面にいてジャンプ中でない時のみ）
        if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
        {
            playerManager.UpdatePlayerState(speed == m_RunSpeed ? PlayerState.Run : PlayerState.Walk);
        }
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void HandleMovement(Vector3 movement, float speed)
    {
        if (!canMove || playerManager.IsHit || playerManager.IsDead)
        {
            return;
        }

        // ジャンプ遷移中は移動を制限
        if (IsInJumpTransition())
        {
            return;
        }

        if (StopManager.Instance.IsStopped)
        {
            if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
            }
            return;
        }

        // 入力値チェック
        if (movement == Vector3.zero || movement.magnitude < 0.01f)
        {
            if (isGrounded && !isJumping && currentJumpPhase == JumpPhase.None)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
                GetComponentInChildren<PlayerCameraController>().OnActionEnd();
            }
            return;
        }

        // カメラ基準での移動方向を計算
        Vector3 forward = m_CameraTransform.forward;
        forward.y = 0; // 水平方向に限定
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 relativeMovement = movement.z * forward + movement.x * right;

        // 空中制御：ジャンプ中は移動速度を調整
        float currentSpeed = speed;
        if (!isGrounded)
        {
            currentSpeed = speed * m_AirControl;
        }

        // 水平移動ベクトルを作成
        Vector3 horizontalMovement = relativeMovement * currentSpeed * Time.deltaTime;

        // CharacterControllerで水平移動のみ適用（垂直移動は別で処理）
        m_CharacterController.Move(horizontalMovement);
    }

    /// <summary>
    /// プレイヤーの回転処理
    /// </summary>
    private void HandleRotation(Vector3 movement)
    {
        if (movement != Vector3.zero)
        {
            // カメラの方向を考慮した移動方向を計算
            Vector3 forward = m_CameraTransform.forward;
            forward.y = 0; // 水平方向に限定
            forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 desiredDirection = movement.z * forward + movement.x * right;

            // 目標の回転を計算
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);

            // プレイヤーを目標の回転に補間して回転
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 120);
        }
    }

    /// <summary>
    /// プレイヤーの状態を更新するメソッド
    /// </summary>
    private void UpdateState(PlayerState state)
    {
        // ジャンプ遷移中は状態遷移を制限
        if (IsInJumpTransition())
        {
            Debug.Log($"ジャンプ遷移中のため状態遷移 {state} をブロック");
            return;
        }

        // 通常のジャンプ中は移動系の状態遷移を制限
        if (isJumping && (state == PlayerState.Idle || state == PlayerState.Walk || state == PlayerState.Run))
        {
            Debug.Log($"ジャンプ中のため地面状態 {state} をブロック");
            return;
        }

        playerManager.UpdatePlayerState(state);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }

    /// <summary>
    /// 物理的な地面接触を確認するメソッド - 改善版
    /// </summary>
    bool IsGroundedPhysically()
    {
        // CharacterControllerの組み込み判定を基本とする
        bool controllerGrounded = m_CharacterController.isGrounded;

        // 追加の詳細判定
        float characterRadius = m_CharacterController.radius;
        float characterHeight = m_CharacterController.height;
        Vector3 bottomCenter = transform.position - Vector3.up * (characterHeight * 0.5f - characterRadius + 0.02f);

        float groundCheckDistance = 0.08f; // 短い距離で確実に検出

        // 中央のレイ
        bool centerGrounded = Physics.Raycast(bottomCenter, Vector3.down, groundCheckDistance, m_LayerMask);

        // 4点での判定（角度を変えてより確実に）
        Vector3[] offsets = {
            transform.forward * characterRadius * 0.4f,
            -transform.forward * characterRadius * 0.4f,
            transform.right * characterRadius * 0.4f,
            -transform.right * characterRadius * 0.4f
        };

        int groundedCount = 0;
        foreach (Vector3 offset in offsets)
        {
            Vector3 rayStart = bottomCenter + offset;
            if (Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, m_LayerMask))
            {
                groundedCount++;
            }
        }

        bool detailedGrounded = centerGrounded || groundedCount >= 2;

        // 最終的な判定 - 速度も考慮
        bool physicallyGrounded = controllerGrounded || (detailedGrounded && verticalVelocity <= 1f);

#if UNITY_EDITOR
        // デバッグ用の線描画
        Color rayColor = physicallyGrounded ? Color.green : Color.red;
        Debug.DrawLine(bottomCenter, bottomCenter + Vector3.down * groundCheckDistance, rayColor);

        foreach (Vector3 offset in offsets)
        {
            Vector3 rayStart = bottomCenter + offset;
            Debug.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance, rayColor * 0.7f);
        }
#endif

        return physicallyGrounded;
    }

    /// <summary>
    /// デバッグ用：クールタイム状態を取得
    /// </summary>
    public bool IsInLandingCooldown()
    {
        return isInLandingCooldown;
    }

    /// <summary>
    /// デバッグ用：残りクールタイム時間を取得
    /// </summary>
    public float GetRemainingLandingCooldown()
    {
        if (!isInLandingCooldown) return 0f;
        float remaining = m_LandingCooldown - (Time.time - lastLandingTime);
        return Mathf.Max(0f, remaining);
    }

    /// <summary>
    /// デバッグ用：クールタイムを強制リセット
    /// </summary>
    public void ResetLandingCooldown()
    {
        isInLandingCooldown = false;
        lastLandingTime = -m_LandingCooldown;
        Debug.Log("着地後クールタイムを強制リセット");
    }

    // コマンドパターンを定義するインターフェース
    private interface ICommand
    {
        bool CanExecute();
        void Execute();
    }

    // 歩行を管理するコマンドクラス
    private class WalkCommand : ICommand
    {
        private readonly PlayerController m_Player;
        private readonly Vector3 m_Direction;

        public WalkCommand(PlayerController player, Vector3 direction)
        {
            this.m_Player = player;
            this.m_Direction = direction;
        }

        public bool CanExecute() => m_Direction != Vector3.zero && !m_Player.IsInJumpTransition();

        public void Execute()
        {
            // ジャンプ遷移中は移動コマンドを実行しない
            if (m_Player.IsInJumpTransition())
            {
                return;
            }

            // ジャンプ中は状態更新をスキップ
            if (!m_Player.isJumping && m_Player.isGrounded && m_Player.currentJumpPhase == JumpPhase.None)
            {
                m_Player.UpdateState(PlayerState.Walk);
            }

            m_Player.GetComponentInChildren<PlayerCameraController>().OnActionEnd();
            m_Player.Move(m_Direction, m_Player.m_WalkSpeed);

            AudioSource audioSource = m_Player.footstepAudioSource;
            if (audioSource != null && m_Player.isGrounded)
            {
                if (audioSource.clip != m_Player.walkClip || !audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.clip = m_Player.walkClip;
                    audioSource.pitch = m_Player.walkPitch;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }

    // 走行を管理するコマンドクラス
    private class RunCommand : ICommand
    {
        private readonly PlayerController m_Player;
        private readonly Vector3 m_Direction;

        public RunCommand(PlayerController player, Vector3 direction)
        {
            this.m_Player = player;
            this.m_Direction = direction;
        }

        public bool CanExecute() => m_Direction != Vector3.zero && !m_Player.IsInJumpTransition();

        public void Execute()
        {
            // ジャンプ遷移中は移動コマンドを実行しない
            if (m_Player.IsInJumpTransition())
            {
                return;
            }

            // ジャンプ中は状態更新をスキップ
            if (!m_Player.isJumping && m_Player.isGrounded && m_Player.currentJumpPhase == JumpPhase.None)
            {
                m_Player.UpdateState(PlayerState.Run);
            }

            m_Player.GetComponentInChildren<PlayerCameraController>().OnRunStart();
            m_Player.Move(m_Direction, m_Player.m_RunSpeed);

            AudioSource audioSource = m_Player.footstepAudioSource;
            if (audioSource != null && m_Player.isGrounded)
            {
                if (audioSource.clip != m_Player.runClip || !audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.clip = m_Player.runClip;
                    audioSource.pitch = m_Player.runPitch;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }
}