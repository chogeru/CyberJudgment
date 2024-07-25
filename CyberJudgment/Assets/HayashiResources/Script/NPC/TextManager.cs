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

    /// <summary>
    /// テキストを表示する処理
    /// </summary>
    /// <param name="characterName"></param>
    /// <param name="textToShow"></param>
    public void ShowText(string characterName, string textToShow)
    {
        if (_textWindowUI == null || _textMeshPro == null)
        {
            DebugUtility.LogError("Textウィンドウがない");
            return;
        }

        isTextEnd = false;
        _textWindowUI.SetActive(true);
        _textMeshPro.gameObject.SetActive(true);
        _nameMeshPro.gameObject.SetActive(true);
        StopManager.Instance.IsStopped = true;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _nameMeshPro.text = characterName;
        TypeText(textToShow, _cts.Token).Forget();
    }

    /// <summary>
    /// テキストをタイプ表示する処理（非同期）
    /// </summary>
    /// <param name="textToShow"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTaskVoid TypeText(string textToShow, CancellationToken token)
    {
        _textMeshPro.text = "";
        foreach (char letter in textToShow.ToCharArray())
        {
            _textMeshPro.text += letter;
            await UniTask.Delay(1, cancellationToken: token);
            if (token.IsCancellationRequested)
            {
                return;
            }
        }
    }

    /// <summary>
    /// テキストを非表示にする処理
    /// </summary>
    public void HideText()
    {
        if (_textWindowUI == null)
        {
            return;
        }

        isTextEnd = true;
        StopManager.Instance.IsStopped = false;
        _textWindowUI.SetActive(false);
        _textMeshPro.gameObject.SetActive(false);
        _nameMeshPro.gameObject.SetActive(false);
    }
}

