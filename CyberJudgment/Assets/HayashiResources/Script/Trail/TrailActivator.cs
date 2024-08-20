using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailActivator : MonoBehaviour
{
    [SerializeField, Header("アニメーター")]
    private Animator m_Animator;

    [SerializeField, Header("トレイルエフェクト")]
    private TrailRenderer m_Trail;

    private AnimatorStateInfo previousState;

    private void Start()
    {
        m_Trail.enabled = false;
    }

    private void Update()
    {
        UpdateTrailState();
    }

    /// <summary>
    /// 現在のアニメーションに応じてトレイルの状態を更新
    /// </summary>
    private void UpdateTrailState()
    {
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        // アニメーションステートが変更された場合
        if (currentState.fullPathHash != previousState.fullPathHash)
        {
            // 攻撃アニメーションが再生された瞬間
            if (currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack"))
            {
                ActivateTrail();
            }
            // 攻撃アニメーションが終了した瞬間
            else if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
            {
                DeactivateTrail();
            }
        }

        previousState = currentState;
    }

    /// <summary>
    /// トレイルをアクティブにする
    /// </summary>
    private void ActivateTrail()
    {
        m_Trail.enabled = true;
    }

    /// <summary>
    /// トレイルを非アクティブにする
    /// </summary>
    private void DeactivateTrail()
    {
        m_Trail.enabled = false;
    }
}
