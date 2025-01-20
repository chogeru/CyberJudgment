using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using AbubuResouse.Singleton;
using AbubuResouse.Log;
#region
/*
.Subscribe
Subscribeは、イベントが発生したときに何か特定のアクション（例えば、プレイヤーを動かす）を起こすために使う
このメソッドを使って、イベントストリームを監視し、イベントが発生するたびにアクションが実行される

.Where
Whereは、特定の条件に基づいてイベントをフィルタリングするために使う
ジャンプボタンが押されたときや,プレイヤーが地面に触れているときなど、
条件に合う場合のみアクションを起こすために使用

.Select
Selectは、元のデータを新しい形に変換するために使う
この処理だと、ユーザーの入力を新しいVector3オブジェクトに変換して、それを使用してプレイヤーを動かす

 .Share
データの重複を防ぐためのもの
観測可能なストリームを複数の場所で共有するときに、同じデータが何度も生成されるのを防ぐ

観測可能なストリーム（Observable）
時間の経過とともに値やイベントを生成するデータのストリームを表す
具体的には、マウスの移動、ボタンのクリック、キーボードの入力、外部サーバーからのデータの取得など、

*/
#endregion

public class PlayerController : MonoBehaviour
{
    #region プレイヤー設定
    [Tab("プレイヤー設定")]
    [SerializeField, Header("歩き速度")]
    private float m_WalkSpeed = 5.0f;
    [SerializeField, Header("走りスピード")]
    private float m_RunSpeed = 10.0f;
    [SerializeField, Header("ジャンプ力")]
    public float m_JumpForce = 300f;
    [SerializeField, Header("重力係数")]
    private float m_GravityMultiplier = 9.81f;
    [SerializeField, Header("最大落下速度")]
    private float m_MaxFallSpeed = 50.0f;
    [SerializeField, Header("走行のしきい値")]
    private float m_RunThreshold = 0.7f;
    [SerializeField, Header("ゲームパッドデッドゾーン")]
    private float m_GamepadDeadZone = 0.2f;

    [SerializeField, Header("スティック入力値 (ゲームパッド)")]
    private Vector2 currentGamepadInput;

    [SerializeField, Header("スティック入力値 (キーボード)")]
    private Vector3 currentKeyboardInput;

    [SerializeField, Header("結合されたスティック入力値")]
    private Vector3 currentCombinedInput;

    [SerializeField, Header("スティック入力の大きさ")]
    private float currentInputMagnitude;

    [EndTab]
    #endregion

    [Tab("シールド設定")]
    [Header("シールド設定")]
    [SerializeField, Header("シールドobject")]
    private GameObject shieldObject;
    [SerializeField, Header("シールド展開エフェクトのプレハブ")]
    private GameObject shieldActivateEffectPrefab;
    [SerializeField, Header("シールド解除エフェクトのプレハブ")]
    private GameObject shieldDeactivateEffectPrefab;
    [SerializeField, Header("シールドアクティブ時に表示するオブジェクト")]
    private GameObject shieldActiveObject;
    [SerializeField, Header("シールド展開時消費マナ")]
    private float guardInitialCost = 10f;
    [SerializeField]
    private float guardSustainCostPerSecond = 3f;
    [SerializeField]
    private float manaRecoveryPerSecond = 3f;

    [Header("Guard Audio")]
    [SerializeField]
    private string guardStartSound;
    [SerializeField]
    private string guardStopSound;

    [EndTab]

    #region 各コンポーネント
    [Tab("各コンポーネント")]
    [SerializeField, Header("プレイヤーのRigidbody")]
    private Rigidbody m_Rigidbody;
    [SerializeField, Header("プレイヤーのカプセルコライダ")]
    private CapsuleCollider m_CapsuleCollider;
    private PlayerManager playerManager;


    [EndTab]
    #endregion

    #region 音声
    [Tab("音声")]
    [SerializeField, Header("歩き音クリップ")]
    private AudioClip walkClip;
    [SerializeField, Header("走り音クリップ")]
    private AudioClip runClip;

