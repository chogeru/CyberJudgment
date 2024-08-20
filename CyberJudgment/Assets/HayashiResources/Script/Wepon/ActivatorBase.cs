using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatorBase : MonoBehaviour
{
    [SerializeField, Header("アニメーター")]
    protected Animator m_Animator;

    private AnimatorStateInfo previousState;

    private void Start()
    {
        // AttackEventManagerのイベントに登録
        AttackEventManager.OnEvent += ActivateItems;
        AttackEventManager.OffEvent += DeactivateItems;
    }

    private void Update()
    {
        UpdateItemState();
    }

    /// <summary>
    /// 現在のアニメーションに応じてアイテムの状態を更新
    /// </summary>
    private void UpdateItemState()
    {
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        // アニメーションステートが変更された場合
        if (currentState.fullPathHash != previousState.fullPathHash)
        {
            // 攻撃アニメーションが終了した瞬間
            if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
            {
                DeactivateItems();
            }
        }

        previousState = currentState;
    }

    /// <summary>
    /// アイテムをアクティブにする
    /// </summary>
    protected abstract void ActivateItems();

    /// <summary>
    /// アイテムを非アクティブにする
    /// </summary>
    protected abstract void DeactivateItems();

    private void OnDestroy()
    {
        // AttackEventManagerのイベントから解除
        AttackEventManager.OnEvent -= ActivateItems;
        AttackEventManager.OffEvent -= DeactivateItems;
    }
}