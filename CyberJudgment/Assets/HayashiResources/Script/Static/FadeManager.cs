using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// フェードイン・フェードアウトを管理するクラス
    /// </summary>
    public class FadeManager : SingletonMonoBehaviour<FadeManager>
    {
        [SerializeField, Header("フェード用の画像")]
        private Image m_FadeImage;

        /// <summary>
        /// 画面をフェードイン
        /// </summary>
        /// <param name="fadeDuration">フェードの長さ（秒）</param>
        public void FadeIn(float fadeDuration)
        {
            //カラーの設定
            Color alfaColor = m_FadeImage.color;
            alfaColor.a = 0f;
            m_FadeImage.color = alfaColor;
            FadeImageAsync(fadeDuration, 1).Forget();
        }

        /// <summary>
        /// 画面をフェードアウト
        /// </summary>
        /// <param name="fadeDuration">フェードの長さ（秒）</param>
        public void FadeOut(float fadeDuration)
        {
            //カラーの設定
            Color alfaColor = m_FadeImage.color;
            alfaColor.a = 1f;
            m_FadeImage.color = alfaColor;
            FadeImageAsync(fadeDuration, 0).Forget();
        }

        /// <summary>
        /// フェード処理を行う非同期メソッド
        /// </summary>
        /// <param name="fadeDuration">フェードの長さ（秒）</param>
        /// <param name="targetAlpha">目標のアルファ値</param>
        /// <returns>UniTask</returns>
        private async UniTask FadeImageAsync(float fadeDuration, float targetAlpha)
        {
            Color currentColor = m_FadeImage.color;
            Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                m_FadeImage.color = Color.Lerp(currentColor, targetColor, elapsedTime / fadeDuration);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            m_FadeImage.color = targetColor;
        }
    }
}
