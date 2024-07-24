using TMPro;
using UnityEngine;
using AbubuResouse.Log;
using System.Threading;
using Cysharp.Threading.Tasks;

public class TextManager : SingletonMonoBehaviour<TextManager>
{
    [SerializeField]
    private GameObject _textWindowUI;

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;

    [SerializeField]
    private TextMeshProUGUI _nameMeshPro;

    public bool isTextEnd = false;
    private CancellationTokenSource _cts;

    public void ShowText(string characterName, string textToShow)
    {
        if (_textWindowUI != null)
        {
            isTextEnd = false;
            _textWindowUI.SetActive(true);
            _textMeshPro.gameObject.SetActive(true);
            _nameMeshPro.gameObject.SetActive(true);
            StopManager.Instance.IsStopped = true;
            if (_textMeshPro != null)
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                }
                _cts = new CancellationTokenSource();
                _nameMeshPro.text = characterName;
                TypeText(textToShow, _cts.Token).Forget();
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

    private async UniTaskVoid TypeText(string textToShow, CancellationToken token)
    {
        _textMeshPro.text = "";
        foreach(char letter in textToShow.ToCharArray())
        {
            _textMeshPro.text += letter;
            await UniTask.Delay(1, cancellationToken: token);
            if(token.IsCancellationRequested)
            {
                return;
            }
        }
    }
        public void HideText()
    {
        if (_textWindowUI != null)
        {
            isTextEnd = true;
            StopManager.Instance.IsStopped = false;
            _textWindowUI.SetActive(false);
            _textMeshPro.gameObject.SetActive(false);
            _nameMeshPro.gameObject.SetActive(false);
        }
    }
}
