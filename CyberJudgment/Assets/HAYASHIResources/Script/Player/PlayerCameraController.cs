using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using AbubuResouse.Singleton;
using AbubuResouse.MVP.Presenter;


/// <summary>
/// プレイヤーカメラを制御するクラス
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    [SerializeField, Header("走行時のカメラオフセット倍率")]
    private float m_RunCameraOffsetMultiplier = 1.5f;

    [SerializeField, Header("敵が近い時のカメラオフセット倍率")]
    private float m_EnemyCameraOffsetMultiplier = 1.5f;

    #region プレイヤー関連の設定
    [SerializeField, Header("プレイヤーのTransform")]
    private Transform m_Player;

    [SerializeField, Header("プレイヤーの本体の位置")]
    private Transform m_PlayerTransfrom;
    #endregion

    #region カメラ感度設定
    [SerializeField, Header("マウス感度")]
    private float m_Sensitivity = 2.0f;

    [SerializeField, Header("ゲームパッド感度")]
    private float m_GamepadSensitivity = 3.0f;
    #endregion

    #region 障害物検出関連
    [SerializeField, Header("障害物レイヤー")]
    private LayerMask m_ObstacleMask;

    [SerializeField, Header("スフィアキャストの半径")]
    private float m_SphereCastRadius = 0.5f;
    #endregion

    #region 回転制限およびスムーズ設定
    [SerializeField, Header("垂直回転の制限角度")]
    private float m_VerticalRotationLimit = 80f;

    [SerializeField, Header("回転の加速時間(SmoothTime)")]
    private float m_RotationSmoothTime = 0.17f;

    // カメラ回転の現在値/速度管理用
    private float m_CurrentVerticalRotation = 0f;
    private float m_CurrentHorizontalRotation = 0f;
    private float m_CurrentRotationSpeedX = 0f;
    private float m_CurrentRotationSpeedY = 0f;
    private float m_RotationVelocityX = 0f;
    private float m_RotationVelocityY = 0f;
    #endregion

    #region UI時のカメラ設定
    [SerializeField, Header("UI開閉時のカメラ目標位置(ローカルオフセット)")]
    private Vector3 m_UITargetPosition;

    [SerializeField, Header("UI開閉時のカメラ目標回転(Euler角)")]
    private Vector3 m_UITargetRotation;

    [SerializeField, Header("UIカメラ移動時間(秒)")]
    private float m_UIMoveDuration = 1.0f;
    #endregion

    private Camera m_MainCamera;

    // プレイヤーとの相対的なオフセット
    private Vector3 m_Offset;

    // 現在のカメラ～プレイヤー距離（走行中などで拡縮）
    private float m_CurrentDistance;

    private UIPresenter _uiPresenter;

    // UI移動の前後で保存しておく位置・回転・距離
    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;
    private float m_OriginalCameraOffsetMagnitude;

    // UIメニューが開いているかどうか
    private bool m_IsUIOpen = false;
    // カメラがUI位置へ移動中かどうか
    private bool m_IsCameraMoving = false;

    [SerializeField, Header("敵のレイヤー")]
    private LayerMask m_EnemyLayer;

    [SerializeField, Header("敵の検出範囲")]
    private float m_EnemyDetectionRange = 10f;

    // OverlapSphereNonAllocで使い回すコリジョン配列
    private Collider[] m_EnemyColliders = new Collider[10];

    // Inspectorで確認用
    [SerializeField]
    private bool isHit;

    void Start()
    {
        InitializeCamera();
    }

    /// <summary>
    /// カメラの初期設定を行う。
    /// </summary>
    private void InitializeCamera()
    {
        m_MainCamera = GetComponent<Camera>();
        // プレイヤーとのオフセット
        m_Offset = transform.position - m_Player.position;
        m_CurrentDistance = m_Offset.magnitude;

        // UIPresenter取得
        _uiPresenter = UIPresenter.Instance;

        // 開始時のカメラ ワールド座標 を記録
        m_OriginalPosition = transform.position;
        // 開始時のカメラ回転 を記録
        m_OriginalRotation = transform.rotation;
        // 開始時のカメラ距離
        m_OriginalCameraOffsetMagnitude = m_Offset.magnitude;
    }

    /// <summary>
    /// 毎フレーム。敵の検出やUI状態の変化をチェック
    /// </summary>
    private void Update()
    {
        // 敵の検出 → カメラオフセットの変更
        bool enemyNearby = IsEnemyNearby();
        isHit = enemyNearby;
        if (enemyNearby)
        {
            m_CurrentDistance = m_OriginalCameraOffsetMagnitude * m_EnemyCameraOffsetMultiplier;
        }

        // UIの状態変更検知（Open/Close）
        // メニューが開いているかどうかが変わっていたら処理
        if (_uiPresenter != null && _uiPresenter.IsMenuOpen != m_IsUIOpen)
        {
            ToggleUICameraPosition(_uiPresenter.IsMenuOpen).Forget();
        }
    }

    /// <summary>
    /// プレイヤーの移動等が完了した後にカメラを追従させるためLateUpdateで処理
    /// </summary>
    private void LateUpdate()
    {
        // 停止中 or UI中にカメラを動かしたくない場合
        if (StopManager.Instance.IsStopped || m_IsUIOpen || m_IsCameraMoving)
            return;

        // 入力取得
        Vector2 cameraInput = GetCameraInput();
        float mouseX = cameraInput.x;
        float mouseY = cameraInput.y;

        // 回転更新
        UpdateRotation(mouseX, mouseY);

        // カメラ目標位置
        Vector3 targetPosition = CalculateCameraTargetPosition();

        // 障害物回避
        ApplyObstacleAvoidance(ref targetPosition);

        // 実際のカメラ適用
        m_MainCamera.transform.position = targetPosition;
        m_MainCamera.transform.rotation = CalculateCameraRotation();
    }

    /// <summary>
    /// 指定範囲に敵がいるかどうかチェック
    /// </summary>
    private bool IsEnemyNearby()
    {
        // まず配列をクリア
        Array.Clear(m_EnemyColliders, 0, m_EnemyColliders.Length);

        int count = Physics.OverlapSphereNonAlloc(
            m_Player.position,
            m_EnemyDetectionRange,
            m_EnemyColliders,
            m_EnemyLayer
        );

        // 視界内かどうかも簡易チェック
        for (int i = 0; i < count; i++)
        {
            var collider = m_EnemyColliders[i];
            if (collider == null) continue;

            Vector3 enemyDirection = (collider.transform.position - m_MainCamera.transform.position).normalized;
            float dot = Vector3.Dot(m_MainCamera.transform.forward, enemyDirection);

            // 前方 60度以内に入っているか(ざっくり)
            if (dot > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// カメラの回転を入力に基づいて更新する。
    /// </summary>
    /// <param name="mouseX">マウス・右スティックの水平入力</param>
    /// <param name="mouseY">マウス・右スティックの垂直入力</param>
    private void UpdateRotation(float mouseX, float mouseY)
    {
        // SmoothDampで回転速度をゆるやかに反映
        m_CurrentRotationSpeedX = Mathf.SmoothDamp(
            m_CurrentRotationSpeedX,
            mouseX,
            ref m_RotationVelocityX,
            m_RotationSmoothTime
        );

        m_CurrentRotationSpeedY = Mathf.SmoothDamp(
            m_CurrentRotationSpeedY,
            mouseY,
            ref m_RotationVelocityY,
            m_RotationSmoothTime
        );

        // 水平回転を加算
        m_CurrentHorizontalRotation += m_CurrentRotationSpeedX;
        // 垂直回転を加算
        m_CurrentVerticalRotation -= m_CurrentRotationSpeedY;

        // 垂直回転角を制限
        m_CurrentVerticalRotation = Mathf.Clamp(m_CurrentVerticalRotation, -m_VerticalRotationLimit, m_VerticalRotationLimit);
    }

    /// <summary>
    /// カメラの目標位置を計算
    /// </summary>
    private Vector3 CalculateCameraTargetPosition()
    {
        // 水平回転 (Yaw)
        Quaternion horizontalRotation = Quaternion.Euler(0f, m_CurrentHorizontalRotation, 0f);
        // 垂直回転 (Pitch)
        Quaternion verticalRotation = Quaternion.Euler(m_CurrentVerticalRotation, 0f, 0f);
        Quaternion totalRotation = horizontalRotation * verticalRotation;

        // 後方方向に m_CurrentDistance だけ離す
        Vector3 offset = totalRotation * Vector3.back * m_CurrentDistance;
        return m_Player.position + offset;
    }

    /// <summary>
    /// カメラの回転(Quaternion)を計算
    /// </summary>
    private Quaternion CalculateCameraRotation()
    {
        Quaternion horizontalRotation = Quaternion.Euler(0f, m_CurrentHorizontalRotation, 0f);
        Quaternion verticalRotation = Quaternion.Euler(m_CurrentVerticalRotation, 0f, 0f);

        return horizontalRotation * verticalRotation;
    }

    /// <summary>
    /// カメラ位置の障害物回避 (SphereCast)
    /// </summary>
    private void ApplyObstacleAvoidance(ref Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Player.position;
        float distance = direction.magnitude;
        if (Physics.SphereCast(
            m_Player.position,
            m_SphereCastRadius,
            direction.normalized,
            out RaycastHit hit,
            distance,
            m_ObstacleMask))
        {
            // 衝突点より少し手前にカメラを置く
            targetPosition = hit.point - direction.normalized * m_SphereCastRadius;
        }
    }

    /// <summary>
    /// UIの開閉に応じてカメラを切り替える
    /// </summary>
    private async UniTaskVoid ToggleUICameraPosition(bool isUIOpen)
    {
        m_IsUIOpen = isUIOpen;
        m_IsCameraMoving = true;

        Vector3 targetLocalOffset;
        Quaternion targetRotation;

        if (isUIOpen)
        {
            // UIを開くとき
            // プレイヤーのローカル座標系で指定されたオフセット＋回転を適用
            targetLocalOffset = m_UITargetPosition;
            targetRotation = Quaternion.Euler(m_UITargetRotation) * m_PlayerTransfrom.rotation;

            // 現在のカメラワールド座標を「プレイヤーから見たローカル座標」として保存
            m_OriginalPosition = m_PlayerTransfrom.InverseTransformPoint(transform.position);
            m_OriginalRotation = transform.rotation;
        }
        else
        {
            // UIを閉じるとき → 元に戻す
            targetLocalOffset = m_OriginalPosition;
            targetRotation = m_OriginalRotation;
        }

        await SmoothMoveCameraAsync(targetLocalOffset, targetRotation);

        m_IsCameraMoving = false;
    }

    /// <summary>
    /// カメラをスムーズに移動させる
    /// </summary>
    private async UniTask SmoothMoveCameraAsync(Vector3 targetLocalOffset, Quaternion targetRotation)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < m_UIMoveDuration)
        {
            float t = elapsedTime / m_UIMoveDuration;
            elapsedTime += Time.deltaTime;

            // プレイヤーのローカルオフセット → ワールド座標
            Vector3 targetPosition = m_PlayerTransfrom.TransformPoint(targetLocalOffset);

            // 位置補間
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            // 回転補間
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // 最終値
        transform.position = m_PlayerTransfrom.TransformPoint(targetLocalOffset);
        transform.rotation = targetRotation;
    }

    /// <summary>
    /// カメラの移動入力を返す(マウス + ゲームパッド右スティック)
    /// </summary>
    private Vector2 GetCameraInput()
    {
        // メニュー中または特定UI中 → カメラ操作しない
        if (_uiPresenter == null
            || _uiPresenter.IsMenuOpen
            || !string.IsNullOrEmpty(_uiPresenter.CurrentUIObject)
            || m_IsCameraMoving)
        {
            return Vector2.zero;
        }

        float mouseX = Input.GetAxis("Mouse X") * m_Sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * m_Sensitivity;

        Vector2 gamepadInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        mouseX += gamepadInput.x * m_GamepadSensitivity;
        mouseY += gamepadInput.y * m_GamepadSensitivity;

        return new Vector2(mouseX, mouseY);
    }

    /// <summary>
    /// 走り始めたときに呼ばれる想定
    /// </summary>
    public void OnRunStart()
    {
        SmoothMoveToRunOffset().Forget();
    }

    /// <summary>
    /// 走行をやめたときなどに呼ばれる想定
    /// </summary>
    public void OnActionEnd()
    {
        SmoothMoveToDefaultOffset().Forget();
    }

    /// <summary>
    /// 走行開始時にカメラのオフセットを徐々に広げる
    /// </summary>
    private async UniTask SmoothMoveToRunOffset()
    {
        float elapsedTime = 0f;
        float startDistance = m_CurrentDistance;
        float targetDistance = m_OriginalCameraOffsetMagnitude * m_RunCameraOffsetMultiplier;

        while (elapsedTime < m_UIMoveDuration)
        {
            float t = elapsedTime / m_UIMoveDuration;
            elapsedTime += Time.deltaTime;

            m_CurrentDistance = Mathf.Lerp(startDistance, targetDistance, t);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        m_CurrentDistance = targetDistance;
    }

    /// <summary>
    /// 走行終了時にカメラのオフセットをデフォルトに戻す
    /// </summary>
    private async UniTask SmoothMoveToDefaultOffset()
    {
        float elapsedTime = 0f;
        float startDistance = m_CurrentDistance;
        float targetDistance = m_OriginalCameraOffsetMagnitude;

        while (elapsedTime < m_UIMoveDuration)
        {
            float t = elapsedTime / m_UIMoveDuration;
            elapsedTime += Time.deltaTime;

            m_CurrentDistance = Mathf.Lerp(startDistance, targetDistance, t);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        m_CurrentDistance = targetDistance;
    }
}
