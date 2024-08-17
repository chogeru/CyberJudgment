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

    private void FixedUpdate()
    {
        if (m_Target != null)
        {
            Vector3 desiredPosition = m_Target.position + m_Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, m_FollowingSpeed);
            transform.position = smoothedPosition;
        }
    }
}
