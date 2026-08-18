using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using AbubuResouse.Singleton;

namespace AbubuResouse.Singleton
{
    public class BossUIManager : SingletonMonoBehaviour<BossUIManager>
    {
        [SerializeField, Header("ボスの名前を表示するText")]
        private TMP_Text _bossNameText;

        [SerializeField, Header("追加のボスの名前を表示するText")]
        private TMP_Text _sliderBossNameText;

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
            if (_bossNameText != null && _sliderBossNameText != null)
            {
                _bossNameText.text = bossName;
                _sliderBossNameText.text = bossName;
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
        public async UniTask SetBossHealth(float currentHealth, float maxHealth, float duration = 0.5f)
        {
            if (_healthSlider == null)
            {
                Debug.LogError("体力Sliderが設定されていない！");
                return;
            }

            _healthSlider.maxValue = maxHealth;

            float startValue = _healthSlider.value;
            float endValue = currentHealth;
            float elapsedTime = 0f;
            var token = _cts.Token;

            while (elapsedTime < duration)
            {
                // キャンセルされていれば中断
                if (token.IsCancellationRequested) return;

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                _healthSlider.value = Mathf.Lerp(startValue, endValue, t);

                // 次のフレームまで待つ
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _healthSlider.value = endValue;
        }

        /// <summary>
        /// ボスUIを開始する
        /// </summary>
        [ContextMenu("StartBossUI")]
        public async UniTask StartBossUI(float initialHealth = 0, float maxHealth = 0)
        {
            if (_bossFrameImage != null && _bossFrameCanvasGroup != null && _sliderCanvasGroup != null)
            {
                // スライダーの初期化
                if (_healthSlider != null)
                {
                    _healthSlider.maxValue = maxHealth;
                    _healthSlider.value = initialHealth;
                }

                _healthSlider.gameObject.SetActive(true);
                _sliderCanvasGroup.alpha = 0f;

                _bossFrameImage.gameObject.SetActive(true);

                await FadeInCanvasGroup(_bossFrameCanvasGroup, 0f, 1f, _fadeDuration, _cts.Token);

                await UniTask.Delay(3000, cancellationToken: _cts.Token);

                await FadeOutCanvasGroup(_bossFrameCanvasGroup, 1f, 0f, _fadeDuration, _cts.Token);
                _bossFrameImage.gameObject.SetActive(false);

                await FadeInCanvasGroup(_sliderCanvasGroup, 0f, 1f, _fadeDuration, _cts.Token);
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
}