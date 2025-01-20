using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ItemMove : MonoBehaviour
{
    [SerializeField, Header("�^�[�Q�b�g�ƂȂ�^�O")]
    private string m_TargetTag = "Player";

    [SerializeField, Header("�v���C���[�̌��Ɍ��������x")]
    private float m_Speed = 5f;
    [SerializeField, Header("�^�[�Q�b�g�ʒu��Y���I�t�Z�b�g")]
    private float m_YOffset = 1f;
    private Collider m_Collider;
    private Rigidbody m_Rb;
    private Transform m_Target;

    private bool m_IsFollowingPlayer = false;

    void Start()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();

        m_Collider.isTrigger = false;

        Invoke("DisableTriggerAfterTime", 3f);
    }

    private void DisableTriggerAfterTime()
    {

        m_Collider.isTrigger = true;

        m_Rb.useGravity = false;
        InitializeMovement();
    }

    /// <summary>
    /// �v���C���[�ƏՓ˂�����g���K�[��؂�
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (!m_Collider.isTrigger) return;
        if (!collision.gameObject.CompareTag(m_TargetTag)) return;

        m_Collider.isTrigger = true;
        m_Rb.useGravity = false;

    }

    /// <summary>
    /// �v���C���[�Ǐ]�J�n����
    /// </summary>
    private void InitializeMovement()
    {
        GameObject targetObj = GameObject.FindGameObjectWithTag(m_TargetTag);
        if (targetObj != null)
        {
            m_Target = targetObj.transform;
            m_IsFollowingPlayer = true;
            m_Rb.useGravity = false;
        }
    }

    void Update()
    {
        if (m_IsFollowingPlayer && m_Target != null)
        {
            Vector3 targetPosition = m_Target.position + Vector3.up * m_YOffset;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.Translate(direction * m_Speed * Time.deltaTime, Space.World);
        }
    }
}
