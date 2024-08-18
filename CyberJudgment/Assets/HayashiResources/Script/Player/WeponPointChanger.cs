using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeponPointChanger : MonoBehaviour
{
    [SerializeField, Header("アニメーター")]
    private Animator m_Animator;

    [SerializeField, Header("背中のポイント")]
    private Transform m_BackPoint;

    [SerializeField, Header("手元のポイント")]
    private Transform m_HandPoint;

    [SerializeField, Header("移動させるポイント")]
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
    /// 現在のアニメーションに応じてポイントの位置を更新
    /// </summary>
    private void UpdatePointPosition()
    {
        //現在のアニメーションステートを所得
        var currentState = m_Animator.GetCurrentAnimatorStateInfo(0);
        //攻撃アニメーションが再生中かどうかを確認
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
    /// ポイントを手元に移動
    /// </summary>
    private void MovePointToHand()
    {
        m_MoveablePoint.SetParent(m_HandPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// ポイントを背中に移動
    /// </summary>
    private void MovePointToBack()
    {
        m_MoveablePoint.SetParent(m_BackPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
    }
}
