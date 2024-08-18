using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPools;

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

    [SerializeField, Header("エフェクトのプレハブ")]
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
    /// 現在のアニメーションに応じてポイントの位置を更新
    /// </summary>
    private void UpdatePointPosition()
    {
        AnimatorStateInfo currentState = m_Animator.GetCurrentAnimatorStateInfo(0);

        // アニメーションステートが変更された場合
        if (currentState.fullPathHash != previousState.fullPathHash)
        {
            // 攻撃アニメーションが再生された瞬間
            if (currentState.IsName("NormalAttack") || currentState.IsName("StrongAttack"))
            {
                MovePointToHand();
            }
            // 攻撃アニメーションが終了した瞬間
            else if (previousState.IsName("NormalAttack") || previousState.IsName("StrongAttack"))
            {
                MovePointToBack();
            }
        }

        previousState = currentState;
    }

    /// <summary>
    /// ポイントを手元に移動
    /// </summary>
    private void MovePointToHand()
    {
        m_MoveablePoint.SetParent(m_HandPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
        // エフェクト生成
        SpawnEffect(m_HandPoint.position);
    }

    /// <summary>
    /// ポイントを背中に移動
    /// </summary>
    private void MovePointToBack()
    {
        m_MoveablePoint.SetParent(m_BackPoint);
        m_MoveablePoint.localPosition = Vector3.zero;
        m_MoveablePoint.localRotation = Quaternion.identity;
        // エフェクト生成
        SpawnEffect(m_BackPoint.position);
    }

    /// <summary>
    /// エフェクトを指定された位置に生成する
    /// </summary>
    /// <param name="position">エフェクトを生成する位置</param>
    private void SpawnEffect(Vector3 position)
    {
        // uPoolsを使用してエフェクトを生成
        GameObject effect = SharedGameObjectPool.Rent(
            m_EffectPrefab,
            position,
            Quaternion.identity);
        // 一定時間後にエフェクトを返却
        ReturnEffectAfterDelay(effect, 0.5f).Forget();

    }
    /// <summary>
    /// エフェクトを指定された時間後に返却する
    /// </summary>
    /// <param name="effect">返却するエフェクト</param>
    /// <param name="delay">返却までの遅延時間</param>
    private async UniTaskVoid ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        SharedGameObjectPool.Return(effect);
    }
}
