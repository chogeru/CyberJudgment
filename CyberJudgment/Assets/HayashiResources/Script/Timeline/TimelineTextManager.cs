using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TimelineTextManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _textWindowUI;

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;

    [SerializeField]
    private TextMeshProUGUI _nameMeshPro;

    public bool isTextEnd = false;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (_textWindowUI != null)
        {
            _textWindowUI.SetActive(false);
        }
    }

    /// <summary>
    /// テキストを表示する処理
    /// </summary>
    /// <param name="characterName">キャラクター名</param>
    /// <param name="textToShow">表示するテキスト</param>
    public void ShowText(string characterName, string textToShow)
    {
        if (_textWindowUI == null || _textMeshPro == null)
        {
            Debug.LogError("Textウィンドウがない");
            return;
        }

        isTextEnd = false;
        _textWindowUI.SetActive(true);
        _textMeshPro.gameObject.SetActive(true);
        _nameMeshPro.gameObject.SetActive(true);
        StopManager.Instance.IsStopped = true;  // テキスト表示中は他の動作を停止する

        _cts?.Cancel();  // 前回の表示をキャンセル
        _cts = new CancellationTokenSource();
        _nameMeshPro.text = characterName;  // キャラクター名を設定
        TypeText(textToShow, _cts.Token).Forget();  // テキストを非同期でタイプ表示
    }

    /// <summary>
    /// テキストをタイプ表示する処理（非同期）
    /// </summary>
    /// <param name="textToShow">表示するテキスト</param>
    /// <param name="token">キャンセルトークン</param>
    /// <returns></returns>
    private async UniTaskVoid TypeText(string textToShow, CancellationToken token)
    {
        _textMeshPro.text = "";
        foreach (char letter in textToShow.ToCharArray())
        {
            _textMeshPro.text += letter;
            await UniTask.Delay(1, cancellationToken: token);  // タイプ速度を調整
            if (token.IsCancellationRequested)
            {
                return;  // キャンセルされた場合は終了
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
        StopManager.Instance.IsStopped = false;  // 他の動作を再開する
        _textWindowUI.SetActive(false);
        _textMeshPro.gameObject.SetActive(false);
        _nameMeshPro.gameObject.SetActive(false);
    }
}
