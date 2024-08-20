using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatorBase : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    protected Animator m_Animator;

    private AnimatorStateInfo previousState;

    private void Start()
    {
        // AttackEventManager�̃C�x���g�ɓo�^
        AttackEventManager.OnEvent += ActivateItems;
        AttackEventManager.OffEvent += DeactivateItems;
    }

    private void Update()
    {
        UpdateItemState();
    }

    /// <summary>
    /// ���݂̃A�j���[�V�����ɉ����ăA�C�e���̏�Ԃ��X�V
    /// </summary>
    private void UpdateItemState()
    {
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        // �A�j���[�V�����X�e�[�g���ύX���ꂽ�ꍇ
        if (currentState.fullPathHash != previousState.fullPathHash)
        {
            // �U���A�j���[�V�������I�������u��
            if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
            {
                DeactivateItems();
            }
        }

        previousState = currentState;
    }

    /// <summary>
    /// �A�C�e�����A�N�e�B�u�ɂ���
    /// </summary>
    protected abstract void ActivateItems();

    /// <summary>
    /// �A�C�e�����A�N�e�B�u�ɂ���
    /// </summary>
    protected abstract void DeactivateItems();

    private void OnDestroy()
    {
        // AttackEventManager�̃C�x���g�������
        AttackEventManager.OnEvent -= ActivateItems;
        AttackEventManager.OffEvent -= DeactivateItems;
    }
}