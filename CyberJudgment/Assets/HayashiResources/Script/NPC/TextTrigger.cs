using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using AbubuResouse.Log;
using AbubuResouse.Singleton;

[System.Serializable]
public class Dialogue
{
    public string characterName;
    [TextArea(3, 10)]
    public string message;
    public string seName;
    public float seVolume = 1.0f;
}

public class TextTrigger : MonoBehaviour
{
    public Dialogue[] dialogues;
    [SerializeField]
    public int _currentIndex = 0;
    [SerializeField, Header("サウンド名")]
    private string _seName;
    [SerializeField,Header("SE音量")]
    private float _seVolume;

    /// <summary>
    /// テキスト表示をトリガーする処理
    /// </summary>
    public void TriggerTextDisplay()
    {
        if (TextManager.Instance == null || dialogues == null || dialogues.Length == 0)
        {
            DebugUtility.LogError("テキストの要素がない");
            SEManager.Instance.PlaySound(this._seName, this._seVolume);
            TextManager.Instance?.HideText();
            ResetTextIndex();
            return;
        }

        if (_currentIndex < dialogues.Length)
        {
            var currentDialogue = dialogues[_currentIndex];
            TextManager.Instance.ShowText(currentDialogue.characterName, currentDialogue.message);
            PlaySoundEffect(currentDialogue.seName, currentDialogue.seVolume);
            SEManager.Instance.PlaySound(this._seName, this._seVolume);
            _currentIndex++;
        }
        else
        {
            DebugUtility.Log("テキストがない");
            TextManager.Instance.HideText();
            ResetTextIndex();
        }

        if (_currentIndex > dialogues.Length)
        {
            TextManager.Instance.HideText();
            ResetTextIndex();
        }
    }

    /// <summary>
    /// 効果音を再生する処理
    /// </summary>
    /// <param name="seName"></param>
    /// <param name="seVolume"></param>
    private void PlaySoundEffect(string seName, float seVolume)
    {
        if (!string.IsNullOrEmpty(seName))
        {
            VoiceManager.Instance.PlaySound(seName, seVolume);
        }
    }

    /// <summary>
    /// テキストインデックスをリセットする処理
    /// </summary>
    public void ResetTextIndex()
    {
        _currentIndex = 0;
    }
}
