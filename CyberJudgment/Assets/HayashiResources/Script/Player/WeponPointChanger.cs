using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeponPointChanger : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    private Animator m_Animator;

    [SerializeField, Header("�w���̃|�C���g")]
    private Transform m_BackPoint;

    [SerializeField, Header("�茳�̃|�C���g")]
    private Transform m_HandPoint;

    [SerializeField, Header("�ړ�������|�C���g")]
    private Transform m_MoveablePoint;

    private void Start()
    {
        UpdatePointPosition();
    }

    private void Update()
    {
        UpdatePointPosition();
    }

    /// <summary>
    /// ���݂̃A�j���[�V�����ɉ����ă|�C���g�̈ʒu���X�V
    /// </summary>
    private void UpdatePointPosition()
    {
        //���݂̃A�j���[�V�����X�e�[�g������
        var currentState = m_Animator.GetCurrentAnimatorStateInfo(0);
        //�U���A�j���[�V�������Đ������ǂ������m�F
        if (currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack"))
        {
            MovePointToHand();
        }
        else
        {
            MovePointToBack();
        }
    }

    /// <summary>
    /// �|�C���g���茳�Ɉړ�
    /// </summary>
    private void MovePointToHand()
    {
        m_MoveablePoint.SetParent(m_HandPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// �|�C���g��w���Ɉړ�
    /// </summary>
    private void MovePointToBack()
    {
        m_MoveablePoint.SetParent(m_BackPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
    }
}
