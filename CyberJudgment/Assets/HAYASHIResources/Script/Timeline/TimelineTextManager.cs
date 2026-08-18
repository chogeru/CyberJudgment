using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using System.Threading;
using AbubuResouse.Singleton;

public class TimelineTextManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _textWindowUI;
    [SerializeField]
    private TextMeshProUGUI _textMeshPro;
    [SerializeField]
    private TextMeshProUGUI _nameMeshPro;

    [Header("タイプ音設定")]
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _typeSound;
    [SerializeField]
    private bool _playTypeSound = true;
    [SerializeField]
    private float _typeSoundVolume = 1f;

    public bool isTextEnd = false;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (_textWindowUI != null)
        {
            _textWindowUI.SetActive(false);
        }

        // AudioSourceが設定されていない場合は自動で追加
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.volume = _typeSoundVolume;
            }
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

            // スペースや改行以外の文字でタイプ音を再生
            if (_playTypeSound && _typeSound != null && _audioSource != null &&
                !char.IsWhiteSpace(letter))
            {
                _audioSource.PlayOneShot(_typeSound, _typeSoundVolume);
            }

            await UniTask.Delay(50, cancellationToken: token);  // タイプ速度を調整（50ms）

            if (token.IsCancellationRequested)
            {
                return;  // キャンセルされた場合は終了
            }
        }

        isTextEnd = true;  // タイプ完了時にフラグを設定
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

        // 音の再生を停止
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }

    /// <summary>
    /// タイプ音の設定を変更
    /// </summary>
    /// <param name="enabled">音を再生するかどうか</param>
    public void SetTypeSoundEnabled(bool enabled)
    {
        _playTypeSound = enabled;
    }

    /// <summary>
    /// タイプ音の音量を変更
    /// </summary>
    /// <param name="volume">音量（0.0f～1.0f）</param>
    public void SetTypeSoundVolume(float volume)
    {
        _typeSoundVolume = Mathf.Clamp01(volume);
        if (_audioSource != null)
        {
            _audioSource.volume = _typeSoundVolume;
        }
    }
}