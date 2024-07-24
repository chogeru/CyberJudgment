using TMPro;
using UnityEngine;
using AbubuResouse.Log;

public class TextManager : SingletonMonoBehaviour<TextManager>
{
    [SerializeField]
    private GameObject _textWindowUI; 

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;

    public bool isTextEnd=false;
    public void ShowText(string textToShow)
    {
        if (_textWindowUI != null)
        {
            isTextEnd = false;
            _textWindowUI.SetActive(true); 
            _textMeshPro.gameObject.SetActive(true);
            StopManager.Instance.IsStopped = true;
            if (_textMeshPro != null)
            {
 
                _textMeshPro.text = textToShow;
            }
            else
            {
                DebugUtility.LogError("TMPが無い");
            }
        }
        else
        {
            DebugUtility.LogError("Textウィンドウがない");
        }
    }
    public void HideText()
    {
        if (_textWindowUI != null)
        {
            isTextEnd =true;
            StopManager.Instance.IsStopped = false;
            _textWindowUI.SetActive(false);
            _textMeshPro.gameObject.SetActive(false);
        }
    }
}
