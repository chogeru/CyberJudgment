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
        [SerializeField, Header("�{�X�̖��O��\������Text")]
        private TMP_Text _bossNameText;

        [SerializeField, Header("�ǉ��̃{�X�̖��O��\������Text")]
        private TMP_Text _sliderBossNameText;

        [SerializeField, Header("�{�X�̎c��̗͂�\������X���C�_�[")]
        private Slider _healthSlider;

        [SerializeField, Header("�{�X�̃t���[��Image")]
        private Image _bossFrameImage;

        [SerializeField, Header("�{�X�t���[����CanvasGroup")]
        private CanvasGroup _bossFrameCanvasGroup;

        [SerializeField, Header("�X���C�_�[��CanvasGroup")]
        private CanvasGroup _sliderCanvasGroup;

        [SerializeField, Header("�A�j���[�V�����ɂ����鎞�� (�b)")]
        private float _fadeDuration = 3.0f;

        private CancellationTokenSource _cts; // �L�����Z���p�̃g�[�N��

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
        /// �{�X�̖��O��ݒ肷��
        /// </summary>
        /// <param name="bossName">�\������{�X�̖��O</param>
        public void SetBossName(string bossName)
        {
            if (_bossNameText != null && _sliderBossNameText != null)
            {
                _bossNameText.text = bossName;
                _sliderBossNameText.text = bossName;
            }
            else
            {
                Debug.LogError("�{�X�̖��O��Text���ݒ肳��Ă��Ȃ��I");
            }
        }

        /// <summary>
        /// �{�X�̗̑͂�ݒ肷��
        /// </summary>
        /// <param name="currentHealth">���݂̗̑�</param>
        /// <param name="maxHealth">�ő�̗�</param>
        public async UniTask SetBossHealth(float currentHealth, float maxHealth, float duration = 0.5f)
        {
            if (_healthSlider == null)
            {
                Debug.LogError("�̗�Slider���ݒ肳��Ă��Ȃ��I");
                return;
            }

            _healthSlider.maxValue = maxHealth;

            float startValue = _healthSlider.value;
            float endValue = currentHealth;
            float elapsedTime = 0f;
            var token = _cts.Token;

            while (elapsedTime < duration)
            {
                // �L�����Z������Ă���Β��f
                if (token.IsCancellationRequested) return;

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                _healthSlider.value = Mathf.Lerp(startValue, endValue, t);

                // ���̃t���[���܂ő҂�
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _healthSlider.value = endValue;
        }

        /// <summary>
        /// �{�XUI���J�n����
        /// </summary>
        [ContextMenu("StartBossUI")]
        public async UniTask StartBossUI(float initialHealth = 0, float maxHealth = 0)
        {
            if (_bossFrameImage != null && _bossFrameCanvasGroup != null && _sliderCanvasGroup != null)
            {
                // �X���C�_�[�̏�����
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
                Debug.LogError("UI�v�f���ݒ肳��Ă��Ȃ��I");
            }
        }


        /// <summary>
        /// �{�XUI�����Z�b�g����
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
                Debug.LogError("UI�v�f���ݒ肳��Ă��Ȃ��I");
            }
        }

        /// <summary>
        /// �L�����o�X�O���[�v�̃A���t�@�l���t�F�[�h�C������񓯊����\�b�h
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
        /// �L�����o�X�O���[�v�̃A���t�@�l���t�F�[�h�A�E�g����񓯊����\�b�h
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