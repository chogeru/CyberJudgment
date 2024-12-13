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
        /// ���C���J�����̃L���b�V����ێ�
        /// </summary>
        private void CacheMainCamera()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// �_���[�W�e�L�X�g��\��
        /// </summary>
        /// <param name="value">�\�����鐔�l</param>
        /// <param name="position">�\���ʒu</param>
        /// <param name="duration">�\������</param>
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
        /// �v���n�u���L�����m�F
        /// </summary>
        /// <returns>�L���Ȃ�true</returns>
        private bool IsPrefabValid()
        {
            if (damageTextPrefab == null)
            {
                Debug.LogError("DamageTextPrefab���A�^�b�`����Ă��Ȃ��I");
                return false;
            }
            return true;
        }

        /// <summary>
        /// �_���[�W�e�L�X�g�C���X�^���X���v�[������擾
        /// </summary>
        /// <param name="position">�擾����ʒu</param>
        /// <returns>�C���X�^���X</returns>
        private GameObject RentDamageTextInstance(Vector3 position)
        {
            GameObject instance = SharedGameObjectPool.Rent(damageTextPrefab, position, Quaternion.identity);
            if (instance == null)
            {
                Debug.LogError($"�_���[�W�e�L�X�g�̐����Ɏ��s�I:���O={damageTextPrefab.name}");
            }
            return instance;
        }

        /// <summary>
        /// �e�L�X�g�R���|�[�l���g��������
        /// </summary>
        /// <param name="damageTextInstance">�_���[�W�e�L�X�g�̃C���X�^���X</param>
        /// <param name="value">�\�����鐔�l</param>
        /// <returns>�ݒ肳�ꂽTMP_Text</returns>
        private TMP_Text SetupTextComponent(GameObject damageTextInstance, int value)
        {
            TMP_Text textComponent = damageTextInstance.GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError($"TMP_Text��{damageTextPrefab.name}�ɑ��݂��Ȃ�");
                SharedGameObjectPool.Return(damageTextInstance);
                return null;
            }

            textComponent.alpha = 1;
            textComponent.text = value.ToString();
            return textComponent;
        }

        /// <summary>
        /// �_���[�W�e�L�X�g�ɃA�j���[�V������K�p
        /// </summary>
        private void AnimateDamageText(GameObject damageTextInstance, TMP_Text textComponent, float duration)
        {
            ApplyBounceAnimation(damageTextInstance.transform, duration);
            ApplyFadeAnimation(textComponent, duration);
        }

        /// <summary>
        /// �o�E���h�A�j���[�V������K�p
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
        /// �t�F�[�h�A�E�g�A�j���[�V������K�p
        /// </summary>
        private void ApplyFadeAnimation(TMP_Text textComponent, float duration)
        {
            float fadeDuration = duration * 0.2f;
            textComponent.DOFade(0, fadeDuration)
                .SetDelay(duration - fadeDuration)
                .SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// �e�L�X�g���J�����������悤�ɐݒ�
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
        /// �_���[�W�e�L�X�g���w�肳�ꂽ���Ԍ�Ƀv�[���ɕԋp
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
