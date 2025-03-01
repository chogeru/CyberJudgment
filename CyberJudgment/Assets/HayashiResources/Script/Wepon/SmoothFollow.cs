using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [SerializeField, Header("追従するtarget")]
    private Transform m_Target;
    [SerializeField, Header("targetに対するオフセット")]
    private Vector3 m_Offset;
    [SerializeField, Header("追従スピード")]
    private float m_FollowingSpeed;

    // 初期回転を保持する変数
    private Quaternion m_InitialRotation;

    private void Start()
    {
        // 初期回転を保存
        m_InitialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (m_Target != null)
        {
            Vector3 desiredPosition = m_Target.position + m_Target.rotation * m_Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, m_FollowingSpeed);
            transform.position = smoothedPosition;

            // 初期回転を基にY軸の回転のみを反映
            Quaternion targetRotation = Quaternion.Euler(m_InitialRotation.eulerAngles.x, m_Target.eulerAngles.y, m_InitialRotation.eulerAngles.z);

            // 現在の回転と目標回転を補間して滑らかに回転させる
            Quaternion desiredRotation = Quaternion.Lerp(transform.rotation, targetRotation, m_FollowingSpeed);
            transform.rotation = desiredRotation;
        }
    }
}
