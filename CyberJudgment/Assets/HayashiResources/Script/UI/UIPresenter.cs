using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.Log;
using System.Linq;

/// <summary>
/// UI�̃v���[���^�[�N���X
/// </summary>
public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
{
    private UIModel _model;
    private IUIView _view;
    private UIRepository _repository;

    [SerializeField]
    private GameObject _menuUI;
    [SerializeField]
    private GameObject _settingUI;
    [SerializeField]
    private GameObject[] _linkedUIObjects;

    public bool IsMenuOpen => _model.IsMenuOpen.Value;
    public string CurrentUIObject => _model.CurrentUIObject.Value;

    /// <summary>
    /// �I�u�W�F�N�g�̏��������ɌĂяo�����
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _model = new UIModel();
        _view = new UIView();

        if (_view == null)
        {
            DebugUtility.LogError("UIView�̎擾�Ɏ��s�I");
            return;
        }

        SetupDatabase().Forget();

        //�f�o�b�N�p
        LogAllButtonLinks();
    }
    private void LogAllButtonLinks()
    {
        var allLinks = _repository.GetAllButtonLinks();
        foreach (var link in allLinks)
        {
            //�F�X�\��
            DebugUtility.Log($"Id: {link.Id}, ButtonName: {link.ButtonName}, LinkedUIObjectName: {link.LinkedUIObjectName}");
        }
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
    private async UniTask InitializeUI()
    {
        _model.IsCursorVisible.Value = false;
        // ���j���[���J������Ԃɐݒ�
        _view.SetMenuVisibility(_menuUI, true);
        _view.SetSettingVisibility(_settingUI, false);
        // �Q�[���J�n���Ƀ��j���[UI��\�����A���̑���UI���A�N�e�B�u�ɂ���
        foreach (var linkedUI in _linkedUIObjects)
        {
            linkedUI.SetActive(false);
        }

        await UniTask.CompletedTask;
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

        _model.CurrentUIObject
            .Where(uiObject => uiObject != "")
            .Subscribe(uiObject => OpenLinkedUI(uiObject))
            .AddTo(this);

        _model.CurrentUIObject
            .Where(uiObject => uiObject == "")
            .Subscribe(_ => CloseLinkedUI())
            .AddTo(this);

        _model.IsCursorVisible
            .Subscribe(isVisible => _view.SetCursorVisibility(isVisible))
            .AddTo(this);

        // UI��������
        InitializeUI().Forget();
    }

    /// <summary>
    /// �w�肳�ꂽ�A�{�^���ɕR�Â���ꂽUI�I�u�W�F�N�g���J��
    /// </summary>
    private void OpenLinkedUI(string uiObject)
    {
        var linkedObject = _linkedUIObjects.FirstOrDefault(obj => obj.name == uiObject);
        if (linkedObject != null)
        {
            _view.SetSettingVisibility(_settingUI, false);
            linkedObject.SetActive(true);
            _model.IsCursorVisible.Value = true;

            SEManager.Instance?.PlaySound("OpenLinkedUISE", 1.0f);
        }
        else
        {
            DebugUtility.LogError($"�w�肳�ꂽUI�I�u�W�F�N�g���݂��Ȃ�!? {uiObject}");
        }
    }

    /// <summary>
    /// ���ׂẴ{�^���ɕR�Â���ꂽUI�I�u�W�F�N�g�����
    /// </summary>
    private void CloseLinkedUI()
    {
        for (int i = 0; i < _linkedUIObjects.Length; i++)
        {
            _view.SetLinkedUIVisibility(_linkedUIObjects, i, false);
        }
        _view.SetSettingVisibility(_settingUI, true);
        _model.IsCursorVisible.Value = true;
        SEManager.Instance?.PlaySound("CloseLinkedUISE", 1.0f);
    }

    public void ShowLinkedUI(string buttonName)
    {
        if (_repository == null)
        {
            DebugUtility.LogError("���|�W�g����null");
            return;
        }

        var buttonLink = _repository.GetButtonLinkByName(buttonName);
        if (buttonLink != null)
        {
            _model.CurrentUIObject.Value = buttonLink.LinkedUIObjectName;
        }
        else
        {
            DebugUtility.LogError($"�w�肳�ꂽ�{�^���ɕR�Â���ꂽUI���݂��Ȃ�: {buttonName}");
        }
    }

    public void ShowSettingUI()
    {
        _model.CurrentUIObject.Value = "";
    }

    /// <summary>
    /// ���j���[UI���J��
    /// </summary>
    private void OpenMenuUI()
    {
        SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
        _model.IsCursorVisible.Value = true;
        _view.SetMenuVisibility(_menuUI, false);
        _view.SetSettingVisibility(_settingUI, true);
    }

    /// <summary>
    /// ���j���[UI�����
    /// </summary>
    private void CloseMenuUI()
    {
        SEManager.Instance.PlaySound("MenuCloseSE", 1.0f);
        _model.IsCursorVisible.Value = false;
        _view.SetMenuVisibility(_menuUI, true);
        _view.SetSettingVisibility(_settingUI, false);
    }

    /// <summary>
    /// ���j���[UI�̕\��/��\����؂�ւ���
    /// </summary>
    public void ToggleMenuUI()
    {
        _model.IsMenuOpen.Value = !_model.IsMenuOpen.Value;

        if (!_model.IsMenuOpen.Value)
        {
            _model.CurrentUIObject.Value = "";
            _view.SetSettingVisibility(_settingUI, false);
            _model.IsCursorVisible.Value = false;
            foreach (var linkedUI in _linkedUIObjects)
            {
                linkedUI?.SetActive(false);
            }
        }
    }

    /// <summary>
    /// �f�[�^�x�[�X�ڑ��̏�����
    /// </summary>
    private async UniTask SetupDatabase()
    {
        var databasePath = $"{Application.persistentDataPath}/ui_database.db";
        _repository = new UIRepository(databasePath);
        await UniTask.SwitchToMainThread();
    }
}
