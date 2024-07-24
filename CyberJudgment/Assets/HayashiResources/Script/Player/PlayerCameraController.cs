using System.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField,Header("プレイヤーのTransform")]
    private Transform m_Player;
    [SerializeField,Header("マウス感度")]
    private float m_Sensitivity = 2.0f;
    [SerializeField,Header("ゲームパッド感度")]
    private float m_GamepadSensitivity = 3.0f;
    [SerializeField,Header("ズーム感度")]
    private float m_ZoomSensitivity = 2.0f; 
    [SerializeField,Header("障害物レイヤー")]
    private LayerMask m_ObstacleMask;
    [SerializeField,Header("ゲームパッドのスクロール速度")]
    private float m_GamePadScrollSpeed;
    [SerializeField,Header("カメラとプレイヤーの最小距離を")]
    private float m_MinDistance = 1.0f;
    [SerializeField,Header("カメラとプレイヤーの最大距離")]
    private float m_MaxDistance = 10.0f;
    [SerializeField,Header("スフィアキャストの半径")]
    private float m_SphereCastRadius = 0.5f;
    [SerializeField,Header("カメラのズーム動作の滑らかさ")]
    private float m_SmoothTime = 0.2f;


    private Camera m_MainCamera;
    //カメラとプレイヤー間のオフセット
    private Vector3 m_Offset;
    //現在のカメラとプレイヤーの距離
    private float m_CurrentDistance;
    //目的のカメラとプレイヤーの距離感
    private float m_TargetDistance;
    //ズーム動作の速度
    private float m_ZoomVelocity = 0.0f;

    private UIPresenter _uiPresenter;

    void Start()
    {
        m_MainCamera = GetComponent<Camera>();
        m_Offset = transform.position - m_Player.position;
        m_CurrentDistance = m_Offset.magnitude;
        m_TargetDistance = m_CurrentDistance;

        // UIPresenterのインスタンスを取得
        _uiPresenter = UIPresenter.Instance;

        // UIがひらいていない時カメラの位置を変更する
        Observable.EveryUpdate()
            .Select(_ =>
            {
                if (_uiPresenter == null || _uiPresenter.IsMenuOpen || !string.IsNullOrEmpty(_uiPresenter.CurrentUIObject))
                {
                    return new Tuple<Vector2, float>(Vector2.zero, 0f);
                }

                Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X") * m_Sensitivity, Input.GetAxis("Mouse Y") * m_Sensitivity);
                Vector2 gamepadInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
                float scrollInput = Mouse.current.scroll.y.ReadValue();
                float gamepadZoomInput = 0.0f;
                if (Gamepad.current != null)
                {
                    if (Gamepad.current.buttonNorth.isPressed)
                    {
                        gamepadZoomInput = m_GamePadScrollSpeed;
                    }
                    else if (Gamepad.current.buttonWest.isPressed)
                    {
                        gamepadZoomInput = -m_GamePadScrollSpeed;
                    }
                }
                // ゲームパッド用感度
                Vector2 adjustedGamepadInput = gamepadInput * m_GamepadSensitivity;
                return new Tuple<Vector2, float>(mouseInput + new Vector2(gamepadInput.x * m_Sensitivity, gamepadInput.y * m_Sensitivity), scrollInput + gamepadZoomInput);
            })
            .Subscribe(inputs =>
            {
                UpdateCameraPositionAsync(inputs.Item1.x, inputs.Item1.y, inputs.Item2).Forget();
            }).AddTo(this);
    }


    private async UniTaskVoid UpdateCameraPositionAsync(float mouseX, float mouseY, float scrollInput)
    {
        if (StopManager.Instance.IsStopped)
            return;
        m_Offset = Quaternion.AngleAxis(mouseX, Vector3.up) * Quaternion.AngleAxis(-mouseY, m_MainCamera.transform.right) * m_Offset;

        //スクロール入力を使用してターゲット距離を更新（m_ZoomSensitivityを使用）
        m_TargetDistance = Mathf.Clamp(m_TargetDistance - scrollInput * m_ZoomSensitivity * Time.deltaTime, m_MinDistance, m_MaxDistance);

        //現在の距離をターゲット距離に滑らかに補間
        m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, m_TargetDistance, ref m_ZoomVelocity, m_SmoothTime);

        //オフセットの長さを現在の距離に設定
        m_Offset = m_Offset.normalized * m_CurrentDistance;

        Vector3 newPosition = m_Player.position + m_Offset;
        Vector3 direction = newPosition - m_Player.position;

        //カメラが障害物にぶつからないようにする
        if (Physics.Raycast(m_Player.position, direction.normalized, out RaycastHit hit, direction.magnitude, m_ObstacleMask))
        {
            newPosition = hit.point - direction.normalized * m_MinDistance;
        }

        m_MainCamera.transform.position = newPosition;
        m_MainCamera.transform.LookAt(m_Player.position);

        await UniTask.Yield(PlayerLoopTiming.Update);
    }
}
