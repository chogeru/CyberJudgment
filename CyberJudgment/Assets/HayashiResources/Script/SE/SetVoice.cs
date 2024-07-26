using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVoice : MonoBehaviour
{
    [SerializeField,Header("�J�n���ɍĐ�����{�C�X��")]
    private string _voiceName;

    [SerializeField, Header("�{�C�X����")]
    private float _Volume;

    [SerializeField ,Header("�J�n���Ɏ����Đ�")]
    private bool _StartOnPlay;

    private void Start()
    {
        if (_StartOnPlay)
        {
            VoiceManager.Instance.PlaySound(_voiceName, _Volume);
        }
    }

    /// <summary>
    /// �C�ӂ̃^�C�~���O�Ń{�C�X�Đ��p�֐�
    /// </summary>
    public void PlayVoice()
    {
        VoiceManager.Instance.PlaySound(_voiceName, _Volume);
    }
}
