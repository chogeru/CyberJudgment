using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVoice : MonoBehaviour
{
    [SerializeField,Header("開始時に再生するボイス名")]
    private string _voiceName;

    [SerializeField, Header("ボイス音量")]
    private float _Volume;

    [SerializeField ,Header("開始時に自動再生")]
    private bool _StartOnPlay;

    private void Start()
    {
        if (_StartOnPlay)
        {
            VoiceManager.Instance.PlaySound(_voiceName, _Volume);
        }
    }

    public void PlayVoice()
    {
        VoiceManager.Instance.PlaySound(_voiceName, _Volume);
    }
}
