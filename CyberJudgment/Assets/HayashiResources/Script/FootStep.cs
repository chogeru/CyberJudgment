using SRDebugger.UI.Other;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    [SerializeField, Header("足音の間隔")]
    private float m_FootStepTIme=0.05f;
    //経過時間
    private float m_ElapsedTime;

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
        SEManager.Instance.PlaySound("FootStep",0.3f);
        GameObject hitEffect = EffectFootStepObjctPool.Instance.GetPooledObject();
        hitEffect.transform.position = transform.position;
        hitEffect.transform.rotation = Quaternion.identity;
        hitEffect.SetActive(true);

        Vector3 forward = transform.forward;
        forward.y = 0; // y軸方向の回転を無効にする
        hitEffect.transform.rotation = Quaternion.LookRotation(forward);
    }
}
