using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using AbubuResouse.UI;
using AbubuResouse.Log;

[System.Serializable]
public class Dialogue
{
    public string characterName;
    [TextArea(3, 10)] // ���s���T�|�[�g
    public string message;
}

public class TextTrigger : MonoBehaviour
{
    public Dialogue[] dialogues;
    [SerializeField]
    public int _currentIndex = 0;
    [SerializeField, Header("�T�E���h��")]
    private string _seName;
    [SerializeField,Header("SE����")]
    private float _seVolume;

    public void TriggerTextDisplay()
    {
        if (TextManager.Instance != null)
        {
            if (dialogues != null && dialogues.Length > 0)
            {
                if (_currentIndex < dialogues.Length)
                {
                    TextManager.Instance.ShowText(dialogues[_currentIndex].characterName, dialogues[_currentIndex].message);
                    DebugUtility.Log(dialogues[_currentIndex].message);
                    _currentIndex++;
                    SEManager.Instance.PlaySound(_seName, _seVolume);
                }
                else
                {
                    DebugUtility.Log("�e�L�X�g���Ȃ�");
                    TextManager.Instance.HideText();
                    ResetTextIndex();
                }
            }
            else
            {
                DebugUtility.LogError("�e�L�X�g�̗v�f���Ȃ�");
                TextManager.Instance.HideText();
                ResetTextIndex();
            }
        }
        else
        {
            Debug.LogError("�e�L�X�g���Ȃ�!!");
        }
        if (_currentIndex > dialogues.Length)
        {
            TextManager.Instance.HideText();
            ResetTextIndex();
        }
    }

    public void ResetTextIndex()
    {
        _currentIndex = 0;
    }
}
