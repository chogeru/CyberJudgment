using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using AbubuResouse.Singleton;
using AbubuResouse.Log;
public class MapNameStationManager : MonoBehaviour
{
    [Header("UI�֘A")]
    [SerializeField] private CanvasGroup mapNameCanvasGroup;
    [SerializeField] private TextMeshProUGUI mapNameText;

    [Header("�T�E���h�֘A")]
    [SerializeField] private string uiActiveOnSE = "ShowUI_SE";
    [SerializeField, Range(0f, 1f)] private float volume = 1.0f;

    [Header("�\���֘A")]
    [SerializeField, Tooltip("�t�F�[�h�C���E�A�E�g�̎���(�b)")]
    private float fadeDuration = 0.5f;
    [SerializeField, Tooltip("�\�����ێ����鎞��(�b)")]
    private float displayTime = 3.0f;
    [SerializeField, Tooltip("Start�ŕ\�����������}�b�v���B��̏ꍇ�͕\���Ȃ�")]
    private string initialMapName = "Default Map Name";

    private bool hasCollider = false;
    private bool hasShownThisSession = false;  // ��x�\�������烊�Z�b�g�܂ŕ\�����Ȃ����߂̃t���O

    private async void Start()
    {
        // �I�u�W�F�N�g��Collider�����邩�ǂ����`�F�b�N
        hasCollider = TryGetComponent<Collider>(out var col);

        if (!hasCollider)
        {
            await UniTask.Delay(2000);
            // �R���C�_�[���������Start�ŕ\��
            if (!string.IsNullOrEmpty(initialMapName))
            {
                ShowMapNameAsync(initialMapName).Forget();
                hasShownThisSession = true;
            }
        }
        else
        {
            DebugUtility.Log("����Z�b�g");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollider && !hasShownThisSession && other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(initialMapName))
            {
                ShowMapNameAsync(initialMapName).Forget();
                hasShownThisSession = true;
            }
        }
    }

    /// <summary>
    /// �}�b�v����\��������J���\�b�h
    /// �K�v�ł���ΊO������C�ӂ̕\�����Ԃ�t�F�[�h���Ԃ��w�肵�ČĂяo����悤�ɃI�[�o�[���[�h
    /// </summary>
    public UniTaskVoid ShowMapNameAsync(string mapName)
    {
        ShowMapNameAsync(mapName, displayTime, fadeDuration).Forget();
        return default;
    }

    public async UniTask ShowMapNameAsync(string mapName, float displayDuration, float fadeTime)
    {
        if (!ValidateUIComponents())
        {
            Debug.LogWarning("CanvasGroup�܂���TextMeshProUGUI���ݒ肳��Ă��܂���B�����𒆒f���܂��B");
            return;
        }

        SetupUIForDisplay(mapName);

        await FadeUI(mapNameCanvasGroup, 0f, 1f, fadeTime);

        PlayUIActivationSound();

        await UniTask.Delay((int)(displayDuration * 1000));

        await FadeUI(mapNameCanvasGroup, 1f, 0f, fadeTime);

        mapNameCanvasGroup.gameObject.SetActive(false);
    }

    /// <summary>
    /// UI�R���|�[�l���g���������ݒ肳��Ă��邩�m�F
    /// </summary>
    private bool ValidateUIComponents()
    {
        return mapNameCanvasGroup != null && mapNameText != null;
    }

    /// <summary>
    /// UI�\���O�̏���������
    /// </summary>
    private void SetupUIForDisplay(string mapName)
    {
        mapNameCanvasGroup.gameObject.SetActive(true);
        mapNameCanvasGroup.alpha = 0f;
        mapNameText.text = mapName;
    }

    /// <summary>
    /// CanvasGroup���t�F�[�h�����鋤�ʏ���
    /// </summary>
    private async UniTask FadeUI(CanvasGroup targetCanvasGroup, float fromAlpha, float toAlpha, float duration)
    {
        if (targetCanvasGroup == null)
        {
            Debug.LogWarning("�t�F�[�h�Ώۂ�CanvasGroup�����ݒ�ł��B");
            return;
        }

        targetCanvasGroup.alpha = fromAlpha;
        await targetCanvasGroup
            .DOFade(toAlpha, duration)
            .SetUpdate(true)
            .AsyncWaitForCompletion()
            .AsUniTask();
    }

    /// <summary>
    /// UI�\�����ɍĐ�����T�E���h���Đ����鋤�ʏ���
    /// </summary>
    private void PlayUIActivationSound()
    {
        if (SEManager.Instance != null && !string.IsNullOrEmpty(uiActiveOnSE))
        {
            SEManager.Instance.PlaySound(uiActiveOnSE, volume);
        }
    }
}
