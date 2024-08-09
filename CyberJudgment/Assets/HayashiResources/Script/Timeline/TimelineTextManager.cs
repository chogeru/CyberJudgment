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
    /// �e�L�X�g��\�����鏈��
    /// </summary>
    /// <param name="characterName">�L�����N�^�[��</param>
    /// <param name="textToShow">�\������e�L�X�g</param>
    public void ShowText(string characterName, string textToShow)
    {
        if (_textWindowUI == null || _textMeshPro == null)
        {
            Debug.LogError("Text�E�B���h�E���Ȃ�");
            return;
        }

        isTextEnd = false;
        _textWindowUI.SetActive(true);
        _textMeshPro.gameObject.SetActive(true);
        _nameMeshPro.gameObject.SetActive(true);
        StopManager.Instance.IsStopped = true;  // �e�L�X�g�\�����͑��̓�����~����

        _cts?.Cancel();  // �O��̕\�����L�����Z��
        _cts = new CancellationTokenSource();
        _nameMeshPro.text = characterName;  // �L�����N�^�[����ݒ�
        TypeText(textToShow, _cts.Token).Forget();  // �e�L�X�g��񓯊��Ń^�C�v�\��
    }

    /// <summary>
    /// �e�L�X�g���^�C�v�\�����鏈���i�񓯊��j
    /// </summary>
    /// <param name="textToShow">�\������e�L�X�g</param>
    /// <param name="token">�L�����Z���g�[�N��</param>
    /// <returns></returns>
    private async UniTaskVoid TypeText(string textToShow, CancellationToken token)
    {
        _textMeshPro.text = "";
        foreach (char letter in textToShow.ToCharArray())
        {
            _textMeshPro.text += letter;
            await UniTask.Delay(1, cancellationToken: token);  // �^�C�v���x�𒲐�
            if (token.IsCancellationRequested)
            {
                return;  // �L�����Z�����ꂽ�ꍇ�͏I��
            }
        }
    }

    /// <summary>
    /// �e�L�X�g���\���ɂ��鏈��
    /// </summary>
    public void HideText()
    {
        if (_textWindowUI == null)
        {
            return;
        }

        isTextEnd = true;
        StopManager.Instance.IsStopped = false;  // ���̓�����ĊJ����
        _textWindowUI.SetActive(false);
        _textMeshPro.gameObject.SetActive(false);
        _nameMeshPro.gameObject.SetActive(false);
    }
}
