using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBGM : MonoBehaviour
{
    [SerializeField, Header("開始時にセットするBGM")]
    private string m_BGMName;
    [SerializeField, Header("音量")]
    private float m_Volume;
    [SerializeField, Header("ランダムBGM")]
    private string[] m_RandomBGMNames;
    [SerializeField, Header("ランダム再生をオンにするかどうか")]
    private bool isRondomBGM;
    private void Start()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.GetComponent<AudioSource>().clip = null;
            if (!string.IsNullOrEmpty(m_BGMName))
            {
                BGMManager.Instance.PlayBGM(m_BGMName, m_Volume);
            }
        }
    }

    private void Update()
    {
        if (BGMManager.Instance != null)
        {
            if (isRondomBGM)
            {
                AudioSource audioSource = BGMManager.Instance.GetComponent<AudioSource>();
                audioSource.loop = false;
                if (!audioSource.isPlaying)
                {
                    var index = Random.Range(0, m_RandomBGMNames.Length);
                    BGMManager.Instance.PlayBGM(m_RandomBGMNames[index], m_Volume);
                }
            }
        }
    }
}
