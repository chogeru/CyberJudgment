using System.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using AbubuResouse.Singleton;
using VInspector;

/// <summary>
/// プレイヤーカメラを制御するクラス
/// </summary>
public class PlayerCameraController : MonoBehaviour
{

    #region プレイヤー関連の設定
    [Tab("プレイヤー関連設定")]
    [SerializeField, Header("プレイヤーのTransform")]
    private Transform m_Player;

    [SerializeField, Header("プレイヤーの本体の位置")]
    private Transform m_PlayerTransfrom;
    [EndTab]
    #endregion

    #region カメラ感度設定
    [Tab("感度設定")]
    [SerializeField, Header("マウス感度")]
    private float m_Sensitivity = 2.0f;

    [SerializeField, Header("ゲームパッド感度")]
    private float m_GamepadSensitivity = 3.0f;
    [EndTab]
    #endregion

    #region 障害物検出関連
    [Tab("障害物設定")]
    [SerializeField, Header("障害物レイヤー")]
    private LayerMask m_ObstacleMask;

    [SerializeField, Header("スフィアキャストの半径")]
    private float m_SphereCastRadius = 0.5f;
    [EndTab]
    #endregion

    #region 回転制限およびスムーズ設定
    [Tab("回転制御")]
    [SerializeField, Header("垂直回転の制限角度")]
    private float m_VerticalRotationLimit = 80f;

    [SerializeField, Header("回転の加速時間")]
    private float m_RotationSmoothTime = 0.17f;

    private float m_CurrentVerticalRotation = 0f;
    private float m_CurrentHorizontalRotation = 0f;
    private float m_CurrentRotationSpeedX = 0f;
    private float m_CurrentRotationSpeedY = 0f;
    private float m_RotationVelocityX = 0f;
    private float m_RotationVelocityY = 0f;
    [EndTab]
    #endregion

    [SerializeField, Header("歩行時のカメラオフセット調整")]
    private float m_WalkCameraOffset = 1.0f;

    [SerializeField, Header("走行時のカメラオフセット調整")]
    private float m_RunCameraOffset = 2.0f;


    #region UI時のカメラ設定
    [Tab("UI時設定")]
    [SerializeField, Header("UI開閉時のカメラ目標位置")]
    private Vector3 m_UITargetPosition;

    [SerializeField, Header("UI開閉時のカメラ目標回転")]
    private Vector3 m_UITargetRotation;

    [SerializeField, Header("UIカメラ移動時間")]
    private float m_UIMoveDuration = 1.0f;
    [EndTab]
    #endregion

    private Camera m_MainCamera;
    private Vector3 m_Offset;
    private float m_CurrentDistance;

    private UIPresenter _uiPresenter;
    private PlayerManager playerManager;

    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;
    private bool m_IsUIOpen = false;
    private bool m_IsCameraMoving = false;

    void Start()
    {
        InitializeCamera();
        ObserveCameraRotation();
        ObserveUIState();
    }

    /// <summary>
    /// カメラの初期設定を行う。カメラのオフセットやプレイヤー位置に基づく初期位置を設定。
    /// </summary>
    private void InitializeCamera()
    {
        m_MainCamera = GetComponent<Camera>();
        m_Offset = transform.position - m_Player.position;
        m_CurrentDistance = m_Offset.magnitude;
        _uiPresenter = UIPresenter.Instance;
        m_OriginalPosition = transform.position;
        m_OriginalRotation = transform.rotation;
    }


    /// <summary>
    /// カメラの回転操作を監視し、毎フレーム入力に応じたカメラの更新を行う。
    /// </summary>
    private void ObserveCameraRotation()
    {
        Observable.EveryUpdate()
            .Select(_ => GetCameraInput())
            .Subscribe(inputs => UpdateCameraPositionAsync(inputs.x, inputs.y).Forget())
            .AddTo(this);
    }

    /// <summary>
    /// UIの開閉状態を監視し、UIが開かれたまたは閉じられたときにカメラ位置を切り替える。
    /// </summary>
    private void ObserveUIState()
    {
        Observable.EveryUpdate()
            .Where(_ => _uiPresenter.IsMenuOpen != m_IsUIOpen)
            .Subscribe(_ => ToggleUICameraPosition(_uiPresenter.IsMenuOpen).Forget())
            .AddTo(this);
    }

