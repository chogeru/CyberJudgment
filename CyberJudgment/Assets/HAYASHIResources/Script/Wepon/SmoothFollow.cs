using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [SerializeField, Header("’Ç]‚·‚étarget")]
    private Transform m_Target;
    [SerializeField, Header("target‚É‘Î‚·‚éƒIƒtƒZƒbƒg")]
    private Vector3 m_Offset;
    [SerializeField, Header("’Ç]ƒXƒs[ƒh")]
    private float m_FollowingSpeed;

    // ‰Šú‰ñ“]‚ğ•Û‚·‚é•Ï”
    private Quaternion m_InitialRotation;

    private void Start()
    {
        // ‰Šú‰ñ“]‚ğ•Û‘¶
        m_InitialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (m_Target != null)
        {
            Vector3 desiredPosition = m_Target.position + m_Target.rotation * m_Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, m_FollowingSpeed);
            transform.position = smoothedPosition;

            // ‰Šú‰ñ“]‚ğŠî‚ÉY²‚Ì‰ñ“]‚Ì‚İ‚ğ”½‰f
            Quaternion targetRotation = Quaternion.Euler(m_InitialRotation.eulerAngles.x, m_Target.eulerAngles.y, m_InitialRotation.eulerAngles.z);

            // Œ»İ‚Ì‰ñ“]‚Æ–Ú•W‰ñ“]‚ğ•âŠÔ‚µ‚ÄŠŠ‚ç‚©‚É‰ñ“]‚³‚¹‚é
            Quaternion desiredRotation = Quaternion.Lerp(transform.rotation, targetRotation, m_FollowingSpeed);
            transform.rotation = desiredRotation;
        }
    }
}
