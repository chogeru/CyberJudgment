using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using AbubuResouse.Singleton;
using AbubuResouse.Log;

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
    private float m_JumpCooldown = 0.1f;

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
    [SerializeField, LabelText("ジャンプ開始アニメーション時間"), Range(0.1f, 0.5f)]
    private float m_JumpStartAnimationDuration = 0.25f; // 15フレーム (60FPSで0.25秒)

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("着地予測距離"), Range(0.1f, 1.0f)]
    private float m_LandingPredictionDistance = 0.3f;

    [FoldoutGroup("プレイヤー設定/ジャンプ設定")]
    [SerializeField, LabelText("着地アニメーション開始距離"), Range(0.1f, 2.0f)]
    private float m_LandingAnimationDistance = 0.5f;

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
    #endregion

    // ジャンプの段階を管理するenum
    private enum JumpPhase
    {
        None,
        Start,
        Rising,
        Falling,
        Landing
    }

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerManager.UpdatePlayerState(PlayerState.Idle);
        m_CharacterController = GetComponent<CharacterController>();

        // 初期化
        verticalVelocity = 0f;
        lastJumpTime = -m_JumpCooldown;
        lastGroundedTime = 0f;
        canCoyoteJump = false;
        jumpInputBuffer = 0f;
        jumpStartTime = 0f;
        isPreparingToLand = false;
        currentJumpPhase = JumpPhase.None;

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
                (input.Movement == Vector3.zero || input.Magnitude < 0.01f)
            )
            .Subscribe(_ =>
            {
                // 地面にいてジャンプ中でない場合のみIdleに遷移
                if (isGrounded && !isJumping)
                {
                    playerManager.UpdatePlayerState(PlayerState.Idle);
                    DebugUtility.Log("Player entered Idle state.");
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

    /// <summary>
    /// 現在の入力を取得する統一メソッド
    /// </summary>
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

    /// <summary>
    /// 走り状態かどうかを判定
    /// </summary>
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

    /// <summary>
    /// 歩き状態かどうかを判定
    /// </summary>
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
        this.UpdateAsObservable()
            .Where(_ => !StopManager.Instance.IsStopped)
            .Where(_ => Input.GetKeyDown(KeyCode.C))
            .Subscribe(_ => TryStartGuard())
            .AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyUp(KeyCode.C))
            .Subscribe(_ => StopGuard())
            .AddTo(this);

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

    private void TryStartGuard()
    {
        // ジャンプ中はガードできない
        if (isJumping) return;

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

            // ジャンプ中でない場合のみIdleに遷移
            if (isGrounded && !isJumping)
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

    void Update()
    {
        HandleGravityAndMovement();
        HandleJumpBuffer();
        HandleJumpStates();
    }

    /// <summary>
    /// ジャンプ入力を登録（バッファー機能付き）
    /// </summary>
    private void RegisterJumpInput()
    {
        jumpInputBuffer = Time.time + m_JumpBufferTime;
        TryJump();
    }

    /// <summary>
    /// ジャンプ入力バッファーの処理
    /// </summary>
    private void HandleJumpBuffer()
    {
        if (jumpInputBuffer > Time.time)
        {
            if (CanJump())
            {
                ExecuteJump();
                jumpInputBuffer = 0f; // バッファーを消費
            }
        }
    }

    /// <summary>
    /// ジャンプ状態の管理（着地検出改善版）
    /// </summary>
    private void HandleJumpStates()
    {
        if (!isJumping) return;

        float timeSinceJumpStart = Time.time - jumpStartTime;
        JumpPhase newPhase = currentJumpPhase;

        // ジャンプ開始から15フレーム（0.25秒）後にLoopに移行
        if (timeSinceJumpStart <= m_JumpStartAnimationDuration)
        {
            // ジャンプ開始段階
            newPhase = JumpPhase.Start;
        }
        else if (verticalVelocity > 0.1f)
        {
            // 上昇段階（まだ上昇している間）
            newPhase = JumpPhase.Rising;
        }
        else
        {
            // 落下段階または着地準備段階
            // 実際に地面に接触した場合は即座に着地処理
            if (isGrounded)
            {
                Debug.Log("Ground contact detected during jump state handling!");
                OnLanded();
                return; // 着地処理を実行したので、この関数を終了
            }
            // 地面に近づいたら着地アニメーションを開始
            else if (PredictImmediateLanding() && verticalVelocity <= 0)
            {
                newPhase = JumpPhase.Landing;
                isPreparingToLand = true;
            }
            else
            {
                newPhase = JumpPhase.Falling;
            }
        }

        // フェーズが変わった時のみ状態更新
        if (newPhase != currentJumpPhase)
        {
            currentJumpPhase = newPhase;
            UpdateJumpAnimationState(newPhase);
            Debug.Log($"Jump phase changed to: {newPhase}, VerticalVelocity: {verticalVelocity:F2}, IsGrounded: {isGrounded}");
        }
    }

    /// <summary>
    /// ジャンプフェーズに応じてアニメーション状態を更新
    /// </summary>
    private void UpdateJumpAnimationState(JumpPhase phase)
    {
        switch (phase)
        {
            case JumpPhase.Start:
                playerManager.UpdatePlayerState(PlayerState.JumpStart);
                break;
            case JumpPhase.Rising:
            case JumpPhase.Falling:
                playerManager.UpdatePlayerState(PlayerState.JumpLoop);
                break;
            case JumpPhase.Landing:
                playerManager.UpdatePlayerState(PlayerState.JumpEnd);
                break;
        }
    }

    /// <summary>
    /// 着地を予測する（即座の着地判定用）
    /// </summary>
    private bool PredictImmediateLanding()
    {
        float characterRadius = m_CharacterController.radius;
        float characterHeight = m_CharacterController.height;
        Vector3 bottomCenter = transform.position - Vector3.up * (characterHeight * 0.5f - characterRadius);

        // 着地アニメーション用の近距離判定（より短い距離で確実に検出）
        bool willLand = Physics.Raycast(bottomCenter, Vector3.down, m_LandingAnimationDistance, m_LayerMask);

        // デバッグ用
        Debug.DrawLine(bottomCenter, bottomCenter + Vector3.down * m_LandingAnimationDistance,
                      willLand ? Color.yellow : Color.blue, 0.1f);

        return willLand;
    }

    /// <summary>
    /// 通常の着地予測（着地処理用）
    /// </summary>
    private bool PredictLanding()
    {
        float characterRadius = m_CharacterController.radius;
        float characterHeight = m_CharacterController.height;
        Vector3 bottomCenter = transform.position - Vector3.up * (characterHeight * 0.5f - characterRadius);

        return Physics.Raycast(bottomCenter, Vector3.down, m_LandingPredictionDistance, m_LayerMask);
    }

    private void HandleGravityAndMovement()
    {
        bool wasGrounded = isGrounded;
        isGrounded = IsGrounded();

        // 地面から離れた時間を記録（コヨーテタイム用）
        if (wasGrounded && !isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        // コヨーテジャンプの判定
        canCoyoteJump = !isGrounded && (Time.time - lastGroundedTime) <= m_CoyoteTime && !isJumping;

        // 重力の適用
        ApplyGravity();

        // 垂直移動を常に適用
        Vector3 verticalMovement = Vector3.up * verticalVelocity * Time.deltaTime;
        m_CharacterController.Move(verticalMovement);

        // 着地検出はHandleJumpStatesで処理（重複を避ける）
    }

    /// <summary>
    /// 着地時の処理（着地アニメーション経由版）
    /// </summary>
    private void OnLanded()
    {
        if (isJumping)
        {
            Debug.Log("OnLanded called - starting landing sequence");

            // まずJumpEndアニメーションを再生
            currentJumpPhase = JumpPhase.Landing;
            playerManager.UpdatePlayerState(PlayerState.JumpEnd);

            // 短時間後に適切な状態に遷移
            StartCoroutine(TransitionAfterLandingAnimation());
        }
    }

    /// <summary>
    /// 着地アニメーション後の状態遷移（改善版）
    /// </summary>
    private System.Collections.IEnumerator TransitionAfterLandingAnimation()
    {
        // 着地アニメーションを少し再生させる（約0.1秒に短縮）
        yield return new WaitForSeconds(0.1f);

        // アニメーションコントローラーのジャンプシーケンスを明示的に終了
        var animController = GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            animController.EndJumpSequence();
        }

        // ジャンプ状態を完全に終了
        isJumping = false;
        isPreparingToLand = false;
        currentJumpPhase = JumpPhase.None;

        Debug.Log("Landing animation finished, transitioning to appropriate state");

        // 現在の入力を即座に取得
        var currentInput = GetCurrentInput();

        Debug.Log($"Landing transition - Input: {currentInput.Movement}, Magnitude: {currentInput.Magnitude}");

        // ガード状態を最優先でチェック
        if (playerManager.IsGuarding)
        {
            playerManager.UpdatePlayerState(PlayerState.Guard);
            Debug.Log("Transitioned to Guard state after landing");
        }
        // 移動入力がある場合
        else if (currentInput.Movement != Vector3.zero && currentInput.Magnitude > 0.01f)
        {
            // 走り判定
            if (ShouldRun(currentInput.Magnitude))
            {
                playerManager.UpdatePlayerState(PlayerState.Run);
                Debug.Log("Transitioned to Run state after landing");
            }
            else if (ShouldWalk(currentInput.Magnitude))
            {
                playerManager.UpdatePlayerState(PlayerState.Walk);
                Debug.Log("Transitioned to Walk state after landing");
            }
            else
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
                Debug.Log("Transitioned to Idle state after landing (input too small)");
            }
        }
        else
        {
            // 静止状態
            playerManager.UpdatePlayerState(PlayerState.Idle);
            Debug.Log("Transitioned to Idle state after landing (no input)");
        }
    }

    /// <summary>
    /// 重力の適用（改善版）
    /// </summary>
    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity <= 0)
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
    /// ジャンプが可能かどうかを判定
    /// </summary>
    private bool CanJump()
    {
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

        // 既にジャンプ中は不可
        if (isJumping)
        {
            return false;
        }

        // 地面にいるか、コヨーテタイム内であればジャンプ可能
        return isGrounded || canCoyoteJump;
    }

    /// <summary>
    /// ジャンプを試行する処理
    /// </summary>
    private void TryJump()
    {
        if (CanJump())
        {
            ExecuteJump();
        }
    }

    /// <summary>
    /// ジャンプの実行（改善版）
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
        currentJumpPhase = JumpPhase.Start;

        // ジャンプ開始状態に遷移
        playerManager.UpdatePlayerState(PlayerState.JumpStart);

        DebugUtility.Log($"Jump executed! Velocity: {verticalVelocity}, Force: {m_JumpForce}");
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

        // 着地アニメーション中は移動を制限
        if (isJumping && currentJumpPhase == JumpPhase.Landing)
        {
            return;
        }

        if (StopManager.Instance.IsStopped)
        {
            if (isGrounded && !isJumping) // ジャンプ中でない場合のみIdle状態に遷移
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
            }
            return;
        }

        // 入力値がゼロなら状態に応じて処理
        if (movement == Vector3.zero || movement.magnitude < 0.01f)
        {
            if (isGrounded && !isJumping) // ジャンプ中でない場合のみIdle状態に遷移
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
        if (isGrounded && !isJumping)
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

        // 着地アニメーション中は移動を制限
        if (isJumping && currentJumpPhase == JumpPhase.Landing)
        {
            return;
        }

        if (StopManager.Instance.IsStopped)
        {
            if (isGrounded && !isJumping)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
            }
            return;
        }

        // 入力値チェックを強化
        if (movement == Vector3.zero || movement.magnitude < 0.01f)
        {
            if (isGrounded && !isJumping) // ジャンプ中でない場合のみIdle状態に遷移
            {
                Debug.Log("HandleMovement: Setting Idle state due to no input");
                playerManager.UpdatePlayerState(PlayerState.Idle);
                GetComponentInChildren<PlayerCameraController>().OnActionEnd();
            }
            return; // 移動処理をスキップ
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
        // ジャンプ着地アニメーション中は他の状態遷移を完全にブロック
        if (isJumping && currentJumpPhase == JumpPhase.Landing)
        {
            Debug.Log($"Blocking state transition to {state} - landing animation in progress");
            return;
        }

        // 通常のジャンプ中は移動系の状態遷移を制限
        if (isJumping && (state == PlayerState.Idle || state == PlayerState.Walk || state == PlayerState.Run))
        {
            Debug.Log($"Blocking ground state {state} during jump");
            return;
        }

        playerManager.UpdatePlayerState(state);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }

    /// <summary>
    /// 地面に触れているかどうかを確認するメソッド（改善版）
    /// </summary>
    bool IsGrounded()
    {
        // CharacterControllerの組み込み判定を基本とする
        bool controllerGrounded = m_CharacterController.isGrounded;

        // 追加の詳細判定
        float characterRadius = m_CharacterController.radius;
        float characterHeight = m_CharacterController.height;
        Vector3 bottomCenter = transform.position - Vector3.up * (characterHeight * 0.5f - characterRadius + 0.05f);

        float groundCheckDistance = 0.15f; // 距離を短く調整

        // 中央のレイ
        bool centerGrounded = Physics.Raycast(bottomCenter, Vector3.down, groundCheckDistance, m_LayerMask);

        // 4点での判定（より確実）
        Vector3[] offsets = {
            transform.forward * characterRadius * 0.5f + transform.right * characterRadius * 0.5f,
            transform.forward * characterRadius * 0.5f - transform.right * characterRadius * 0.5f,
            -transform.forward * characterRadius * 0.5f + transform.right * characterRadius * 0.5f,
            -transform.forward * characterRadius * 0.5f - transform.right * characterRadius * 0.5f
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

        // 最終的な判定（CharacterControllerの判定を優先し、追加判定で補強）
        bool finalGrounded = controllerGrounded || (detailedGrounded && verticalVelocity <= 1f);

#if UNITY_EDITOR
        // デバッグ用の線描画
        Color rayColor = finalGrounded ? Color.green : Color.red;
        Debug.DrawLine(bottomCenter, bottomCenter + Vector3.down * groundCheckDistance, rayColor);

        foreach (Vector3 offset in offsets)
        {
            Vector3 rayStart = bottomCenter + offset;
            Debug.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance, rayColor * 0.7f);
        }
#endif

        return finalGrounded;
    }

    // コマンドパターンを定義するインターフェース
    private interface ICommand
    {
        // コマンドが実行可能かどうかを判定するメソッド
        bool CanExecute();
        // コマンドを実行するメソッド
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

        public bool CanExecute() => m_Direction != Vector3.zero;

        public void Execute()
        {
            // 着地アニメーション中は移動コマンドを実行しない
            if (m_Player.isJumping && m_Player.currentJumpPhase == JumpPhase.Landing)
            {
                return;
            }

            // ジャンプ中は状態更新を完全にスキップ
            if (!m_Player.isJumping && m_Player.isGrounded)
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

        public bool CanExecute() => m_Direction != Vector3.zero;

        public void Execute()
        {
            // 着地アニメーション中は移動コマンドを実行しない
            if (m_Player.isJumping && m_Player.currentJumpPhase == JumpPhase.Landing)
            {
                return;
            }

            // ジャンプ中は状態更新を完全にスキップ
            if (!m_Player.isJumping && m_Player.isGrounded)
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