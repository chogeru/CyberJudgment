using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPools;

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

    [SerializeField, Header("�G�t�F�N�g�̃v���n�u")]
    private GameObject m_EffectPrefab;

    private AnimatorStateInfo previousState;


    private void Start()
    {
        SharedGameObjectPool.Prewarm(m_EffectPrefab, 5);
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
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        // �A�j���[�V�����X�e�[�g���ύX���ꂽ�ꍇ
        if (currentState.fullPathHash != previousState.fullPathHash)
        {
            // �U���A�j���[�V�������Đ����ꂽ�u��
            if (currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack"))
            {
                MovePointToHand();
            }
            // �U���A�j���[�V�������I�������u��
            else if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
            {
                MovePointToBack();
            }
        }

        previousState = currentState;
    }

    /// <summary>
    /// �|�C���g���茳�Ɉړ�
    /// </summary>
    private void MovePointToHand()
    {
        m_MoveablePoint.SetParent(m_HandPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
        // �G�t�F�N�g����
        SpawnEffect(m_HandPoint.position);
    }

    /// <summary>
    /// �|�C���g��w���Ɉړ�
    /// </summary>
    private void MovePointToBack()
    {
        m_MoveablePoint.SetParent(m_BackPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
        // �G�t�F�N�g����
        SpawnEffect(m_BackPoint.position);
    }

    /// <summary>
    /// �G�t�F�N�g���w�肳�ꂽ�ʒu�ɐ�������
    /// </summary>
    /// <param name="position">�G�t�F�N�g�𐶐�����ʒu</param>
    private void SpawnEffect(Vector3 position)
    {
        // uPools���g�p���ăG�t�F�N�g�𐶐�
        GameObject effect = SharedGameObjectPool.Rent(
            m_EffectPrefab,
            position,
            Quaternion.identity);
        // ��莞�Ԍ�ɃG�t�F�N�g��ԋp
        ReturnEffectAfterDelay(effect, 0.5f).Forget();

    }
    /// <summary>
    /// �G�t�F�N�g���w�肳�ꂽ���Ԍ�ɕԋp����
    /// </summary>
    /// <param name="effect">�ԋp����G�t�F�N�g</param>
    /// <param name="delay">�ԋp�܂ł̒x������</param>
    private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        SharedGameObjectPool.Return(effect);
    }
}
