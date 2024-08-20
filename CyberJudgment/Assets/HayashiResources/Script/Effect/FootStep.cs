using SRDebugger.UI.Other;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Singleton;
using uPools;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

public class FootStep : MonoBehaviour
{
    [SerializeField, Header("足音の間隔")]
    private float m_FootStepTIme=0.05f;
    //経過時間
    private float m_ElapsedTime;

    public GameObject m_EffectPrefab;

    private void Start()
    {
        SharedGameObjectPool.Prewarm(m_EffectPrefab, 20);
    }

    private void Update()
    {
        m_ElapsedTime += Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (m_ElapsedTime > m_FootStepTIme)
        {
            GenerateFootstep();
            m_ElapsedTime = 0;
        }
    }

    void GenerateFootstep()
    {
        SEManager.Instance.PlaySound("FootStep",0.1f);

        // uPoolsを使用してエフェクトを生成
        GameObject effect = SharedGameObjectPool.Rent(
            m_EffectPrefab,
            transform.position,
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
