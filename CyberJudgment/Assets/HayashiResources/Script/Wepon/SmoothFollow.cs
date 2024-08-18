using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [SerializeField, Header("�Ǐ]����target")]
    private Transform m_Target;
    [SerializeField, Header("target�ɑ΂���I�t�Z�b�g")]
    private Vector3 m_Offset;
    [SerializeField, Header("�Ǐ]�X�s�[�h")]
    private float m_FollowingSpeed;

    // ������]��ێ�����ϐ�
    private Quaternion m_InitialRotation;

    private void Start()
    {
        // ������]��ۑ�
        m_InitialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (m_Target != null)
        {
            Vector3 desiredPosition = m_Target.position + m_Target.rotation * m_Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, m_FollowingSpeed);
            transform.position = smoothedPosition;

            // ������]�����Y���̉�]�݂̂𔽉f
            Quaternion targetRotation = Quaternion.Euler(m_InitialRotation.eulerAngles.x, m_Target.eulerAngles.y, m_InitialRotation.eulerAngles.z);

            // ���݂̉�]�ƖڕW��]���Ԃ��Ċ��炩�ɉ�]������
            Quaternion desiredRotation = Quaternion.Lerp(transform.rotation, targetRotation, m_FollowingSpeed);
            transform.rotation = desiredRotation;
        }
    }
}