    /// <summary>
    /// プレイヤーの入力に応じたカメラの移動量を計算して返す。
    /// </summary>
    /// <returns>カメラの水平・垂直の入力値</returns>
    private Vector2 GetCameraInput()
    {
        if (_uiPresenter == null || _uiPresenter.IsMenuOpen || !string.IsNullOrEmpty(_uiPresenter.CurrentUIObject) || m_IsCameraMoving)
        {
            return Vector2.zero;
        }

        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X") * m_Sensitivity, Input.GetAxis("Mouse Y") * m_Sensitivity);
        Vector2 gamepadInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        return mouseInput + new Vector2(gamepadInput.x * m_Sensitivity, gamepadInput.y * m_Sensitivity);
    }

    /// <summary>
    /// 入力に基づきカメラの位置と回転を更新する。非同期で実行され、UIが開いている場合は処理をスキップ。
    /// </summary>
    /// <param name="mouseX">マウスの水平入力</param>
    /// <param name="mouseY">マウスの垂直入力</param>
    private async UniTaskVoid UpdateCameraPositionAsync(float mouseX, float mouseY)
    {
        if (StopManager.Instance.IsStopped || m_IsUIOpen || m_IsCameraMoving)
            return;

        UpdateRotation(mouseX, mouseY);

        // プレイヤーの状態に応じてオフセットを調整
        float offsetMultiplier = playerManager.CurrentState == PlayerState.Run ? m_RunCameraOffset : m_WalkCameraOffset;
        Vector3 targetPosition = CalculateCameraTargetPosition(offsetMultiplier);
        ApplyObstacleAvoidance(ref targetPosition);

        m_MainCamera.transform.position = targetPosition;
        m_MainCamera.transform.rotation = CalculateCameraRotation();

        await UniTask.Yield(PlayerLoopTiming.Update);
    }


    /// <summary>
    /// カメラの回転を入力に基づいて更新する。
    /// </summary>
    /// <param name="mouseX">マウスの水平入力</param>
    /// <param name="mouseY">マウスの垂直入力</param>
    private void UpdateRotation(float mouseX, float mouseY)
    {
        m_CurrentRotationSpeedX = Mathf.SmoothDamp(m_CurrentRotationSpeedX, mouseX, ref m_RotationVelocityX, m_RotationSmoothTime);
        m_CurrentRotationSpeedY = Mathf.SmoothDamp(m_CurrentRotationSpeedY, mouseY, ref m_RotationVelocityY, m_RotationSmoothTime);

        m_CurrentHorizontalRotation += m_CurrentRotationSpeedX;
        m_CurrentVerticalRotation -= m_CurrentRotationSpeedY;
        m_CurrentVerticalRotation = Mathf.Clamp(m_CurrentVerticalRotation, -m_VerticalRotationLimit, m_VerticalRotationLimit);
    }

    /// <summary>
    /// カメラの目標位置を計算する。プレイヤーの位置と回転に基づいてカメラの位置を決定。
    /// </summary>
    /// <returns>カメラの目標位置</returns>
    private Vector3 CalculateCameraTargetPosition(float offsetMultiplier)
    {
        Quaternion horizontalRotation = Quaternion.Euler(0f, m_CurrentHorizontalRotation, 0f);
        Quaternion verticalRotation = Quaternion.Euler(m_CurrentVerticalRotation, 0f, 0f);
        Quaternion totalRotation = horizontalRotation * verticalRotation;

        m_Offset = totalRotation * Vector3.back * m_CurrentDistance * offsetMultiplier; // オフセットを調整
        return m_Player.position + m_Offset;
    }


    /// <summary>
    /// カメラの回転を計算する。プレイヤーの回転に基づいてカメラの回転を決定。
    /// </summary>
    /// <returns>カメラの回転</returns>
    private Quaternion CalculateCameraRotation()
    {
        Quaternion horizontalRotation = Quaternion.Euler(0f, m_CurrentHorizontalRotation, 0f);
        Quaternion verticalRotation = Quaternion.Euler(m_CurrentVerticalRotation, 0f, 0f);
        return horizontalRotation * verticalRotation;
    }

