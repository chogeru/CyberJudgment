using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

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
    [EndTab]
    #endregion

    #region 各コンポーネント
    [Tab("各コンポーネント")]
    [SerializeField, Header("プレイヤーのRigidbody")]
    private Rigidbody m_Rigidbody;
    [SerializeField, Header("プレイヤーのカプセルコライダ")]
    private CapsuleCollider m_CapsuleCollider;
    [SerializeField, Header("アニメ-ター")]
    private Animator m_Animator;
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

    //プレイヤーの行動状態を表すReactiveProperty
    private ReactiveProperty<bool> isIdle = new ReactiveProperty<bool>(true);
    private ReactiveProperty<bool> isWalk = new ReactiveProperty<bool>(false);
    private ReactiveProperty<bool> isRun = new ReactiveProperty<bool>(false);

    // ローカルプレイヤーが開始した時に呼び出されるメソッド
    public void Start()
    {
        //各コンポーネントの所得
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        //衝突検出を連続的に設定
        m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        //動きを非同期に初期化
        InitializeMovement().Forget();
        //アニメーションのバインド
        BindAnimations();
        Cursor.visible = false;
    }

    // プレイヤーの動きを非同期的に初期化(現在メニュー画面と同期)
    private async UniTaskVoid InitializeMovement()
    {

        // プレイヤーの移動入力をリアクティブに監視
        var moveStream = this.UpdateAsObservable()
         .Select(_ => new Vector3(Gamepad.current?.leftStick.ReadValue().x ?? Input.GetAxis("Horizontal"),
                                  0,
                                  Gamepad.current?.leftStick.ReadValue().y ?? Input.GetAxis("Vertical")))
         .Share();

        // Shiftキーを押しながらの走行を購読
        moveStream
             .Where(_ => Input.GetKey(KeyCode.LeftShift) || (Gamepad.current?.leftTrigger.isPressed ?? false))
             .Subscribe(movement => new RunCommand(this, movement).Execute());

        // 通常の歩行を購読
        moveStream
            .Where(_ => !Input.GetKey(KeyCode.LeftShift) && !(Gamepad.current?.leftTrigger.isPressed ?? false))
            .Subscribe(movement => new WalkCommand(this, movement).Execute());

        // ジャンプの入力と地面に触れているかの確認
        this.UpdateAsObservable()
            .Where(_ => Input.GetButtonDown("Jump") || (Gamepad.current?.buttonSouth.isPressed ?? false)) // Bボタンもジャンプ
            .Where(_ => IsGrounded())
            .Subscribe(_ => m_Rigidbody.AddForce(new Vector3(0.0f, m_JumpForce, 0.0f)));

        await UniTask.Yield();

    }
    void Update()
    {
        //重力の適応
        UseGravity();
        // 段差登りを試みる
        TryStepUp();
    }
    private void UseGravity()
    {
        // 重力の追加
        m_Rigidbody.AddForce(Physics.gravity * m_Rigidbody.mass * m_GravityMultiplier);
    }
    void TryStepUp()
    {
        if (m_Rigidbody.velocity.magnitude < 0.1f)
            return;
        //プレイヤーの前方0.3の位置から、レイキャストを設定
        Vector3 forward = transform.forward * 0.3f;
        //レイキャスト開始点はカプセルの中心点より
        Vector3 rayStart = transform.position + forward + Vector3.up * m_CapsuleCollider.height * 0.5f;
        //カプセルコライダーの底まで
        Vector3 rayEnd = transform.position + forward + Vector3.down * 0.1f;

        RaycastHit hitInfo;
        if (Physics.Raycast(rayStart, Vector3.down, out hitInfo, m_CapsuleCollider.height * 0.6f, m_LayerMask))
        {
            // レイキャストが何かにヒットし、その高さが乗り越えられる高さならプレイヤーを移動
            float stepHeight = hitInfo.point.y - transform.position.y;
            if (stepHeight <= m_MaxStepHeight && stepHeight > 0)
            {
                // プレイヤーの位置を段差の上に更新
                Vector3 targetPosition = new Vector3(transform.position.x, hitInfo.point.y + stepHeight, transform.position.z);
                //プレイヤーの位置を現在の位置からターゲットの位置に
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10);
            }
        }
#if UNITY_EDITOR
        //デバッグ用のレイキャスト表示
        Debug.DrawLine(rayStart, rayEnd, Color.blue);
#endif
    }

    //アニメーションの状態をバインドするメソッド
    private void BindAnimations()
    {
        //ReactivePropertyを使ってアニメータの各状態を購読し、変化があるたびにAnimatorへ反映
        isIdle.Subscribe(idle => m_Animator.SetBool("Idle", idle)).AddTo(this);//待機
        isWalk.Subscribe(walk => m_Animator.SetBool("Walk", walk)).AddTo(this);//歩行
        isRun.Subscribe(run => m_Animator.SetBool("Run", run)).AddTo(this);//走り

        //入力軸に基づくアニメーション変数もリアクティブに更新
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
    // プレイヤーを指定の速度で移動
    public void Move(Vector3 movement, float speed)
    {

        // カメラの前方方向を取得し、それを地面の平面にフラットにする
        Vector3 forward = m_CameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        // カメラの前方方向に基づいて右方向を計算する
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        // カメラの水平回転に基づく相対的な移動方向を計算する
        Vector3 relativeMovement = movement.z * forward + movement.x * right;


        //移動している場合
        if (movement != Vector3.zero)
        {
            //ターゲットの回転を計算
            Quaternion targetRotation = Quaternion.LookRotation(relativeMovement, Vector3.up);
            //プレイヤーの現在の回転とターゲットの回転を補完
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10);
            // 移動時の状態
            UpdateState(false, !isRun.Value, isRun.Value);
        }
        else
        {
            //Idle状態に更新
            UpdateState(true, false, false);
        }

        //指定の方向に移動
        m_Rigidbody.velocity = relativeMovement * speed;
    }

    // プレイヤーの状態を更新するメソッド
    private void UpdateState(bool idle, bool walk, bool run)
    {
        isIdle.Value = idle;
        isWalk.Value = walk;
        isRun.Value = run;
    }

    // 地面に触れているかどうかを確認するメソッド
    bool IsGrounded()
    {
        //レイキャストの開始地点をプレイヤーの少し上に設定
        Vector3 start = transform.position + Vector3.up * 0.1f;
        //終了地点を開始地点から下に0.5fの位置に
        Vector3 end = start - Vector3.up * 0.5f;
        //地面との衝突を確認
        bool isGrounded = Physics.Raycast(start, -Vector3.up, out RaycastHit hit, 0.5f);
#if UNITY_EDITOR
        Debug.DrawLine(start, end, isGrounded ? Color.green : Color.red);
#endif
        //地面が水平であることを確認
        return isGrounded && hit.normal.y > 0.5;
    }

    // コマンドパターンを定義するインターフェース
    private interface ICommand
    {
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

        public void Execute()
        {
            m_Player.UpdateState(false, true, false);
            m_Player.Move(m_Direction, m_Player.m_WalkSpeed);
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

        public void Execute()
        {
            m_Player.UpdateState(false, false, true);
            m_Player.Move(m_Direction, m_Player.m_RunSpeed);
        }
    }

}
