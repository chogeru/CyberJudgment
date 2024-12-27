using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Singleton;
using Cysharp.Threading.Tasks;
using uPools;

public class FootStep : MonoBehaviour
{
    [SerializeField, Header("足音の間隔")]
    private float m_FootStepTime = 0.05f;

    // 経過時間
    private float m_ElapsedTime;

    // 足音エフェクトのプレハブ
    public GameObject m_EffectPrefab;

    private void Update()
    {
        m_ElapsedTime += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 足音生成の間隔をチェック
        if (m_ElapsedTime > m_FootStepTime)
        {
            GenerateFootstep();
            m_ElapsedTime = 0;
        }
    }

    /// <summary>
    /// 足音エフェクトを生成
    /// </summary>
    private void GenerateFootstep()
    {
        // EffectManager を使用してエフェクトを再生
        Vector3 effectPosition = transform.position;
        EffectManager.Instance.PlayEffect(m_EffectPrefab, effectPosition, Quaternion.identity, 0.5f);
    }
}
