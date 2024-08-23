using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Log;
using System.Diagnostics;

public class EnemySetCol : MonoBehaviour
{
    [SerializeField,Header("�A�j���[�^�[")]
    private Animator _animator;

    [SerializeField, Header("EnemyData")]
    private EnemyData _enemyData;

    [SerializeField,Header("�U�����ɃA�N�e�B�u�ɂ���R���C�_�[")]
    private Collider[] _attackColliders;

    private void Update()
    {
        if (!_animator.GetBool("Attack") && !_animator.GetBool("StrongAttack"))
        {
            DeactivateCollider();
        }
    }
    /// <summary>
    /// �w�肵�����̃R���C�_�[���A�N�e�B�u�ɂ���֐�
    /// </summary>
    /// <param name="count">��</param>

    public void ActivateColliders(int count)
    {
        DeactivateCollider();

        int activeCount = Mathf.Clamp(count, 0, _attackColliders.Length);

        for (int i = 0; i < _attackColliders.Length; i++)
        {
            _attackColliders[i].enabled = i < activeCount;
        }
    }

    /// <summary>
    /// �w��̔ԍ��̃R���C�_�[���A�N�e�B�u�ɂ���֐�
    /// </summary>
    /// <param name="index">�ԍ�</param>
    public void ActivateColliderByIndex(int index)
    {
        // �w�肳�ꂽ�C���f�b�N�X���L���Ȕ͈͓����`�F�b�N
        if (index >= 0 && index < _attackColliders.Length)
        {
            // �w��̃R���C�_�[���A�N�e�B�u�ɂ���
            _attackColliders[index].enabled = true;
        }
        else
        {
            DebugUtility.Log("�w�肳�ꂽ�C���f�b�N�X���͈͊O�ł�: " + index);
        }
    }

    /// <summary>
    /// �S�ẴR���C�_�[���A�N�e�B�u�ɂ���֐�
    /// </summary>
    public void DeactivateCollider()
    {
        foreach(Collider col in _attackColliders)
        {
            col.enabled = false;
        }
    }
}
