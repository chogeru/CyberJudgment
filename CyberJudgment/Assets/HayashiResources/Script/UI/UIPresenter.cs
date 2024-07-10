using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.UI;
using AbubuResouse.Log;
/// <summary>
/// UI�̃v���[���^�[�N���X�BUI�̏������ƍX�V���Ǘ�����V���O���g��
/// </summary>
public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
{
    public static UIPresenter Instance;

    private UIModel _model;
    private UIView _view;

    /// <summary>
    /// �I�u�W�F�N�g�̏��������ɌĂяo�����
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        _model = new UIModel();
        _view = GetComponent<UIView>();

        if (_view == null)
        {
            DebugUtility.LogError("UIView ��������Ȃ�");
        }
        InitializeUI();
    }

    /// <summary>
    /// ��������ɌĂяo�����
    /// ���t���[��Esc�L�[�̓��͂��`�F�b�N���A���j���[�̕\��/��\����؂�ւ���
    /// </summary>
    private void Start()
    {
        SetupSubscriptions();
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Escape))
            .Subscribe(_ => ToggleMenuUI())
            .AddTo(this);
    }

    /// <summary>
    /// UI��������
    /// ���j���[��ʂ�\�����āA�ݒ��ʂ��\���ɂ��Ă���
    /// </summary>
    private void InitializeUI()
    {
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
    }

    /// <summary>
    /// ���f���̏�ԂɊ�Â���UI�̍X�V��ݒ肷��
    /// </summary>
    private void SetupSubscriptions()
    {
        _model.IsMenuOpen
            .Where(isOpen => isOpen)
            .Subscribe(_ => OpenMenuUI())
            .AddTo(this);

        _model.IsMenuOpen
            .Where(isOpen => !isOpen)
            .Subscribe(_ => CloseMenuUI())
            .AddTo(this);
    }

    /// <summary>
    /// ���j���[UI���J��
    /// </summary>
    private void OpenMenuUI()
    {

        if (SEManager.Instance == null || _view == null)
        {
            DebugUtility.LogError("SEManager��_view��null");
            return;
        }
        SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
        _view.SetCursorVisibility(true);
        _view.SetMenuVisibility(false);
        _view.SetSettingVisibility(true);
    }

    /// <summary>
    /// ���j���[UI�����
    /// </summary>
    private void CloseMenuUI()
    {
        if (SEManager.Instance == null || _view == null)
        {
            DebugUtility.LogError("SEManager��_view��null");
            return;
        }
        SEManager.Instance.PlaySound("MenuCloseSE", 1.0f);
        _view.SetCursorVisibility(false);
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
    }

    /// <summary>
    /// ���j���[UI�̕\��/��\����؂�ւ���
    /// </summary>
    public void ToggleMenuUI()
    {
        _model.IsMenuOpen.Value = !_model.IsMenuOpen.Value;
    }
}
