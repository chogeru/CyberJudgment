using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AbubuResouse.Singleton;
using Cysharp.Threading.Tasks;
using uPools;
using System;
using DG.Tweening;

namespace AbubuResouse.Singleton
{
    public class DamageUIManager : SingletonMonoBehaviour<DamageUIManager>
    {
        [SerializeField]
        private GameObject damageTextPrefab;

        private Camera mainCamera;

        private void Start()
        {
            CacheMainCamera();
        }

        /// <summary>
        /// メインカメラのキャッシュを保持
        /// </summary>
        private void CacheMainCamera()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// ダメージテキストを表示
        /// </summary>
        /// <param name="value">表示する数値</param>
        /// <param name="position">表示位置</param>
        /// <param name="duration">表示時間</param>
        public void ShowDamageText(int value, Vector3 position, float duration = 1.5f)
        {
            if (!IsPrefabValid())
                return;

            GameObject damageTextInstance = RentDamageTextInstance(position);
            if (damageTextInstance == null)
                return;

            TMP_Text textComponent = SetupTextComponent(damageTextInstance, value);
            if (textComponent == null)
                return;

            AlignToCamera(damageTextInstance);
            AnimateDamageText(damageTextInstance, textComponent, duration);
            ScheduleReturnToPool(damageTextInstance, duration).Forget();
        }

        /// <summary>
        /// プレハブが有効か確認
        /// </summary>
        /// <returns>有効ならtrue</returns>
        private bool IsPrefabValid()
        {
            if (damageTextPrefab == null)
            {
                Debug.LogError("DamageTextPrefabがアタッチされていない！");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ダメージテキストインスタンスをプールから取得
        /// </summary>
        /// <param name="position">取得する位置</param>
        /// <returns>インスタンス</returns>
        private GameObject RentDamageTextInstance(Vector3 position)
        {
            GameObject instance = SharedGameObjectPool.Rent(damageTextPrefab, position, Quaternion.identity);
            if (instance == null)
            {
                Debug.LogError($"ダメージテキストの生成に失敗！:名前={damageTextPrefab.name}");
            }
            return instance;
        }

        /// <summary>
        /// テキストコンポーネントを初期化
        /// </summary>
        /// <param name="damageTextInstance">ダメージテキストのインスタンス</param>
        /// <param name="value">表示する数値</param>
        /// <returns>設定されたTMP_Text</returns>
        private TMP_Text SetupTextComponent(GameObject damageTextInstance, int value)
        {
            TMP_Text textComponent = damageTextInstance.GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError($"TMP_Textが{damageTextPrefab.name}に存在しない");
                SharedGameObjectPool.Return(damageTextInstance);
                return null;
            }

            textComponent.alpha = 1;
            textComponent.text = value.ToString();
            return textComponent;
        }

        /// <summary>
        /// ダメージテキストにアニメーションを適用
        /// </summary>
        private void AnimateDamageText(GameObject damageTextInstance, TMP_Text textComponent, float duration)
        {
            ApplyBounceAnimation(damageTextInstance.transform, duration);
            ApplyFadeAnimation(textComponent, duration);
        }

        /// <summary>
        /// バウンドアニメーションを適用
        /// </summary>
        private void ApplyBounceAnimation(Transform textTransform, float duration)
        {
            float bounceDuration = duration * 0.8f;
            int bounceCount = 4;
            float initialHeight = 0.6f;

            float elapsedTime = 0f;
            float bounceInterval = bounceDuration / bounceCount;
            Vector3 originalPosition = textTransform.localPosition;

            DOTween.To(() => elapsedTime, t => elapsedTime = t, bounceDuration, bounceDuration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    float timeInCurrentBounce = elapsedTime % bounceInterval;
                    float progress = timeInCurrentBounce / bounceInterval;
                    float currentHeight = initialHeight * (1 - (elapsedTime / bounceDuration));
                    float offsetY = Mathf.Sin(progress * Mathf.PI) * currentHeight;
                    textTransform.localPosition = originalPosition + new Vector3(0, offsetY, 0);
                });
        }

        /// <summary>
        /// フェードアウトアニメーションを適用
        /// </summary>
        private void ApplyFadeAnimation(TMP_Text textComponent, float duration)
        {
            float fadeDuration = duration * 0.2f;
            textComponent.DOFade(0, fadeDuration)
                .SetDelay(duration - fadeDuration)
                .SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// テキストがカメラを向くように設定
        /// </summary>
        private void AlignToCamera(GameObject damageTextInstance)
        {
            var billboard = damageTextInstance.GetComponent<Billboard>();
            if (billboard == null)
            {
                billboard = damageTextInstance.AddComponent<Billboard>();
            }
            billboard.SetTargetCamera(mainCamera);
        }

        /// <summary>
        /// ダメージテキストを指定された時間後にプールに返却
        /// </summary>
        private async UniTaskVoid ScheduleReturnToPool(GameObject damageText, float delay)
        {
            try
            {
                await UniTask.Delay((int)(delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
                SharedGameObjectPool.Return(damageText);
            }
            catch (OperationCanceledException) { }
        }
    }
}
