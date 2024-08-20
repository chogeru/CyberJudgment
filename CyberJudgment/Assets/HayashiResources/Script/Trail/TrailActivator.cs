using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailActivator : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    private Animator m_Animator;

    [SerializeField, Header("�g���C���G�t�F�N�g")]
    private TrailRenderer m_Trail;

    private AnimatorStateInfo previousState;

    private void Start()
    {
        m_Trail.enabled = false;

        // AnimationEventManager�̃C�x���g�ɓo�^
        AttackEventManager.OnEvent += ActivateTrail;
        AttackEventManager.OffEvent += DeactivateTrail;
    }

    private void Update()
    {
        UpdateTrailState();
    }

    /// <summary>
    /// ���݂̃A�j���[�V�����ɉ����ăg���C���̏�Ԃ��X�V
    /// </summary>
    private void UpdateTrailState()
    {
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        // �A�j���[�V�����X�e�[�g���ύX���ꂽ�ꍇ
        if (currentState.fullPathHash != previousState.fullPathHash)
        {
            // �U���A�j���[�V�������I�������u��
            if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
            {
                DeactivateTrail();
            }
        }

        previousState = currentState;
    }

    /// <summary>
    /// �g���C�����A�N�e�B�u�ɂ���
    /// </summary>
    private void ActivateTrail()
    {
        m_Trail.enabled = true;
    }

    /// <summary>
    /// �g���C�����A�N�e�B�u�ɂ���
    /// </summary>
    private void DeactivateTrail()
    {
        m_Trail.enabled = false;
    }

    private void OnDestroy()
    {
        // AnimationEventManager�̃C�x���g�������
        AttackEventManager.OnEvent -= ActivateTrail;
        AttackEventManager.OffEvent -= DeactivateTrail;
    }
}
