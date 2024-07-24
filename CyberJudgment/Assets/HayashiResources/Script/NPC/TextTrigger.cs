using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using AbubuResouse.UI;
using AbubuResouse.Log;

public class TextTrigger : MonoBehaviour
{
    public string[] _textsToDisplay;
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
            if (_textsToDisplay != null && _textsToDisplay.Length > 0)
            {
                if (_currentIndex < _textsToDisplay.Length)
                {
                    TextManager.Instance.ShowText(_textsToDisplay[_currentIndex]);
                    DebugUtility.Log(_textsToDisplay[_currentIndex]);
                    _currentIndex++;
                    SEManager.Instance.PlaySound(_seName, _seVolume);
                }
                else
                {
                    DebugUtility.Log("�e�L�X�g���Ȃ�");
                    TextManager.Instance.HideText();

                }
            }
            else
            {
                DebugUtility.LogError("�e�L�X�g�̗v�f���Ȃ�");
                TextManager.Instance.HideText(); 
            }
        }
        else
        {
            Debug.LogError("�e�L�X�g���Ȃ�!!");
        }
        if (_currentIndex > _textsToDisplay.Length)
        {
            TextManager.Instance.HideText(); 
        }
    }

    public void ResetTextIndex()
    {
        _currentIndex = 0;
    }
}
