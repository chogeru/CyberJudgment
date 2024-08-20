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

        // AnimationEventManagerのイベントに登録
        AttackEventManager.OnEvent += ActivateTrail;
        AttackEventManager.OffEvent += DeactivateTrail;
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
            // 攻撃アニメーションが終了した瞬間
            if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
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

    private void OnDestroy()
    {
        // AnimationEventManagerのイベントから解除
        AttackEventManager.OnEvent -= ActivateTrail;
        AttackEventManager.OffEvent -= DeactivateTrail;
    }
}
