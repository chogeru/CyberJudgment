using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Log;
using System.Diagnostics;

public class EnemySetCol : MonoBehaviour
{
    [SerializeField,Header("アニメーター")]
    private Animator _animator;

    [SerializeField, Header("EnemyData")]
    private EnemyData _enemyData;

    [SerializeField,Header("攻撃時にアクティブにするコライダー")]
    private Collider[] _attackColliders;

    private void Update()
    {
        if (!_animator.GetBool("Attack") && !_animator.GetBool("StrongAttack"))
        {
            DeactivateCollider();
        }
    }
    /// <summary>
    /// 指定した数のコライダーをアクティブにする関数
    /// </summary>
    /// <param name="count">数</param>

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
    /// 指定の番号のコライダーをアクティブにする関数
    /// </summary>
    /// <param name="index">番号</param>
    public void ActivateColliderByIndex(int index)
    {
        // 指定されたインデックスが有効な範囲内かチェック
        if (index >= 0 && index < _attackColliders.Length)
        {
            // 指定のコライダーをアクティブにする
            _attackColliders[index].enabled = true;
        }
        else
        {
            DebugUtility.Log("指定されたインデックスが範囲外です: " + index);
        }
    }

    /// <summary>
    /// 全てのコライダーを非アクティブにする関数
    /// </summary>
    public void DeactivateCollider()
    {
        foreach(Collider col in _attackColliders)
        {
            col.enabled = false;
        }
    }
}