    [SerializeField, Header("歩き時のAudio Pitch(速度)"), Range(0.5f, 3f)]
    private float walkPitch = 1f;
    [SerializeField, Header("走り時のAudio Pitch(速度)"), Range(0.5f, 3f)]
    private float runPitch = 1.2f;

    [SerializeField, Header("フットステップ用AudioSource")]
    private AudioSource footstepAudioSource;
    [EndTab]
    #endregion

    #region カメラと衝突設定
    [Tab("カメラと衝突設定")]
    [SerializeField, Header("自身のカメラ")]
    private Transform m_CameraTransform;

    [SerializeField, Header("衝突検出用レイヤーマスク")]
    private LayerMask m_LayerMask;
    [SerializeField, Header("乗り越えられる段差の高さ")]
    private float m_MaxStepHeight = 0.3f;
    [EndTab]
    #endregion

    private bool isGrounded;
    private bool canMove = true;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerManager.UpdatePlayerState(PlayerState.Idle);
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        InitializeMovement().Forget();
        InitializeGuarding().Forget();
    }

    /// <summary>
    /// プレイヤーの動きを非同期的に初期化
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid InitializeMovement()
    {
        var moveStream = this.UpdateAsObservable()
            .Select(_ =>
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

                return new { Movement = combinedInput, Magnitude = magnitude };
            })
            .Share();

        // RunCommandのサブスクリプションにガード中でないことを追加
        moveStream
            .Where(input =>
                !playerManager.IsGuarding && // 追加: ガード中でないこと
                (
                    (Gamepad.current != null && (input.Magnitude >= m_RunThreshold || Gamepad.current.leftTrigger.isPressed)) ||
                    (Gamepad.current == null && Input.GetKey(KeyCode.LeftShift))
                )
            )
            .Where(input => input.Movement != Vector3.zero)
            .Subscribe(input =>
            {
                var runCommand = new RunCommand(this, input.Movement);
                if (runCommand.CanExecute())
                {
                    runCommand.Execute();
                }
            })
            .AddTo(this);

        // WalkCommandのサブスクリプションにガード中でないことを追加
        moveStream
            .Where(input =>
                !playerManager.IsGuarding && // 追加: ガード中でないこと
                (
                    (Gamepad.current != null && input.Magnitude >= 0.2f && input.Magnitude < m_RunThreshold && !Gamepad.current.leftTrigger.isPressed) ||
                    (Gamepad.current == null && !Input.GetKey(KeyCode.LeftShift))
                )
            )
            .Where(input => input.Movement != Vector3.zero)
            .Subscribe(input =>
            {
                var walkCommand = new WalkCommand(this, input.Movement);
                if (walkCommand.CanExecute())
                {
                    walkCommand.Execute();
                }
            })
            .AddTo(this);

        // Idleのサブスクリプションにガード中でないことを追加
        moveStream
            .Where(input =>
                !playerManager.IsGuarding && // 追加: ガード中でないこと
                (input.Movement == Vector3.zero || input.Magnitude < 0.01f)
            )
            .Subscribe(_ =>
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
                DebugUtility.Log("Player entered Idle state.");

                if (footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Stop();
                }
            })
            .AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => Input.GetButtonDown("Jump") || (Gamepad.current?.buttonSouth.isPressed ?? false))
            .Where(_ => IsGrounded())
            .Subscribe(_ => m_Rigidbody.AddForce(new Vector3(0.0f, m_JumpForce, 0.0f)))
            .AddTo(this);

        await UniTask.Yield();
    }

    /// <summary>
    /// ガード機能を非同期的に初期化します。
    /// </summary>
    private async UniTaskVoid InitializeGuarding()
    {
        // ガード開始の入力を監視（Cキー押下）
        this.UpdateAsObservable()
            .Where(_ => !StopManager.Instance.IsStopped)
            .Where(_ => Input.GetKeyDown(KeyCode.C))
            .Subscribe(_ => TryStartGuard())
            .AddTo(this);

        // ガード終了の入力を監視（Cキーリリース）
        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyUp(KeyCode.C))
            .Subscribe(_ => StopGuard())
            .AddTo(this);

        // MPの消費と回復を管理
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
                    // ガードを維持するのに十分なMPがないためガードを停止
                    StopGuard();
                }
            }
            else
            {
                // ガードしていない間はMPを回復
                playerManager.playerMP.RecoverMP(manaRecoveryPerSecond * Time.deltaTime);
            }

            await UniTask.Yield();
        }
    }



    /// <summary>
    /// ガードを開始します。
    /// </summary>
    private void TryStartGuard()
    {
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
        }
        else
        {
            // MP不足時のフィードバック
            Debug.Log("MPが不足しているため、ガードできません。");
        }
    }

    /// <summary>
    /// ガードを停止します。
    /// </summary>
    private void StopGuard()
    {
        if (playerManager.IsGuarding)
        {
            playerManager.SetGuarding(false);
            DeactivateShield();
            playerManager.UpdatePlayerState(PlayerState.Idle);
            PlayGuardSound(false);
            if (shieldDeactivateEffectPrefab != null && shieldObject != null)
            {
                EffectManager.Instance.PlayEffect(shieldDeactivateEffectPrefab, shieldObject.transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// シールドエフェクトを有効化します。
    /// </summary>
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

    /// <summary>
    /// シールドエフェクトを無効化します。
    /// </summary>
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

    /// <summary>
    /// ガード開始/停止時のサウンドを再生します。
    /// </summary>
    /// <param name="isGuarding">ガード中かどうか</param>
    private void PlayGuardSound(bool isGuarding)
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySound(isGuarding ? guardStartSound : guardStopSound, 1);
        }
    }

    void FixedUpdate()
    {
        UseGravity();
        TryStepUp();
    }

    /// <summary>
    /// 重力の適応
    /// </summary>
    private void UseGravity()
    {
        // 重力の追加
        m_Rigidbody.AddForce(Physics.gravity * m_Rigidbody.mass * m_GravityMultiplier);

        // 落下速度に最大値を設定
        if (m_Rigidbody.velocity.y < -m_MaxFallSpeed)
        {
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, -m_MaxFallSpeed, m_Rigidbody.velocity.z);
        }

        // 地面に触れているかどうかを更新
        isGrounded = IsGrounded();
    }

    /// <summary>
    /// プレイヤーの段差乗り越える処理
    /// </summary>
    void TryStepUp()
    {
        if (m_Rigidbody.velocity.magnitude < 0.1f)
            return;

        Vector3 forward = transform.forward * 0.35f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Vector3 rayEnd = rayStart + forward;

        if (Physics.Raycast(rayStart, forward, out RaycastHit hit, 0.35f, m_LayerMask))
        {
            float stepHeight = hit.point.y - transform.position.y;
            if (stepHeight <= m_MaxStepHeight && stepHeight > 0)
            {
                Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y + m_CapsuleCollider.height * 0.5f, transform.position.z);
                m_Rigidbody.position = Vector3.Lerp(m_Rigidbody.position, targetPosition, Time.fixedDeltaTime * 10);
            }
        }
#if UNITY_EDITOR
        Debug.DrawLine(rayStart, rayEnd, Color.blue);
#endif
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    /// <param name="movement">移動方向</param>
    /// <param name="speed">移動速度</param>
    public void Move(Vector3 movement, float speed)
    {
        if (!canMove || playerManager.IsHit || playerManager.IsDead || playerManager.IsGuarding)
        {
            return;
        }

        if (StopManager.Instance.IsStopped)
        {
            playerManager.UpdatePlayerState(PlayerState.Idle);
            return;
        }

        if (isGrounded)
        {
            // 入力値がゼロなら確実にIdle状態に遷移
            if (movement == Vector3.zero || movement.magnitude < 0.01f)
            {
                playerManager.UpdatePlayerState(PlayerState.Idle);
                GetComponentInChildren<PlayerCameraController>().OnActionEnd();
                return; // 移動処理をスキップ
            }

            // 入力値がゼロではない場合、移動と回転を処理
            HandleMovement(movement, speed);
            HandleRotation(movement);

            // アニメーションをWalkまたはRunに設定
            playerManager.UpdatePlayerState(speed == m_RunSpeed ? PlayerState.Run : PlayerState.Walk);
        }
    }


    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="movement">移動方向</param>
    /// <param name="speed">移動速度</param>
    private void HandleMovement(Vector3 movement, float speed)
    {
        if (!canMove || playerManager.IsHit || playerManager.IsDead)
        {
            return;
        }

        if (StopManager.Instance.IsStopped)
        {
            playerManager.UpdatePlayerState(PlayerState.Idle);
            return;
        }

        // 入力値がゼロまたは非常に小さい場合はIdle状態に遷移
        if (movement == Vector3.zero || movement.magnitude < 0.01f)
        {
            playerManager.UpdatePlayerState(PlayerState.Idle);
            GetComponentInChildren<PlayerCameraController>().OnActionEnd();
            return; // 移動処理をスキップ
        }

        // カメラ基準での移動方向を計算
        Vector3 forward = m_CameraTransform.forward;
        forward.y = 0; // 水平方向に限定
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 relativeMovement = movement.z * forward + movement.x * right;

        Vector3 velocity = relativeMovement * speed;
        ApplySlopeAdjustment(ref velocity, relativeMovement);

        m_Rigidbody.velocity = velocity;

    }


    /// <summary>
    /// プレイヤーの回転処理
    /// </summary>
    /// <param name="movement">回転方向</param>
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
    /// 斜面移動の処理
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="relativeMovement"></param>
    private void ApplySlopeAdjustment(ref Vector3 velocity, Vector3 relativeMovement)
    {
        Vector3 start = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(start, relativeMovement, out RaycastHit hit, 0.5f, m_LayerMask))
        {
            float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (slopeAngle <= m_MaxStepHeight)
            {
                velocity.y = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * m_WalkSpeed;
            }
        }
    }

    /// <summary>
    /// プレイヤーの状態を更新するメソッド
    /// </summary>
    /// <param name="state"></param>
    private void UpdateState(PlayerState state)
    {
        playerManager.UpdatePlayerState(state);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
    /// <summary>
    /// 地面に触れているかどうかを確認するメソッド
    /// </summary>
    /// <returns></returns>
    bool IsGrounded()
    {
        //レイキャストの開始地点をプレイヤーの少し上に設定
        Vector3 start = transform.position + Vector3.up * 0.1f;
        //終了地点を開始地点から下に0.5fの位置に
        Vector3 end = start - Vector3.up * 0.2f;
        //地面との衝突を確認
        bool isGrounded = Physics.Raycast(start, -Vector3.up, out RaycastHit hit, 0.5f);

        if (isGrounded)
        {
            DebugUtility.Log($"Grounded on: {hit.collider.gameObject.name}");
        }
        else
        {
            DebugUtility.Log("Not grounded");
        }

#if UNITY_EDITOR
        Debug.DrawLine(start, end, isGrounded ? Color.green : Color.red);
#endif
        return isGrounded;
    }

    // コマンドパターンを定義するインターフェース
    private interface ICommand
    {
        // コマンドが実行可能かどうかを判定するメソッド
        bool CanExecute();
        // コマンドが実行可能かどうかを判定するメソッド
        void Execute();
    }
    // 歩行を管理するコマンドクラス
    // このクラスはプレイヤーの歩行行動をカプセル化し、
    // 呼び出された際にプレイヤーを指定された方向と速度で移動させる

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
            m_Player.UpdateState(PlayerState.Walk);
            m_Player.GetComponentInChildren<PlayerCameraController>().OnActionEnd();
            m_Player.Move(m_Direction, m_Player.m_WalkSpeed);

            AudioSource audioSource = m_Player.footstepAudioSource;
            if (audioSource != null)
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
    // このクラスはプレイヤーの走行行動をカプセル化し、
    // 呼び出された際にプレイヤーを指定された方向に高速で移動させる
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
            m_Player.UpdateState(PlayerState.Run);
            m_Player.GetComponentInChildren<PlayerCameraController>().OnRunStart();
            m_Player.Move(m_Direction, m_Player.m_RunSpeed);
            AudioSource audioSource = m_Player.footstepAudioSource;
            if (audioSource != null)
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
