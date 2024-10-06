using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BossUIManager : MonoBehaviour
{
    [SerializeField, Header("�{�X�̖��O��\������Text")]
    private TMP_Text _bossNameText;

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
        if (_bossNameText != null)
        {
            _bossNameText.text = bossName;
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
    public void SetBossHealth(float currentHealth, float maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = currentHealth;
        }
        else
        {
            Debug.LogError("�̗�Slider���ݒ肳��Ă��Ȃ��I");
        }
    }

    /// <summary>
    /// �{�XUI���J�n����
    /// </summary>
    [ContextMenu("StartBossUI")]
    public async UniTask StartBossUI()
    {
        if (_bossFrameImage != null && _bossFrameCanvasGroup != null && _sliderCanvasGroup != null)
        {
            _bossFrameImage.gameObject.SetActive(true);
            _healthSlider.gameObject.SetActive(true); // �X���C�_�[�͍ŏ���A�N�e�B�u

            // �{�X�t���[���̃t�F�[�h�C��
            await FadeInCanvasGroup(_bossFrameCanvasGroup, 0f, 1f, _fadeDuration, _cts.Token);

            // 3�b�ҋ@
            await UniTask.Delay(3000, cancellationToken: _cts.Token);

            // �{�X�t���[���̃t�F�[�h�A�E�g�ƃX���C�_�[�̃t�F�[�h�C���𓯎��ɍs��
            await UniTask.WhenAll(
                FadeOutCanvasGroup(_bossFrameCanvasGroup, 1f, 0f, _fadeDuration, _cts.Token),
                FadeInCanvasGroup(_sliderCanvasGroup, 0f, 1f, _fadeDuration, _cts.Token)
            );

            // �t�F�[�h�A�E�g������Ƀ{�X�t���[�����A�N�e�B�u��
            _bossFrameImage.gameObject.SetActive(false);
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
