using System.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using AbubuResouse.Singleton;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField, Header("プレイヤーのTransform")]
    private Transform m_Player;
    [SerializeField,Header("プレイヤーの本体の位置")]
    private Transform m_PlayerTransfrom;
    [SerializeField, Header("マウス感度")]
    private float m_Sensitivity = 2.0f;
    [SerializeField, Header("ゲームパッド感度")]
    private float m_GamepadSensitivity = 3.0f;
    [SerializeField, Header("障害物レイヤー")]
    private LayerMask m_ObstacleMask;
    [SerializeField, Header("スフィアキャストの半径")]
    private float m_SphereCastRadius = 0.5f;

    [SerializeField, Header("回転の加速時間")]
    private float m_RotationSmoothTime = 0.17f;

    [SerializeField, Header("UI開閉時のカメラ目標位置")]
    private Vector3 m_UITargetPosition;
    [SerializeField, Header("UI開閉時のカメラ目標回転")]
    private Vector3 m_UITargetRotation;
    [SerializeField, Header("UIカメラ移動時間")]
    private float m_UIMoveDuration = 1.0f;

    private Camera m_MainCamera;
    private Vector3 m_Offset;
    private float m_CurrentDistance;

    private UIPresenter _uiPresenter;

    private float m_CurrentRotationSpeedX = 0f;
    private float m_CurrentRotationSpeedY = 0f;
    private float m_RotationVelocityX = 0f;
    private float m_RotationVelocityY = 0f;

    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;
    private bool m_IsUIOpen = false;
    private bool m_IsCameraMoving = false;

    void Start()
    {
        m_MainCamera = GetComponent<Camera>();
        m_Offset = transform.position - m_Player.position;
        m_CurrentDistance = m_Offset.magnitude;

        _uiPresenter = UIPresenter.Instance;

        m_OriginalPosition = transform.position;
        m_OriginalRotation = transform.rotation;

        Observable.EveryUpdate()
            .Select(_ =>
            {
                if (_uiPresenter == null || _uiPresenter.IsMenuOpen || !string.IsNullOrEmpty(_uiPresenter.CurrentUIObject) || m_IsCameraMoving)
                {
                    return Vector2.zero;
                }

                Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X") * m_Sensitivity, Input.GetAxis("Mouse Y") * m_Sensitivity);
                Vector2 gamepadInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
                return mouseInput + new Vector2(gamepadInput.x * m_Sensitivity, gamepadInput.y * m_Sensitivity);
            })
            .Subscribe(inputs =>
            {
                UpdateCameraPositionAsync(inputs.x, inputs.y).Forget();
            }).AddTo(this);

        // Subscribe to UI open/close state changes
        Observable.EveryUpdate()
            .Where(_ => _uiPresenter.IsMenuOpen != m_IsUIOpen)
            .Subscribe(_ => ToggleUICameraPosition(_uiPresenter.IsMenuOpen).Forget())
            .AddTo(this);
    }

    private async UniTaskVoid UpdateCameraPositionAsync(float mouseX, float mouseY)
    {
        if (StopManager.Instance.IsStopped || m_IsUIOpen || m_IsCameraMoving) // カメラ移動中の操作を無効化
            return;

        // 現在の回転速度を滑らかに更新
        m_CurrentRotationSpeedX = Mathf.SmoothDamp(m_CurrentRotationSpeedX, mouseX, ref m_RotationVelocityX, m_RotationSmoothTime);
        m_CurrentRotationSpeedY = Mathf.SmoothDamp(m_CurrentRotationSpeedY, mouseY, ref m_RotationVelocityY, m_RotationSmoothTime);

        // オフセットを回転に基づいて更新する
        Quaternion horizontalRotation = Quaternion.AngleAxis(m_CurrentRotationSpeedX, Vector3.up);
        Quaternion verticalRotation = Quaternion.AngleAxis(-m_CurrentRotationSpeedY, m_MainCamera.transform.right);
        m_Offset = horizontalRotation * verticalRotation * m_Offset;

        // プレイヤーの位置に基づいてカメラの新しい位置を計算
        Vector3 targetPosition = m_Player.position + m_Offset;
        Vector3 direction = targetPosition - m_Player.position;

        // 障害物をチェックしてカメラ位置を調整
        if (Physics.SphereCast(m_Player.position, m_SphereCastRadius, direction.normalized, out RaycastHit hit, direction.magnitude, m_ObstacleMask))
        {
            targetPosition = hit.point - direction.normalized * m_SphereCastRadius;
        }

        // カメラの位置と回転を更新
        m_MainCamera.transform.position = targetPosition;
        m_MainCamera.transform.rotation = Quaternion.LookRotation(m_Player.position - targetPosition);

        await UniTask.Yield(PlayerLoopTiming.Update);
    }

    private async UniTaskVoid ToggleUICameraPosition(bool isUIOpen)
    {
        m_IsUIOpen = isUIOpen;
        m_IsCameraMoving = true; // カメラ移動開始

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (isUIOpen)
        {
            // UIが開かれた時のカメラの目標位置と回転
            targetPosition = m_PlayerTransfrom.position + m_PlayerTransfrom.TransformDirection(m_UITargetPosition);
            targetRotation = Quaternion.Euler(m_UITargetRotation) * m_PlayerTransfrom.rotation;
        }
        else
        {
            // UIが閉じられた時のカメラの元の位置と回転にスムーズに戻る
            targetPosition = m_Player.position + m_Offset; // 現在のオフセットに基づいてターゲット位置を計算
            targetRotation = Quaternion.LookRotation(m_Player.position - targetPosition); // プレイヤーを向く回転を計算
        }

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < m_UIMoveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / m_UIMoveDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / m_UIMoveDuration);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        m_IsCameraMoving = false; // カメラ移動終了
    }

}