    /// <summary>
    /// カメラの障害物回避処理を行う。障害物がある場合はカメラ位置を調整。
    /// </summary>
    /// <param name="targetPosition">目標位置</param>
    private void ApplyObstacleAvoidance(ref Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_Player.position;
        if (Physics.SphereCast(m_Player.position, m_SphereCastRadius, direction.normalized, out RaycastHit hit, direction.magnitude, m_ObstacleMask))
        {
            targetPosition = hit.point - direction.normalized * m_SphereCastRadius;
        }
    }

    /// <summary>
    /// UIの開閉に応じてカメラ位置を切り替える。UIが開かれたときと閉じられたときでカメラの目標位置を変更。
    /// </summary>
    /// <param name="isUIOpen">UIが開いているかどうか</param>
    private async UniTaskVoid ToggleUICameraPosition(bool isUIOpen)
    {
        m_IsUIOpen = isUIOpen;
        m_IsCameraMoving = true;

        Vector3 targetLocalOffset;
        Quaternion targetRotation;

        if (isUIOpen)
        {
            // UIを開くときの目標位置と回転
            targetLocalOffset = m_UITargetPosition; // プレイヤーのローカル座標系に基づいた目標位置
            targetRotation = GetUITargetRotation();

            // 元の位置と回転を保存しておく
            m_OriginalPosition = m_PlayerTransfrom.InverseTransformPoint(transform.position);
            m_OriginalRotation = transform.rotation;
        }
        else
        {
            // UIを閉じるときは元の位置と回転に戻す
            targetLocalOffset = m_OriginalPosition;
            targetRotation = m_OriginalRotation;
        }

        // カメラをスムーズに移動
        await SmoothMoveCameraAsync(targetLocalOffset, targetRotation);

        m_IsCameraMoving = false;
    }

    /// <summary>
    /// UIを開いたときのカメラの目標位置を計算する。プレイヤーのローカル座標系を使用。
    /// </summary>
    /// <returns>カメラの目標位置（ローカル座標系）</returns>
    private Vector3 GetUITargetPosition()
    {
        return m_PlayerTransfrom.position + m_PlayerTransfrom.TransformDirection(m_UITargetPosition);
    }

    /// <summary>
    /// UIを開いたときのカメラの目標回転を計算する。プレイヤーの回転を基準に決定。
    /// </summary>
    /// <returns>カメラの目標回転</returns>
    private Quaternion GetUITargetRotation()
    {
        // UI時のターゲット回転をプレイヤーの回転に基づいて計算
        return Quaternion.Euler(m_UITargetRotation) * m_PlayerTransfrom.rotation;
    }

    /// <summary>
    /// 元の位置に戻る際のカメラの位置を計算する。オフセットを基に計算。
    /// </summary>
    /// <returns>カメラの元の位置</returns>
    private Vector3 GetOriginalPosition()
    {
        return m_Player.position + m_Offset;
    }


    /// <summary>
    /// 元の位置に戻る際のカメラの回転を計算する。プレイヤー位置を基に回転を決定。
    /// </summary>
    /// <returns>カメラの元の回転</returns>
    private Quaternion GetOriginalRotation()
    {
        return Quaternion.LookRotation(m_Player.position - GetOriginalPosition());
    }

    /// <summary>
    /// カメラをスムーズに移動させる。指定されたローカルオフセットと回転を使用してカメラを移動。
    /// </summary>
    /// <param name="targetLocalOffset">カメラの目標位置（ローカル座標系）</param>
    /// <param name="targetRotation">カメラの目標回転</param>
    private async UniTask SmoothMoveCameraAsync(Vector3 targetLocalOffset, Quaternion targetRotation)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < m_UIMoveDuration)
        {
            // ターゲット位置をプレイヤーのローカルオフセットに基づいて計算
            Vector3 targetPosition = m_PlayerTransfrom.TransformPoint(targetLocalOffset);

            // 現在のカメラ位置からターゲット位置への移動
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / m_UIMoveDuration);

            // 現在のカメラ回転からターゲット回転への移動
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / m_UIMoveDuration);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // 最終位置と回転をセット
        transform.position = m_PlayerTransfrom.TransformPoint(targetLocalOffset);
        transform.rotation = targetRotation;
    }
}
