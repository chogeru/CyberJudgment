using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BossUIManager : MonoBehaviour
{
    [SerializeField, Header("ボスの名前を表示するText")]
    private TMP_Text _bossNameText;

    [SerializeField, Header("ボスの残り体力を表示するスライダー")]
    private Slider _healthSlider;

    [SerializeField, Header("ボスのフレームImage")]
    private Image _bossFrameImage;

    [SerializeField, Header("ボスフレームのCanvasGroup")]
    private CanvasGroup _bossFrameCanvasGroup;

    [SerializeField, Header("スライダーのCanvasGroup")]
    private CanvasGroup _sliderCanvasGroup;

    [SerializeField, Header("アニメーションにかける時間 (秒)")]
    private float _fadeDuration = 3.0f;

    private CancellationTokenSource _cts; // キャンセル用のトークン

    private void Awake()
    {
        _cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    /// <summary>
    /// ボスの名前を設定する
    /// </summary>
    /// <param name="bossName">表示するボスの名前</param>
    public void SetBossName(string bossName)
    {
        if (_bossNameText != null)
        {
            _bossNameText.text = bossName;
        }
        else
        {
            Debug.LogError("ボスの名前のTextが設定されていない！");
        }
    }

    /// <summary>
    /// ボスの体力を設定する
    /// </summary>
    /// <param name="currentHealth">現在の体力</param>
    /// <param name="maxHealth">最大体力</param>
    public void SetBossHealth(float currentHealth, float maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = currentHealth;
        }
        else
        {
            Debug.LogError("体力Sliderが設定されていない！");
        }
    }

    /// <summary>
    /// ボスUIを開始する
    /// </summary>
    [ContextMenu("StartBossUI")]
    public async UniTask StartBossUI()
    {
        if (_bossFrameImage != null && _bossFrameCanvasGroup != null && _sliderCanvasGroup != null)
        {
            _bossFrameImage.gameObject.SetActive(true);
            _healthSlider.gameObject.SetActive(true); // スライダーは最初非アクティブ

            // ボスフレームのフェードイン
            await FadeInCanvasGroup(_bossFrameCanvasGroup, 0f, 1f, _fadeDuration, _cts.Token);

            // 3秒待機
            await UniTask.Delay(3000, cancellationToken: _cts.Token);

            // ボスフレームのフェードアウトとスライダーのフェードインを同時に行う
            await UniTask.WhenAll(
                FadeOutCanvasGroup(_bossFrameCanvasGroup, 1f, 0f, _fadeDuration, _cts.Token),
                FadeInCanvasGroup(_sliderCanvasGroup, 0f, 1f, _fadeDuration, _cts.Token)
            );

            // フェードアウト完了後にボスフレームを非アクティブに
            _bossFrameImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("UI要素が設定されていない！");
        }
    }

    /// <summary>
    /// ボスUIをリセットする
    /// </summary>
    [ContextMenu("ResetBossUI")]
    public void ResetBossUI()
    {
        if (_bossFrameCanvasGroup != null && _sliderCanvasGroup != null)
        {
            _bossFrameCanvasGroup.alpha = 0;
            _sliderCanvasGroup.alpha = 0;
            _bossFrameImage.gameObject.SetActive(false);
            _healthSlider.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("UI要素が設定されていない！");
        }
    }

    /// <summary>
    /// キャンバスグループのアルファ値をフェードインする非同期メソッド
    /// </summary>
    private async UniTask FadeInCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration, CancellationToken cancellationToken)
    {
        float elapsedTime = 0f;
        canvasGroup.alpha = from;

        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        canvasGroup.alpha = to;
    }

    /// <summary>
    /// キャンバスグループのアルファ値をフェードアウトする非同期メソッド
    /// </summary>
    private async UniTask FadeOutCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration, CancellationToken cancellationToken)
    {
        float elapsedTime = 0f;
        canvasGroup.alpha = from;

        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        canvasGroup.alpha = to;
    }
}
