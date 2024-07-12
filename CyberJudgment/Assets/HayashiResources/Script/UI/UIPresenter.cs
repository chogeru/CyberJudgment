using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.UI;
using AbubuResouse.Log;
using Zenject;
using SQLite4Unity3d;
using System.Collections.Generic;

/// <summary>
/// UI�̃v���[���^�[�N���X�BUI�̏������ƍX�V���Ǘ�����V���O���g��
/// </summary>
public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
{
    private UIModel _model;
    private UIView _view;
    private UIRepository _repository;
    private SQLiteConnection _dbConnection;

    [Inject]
    public void Construct(UIRepository repository, UIView view, UIModel model)
    {
        _repository = repository;
        _view = view;
        _model = model;
    }

    public bool IsMenuOpen => _model.IsMenuOpen.Value;
    public string CurrentUIObject => _model.CurrentUIObject.Value;

    /// <summary>
    /// �I�u�W�F�N�g�̏��������ɌĂяo�����
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetupDatabase();
        SetupSubscriptions();
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
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
        // �Q�[���J�n���Ƀ��j���[UI��\�����A���̑���UI���A�N�e�B�u�ɂ���
        foreach (var linkedUI in _view.LinkedUIObjects)
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
    /// �w�肳�ꂽ�����N���ꂽUI�I�u�W�F�N�g���J��
    /// </summary>
    private void OpenLinkedUI(string uiObject)
    {
        int index = int.Parse(uiObject);
        _view.SetSettingVisibility(false);
        _view.SetLinkedUIVisibility(index, true);
        _model.IsCursorVisible.Value = true;

        // SE���Đ�
        SEManager.Instance?.PlaySound("OpenLinkedUISE", 1.0f);
    }

    /// <summary>
    /// ���ׂẴ����N���ꂽUI�I�u�W�F�N�g�����
    /// </summary>
    private void CloseLinkedUI()
    {
        for (int i = 0; i < _view.LinkedUIObjects.Length; i++)
        {
            _view.SetLinkedUIVisibility(i, false);
        }
        _view.SetSettingVisibility(true);
        _model.IsCursorVisible.Value = true;

        // SE���Đ�
        SEManager.Instance?.PlaySound("CloseLinkedUISE", 1.0f);
    }

    public void ShowLinkedUI(string buttonName)
    {
        var buttonLink = _repository.GetButtonLinkByName(buttonName);
        if (buttonLink != null)
        {
            _model.CurrentUIObject.Value = buttonLink.LinkedUIObjectName;
        }
        else
        {
            Debug.LogError("No linked UI found for button: " + buttonName);
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
        if (SEManager.Instance == null || _view == null)
        {
            DebugUtility.LogError("SEManager��_view��null");
            return;
        }
        SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
        _model.IsCursorVisible.Value = true;
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
        _model.IsCursorVisible.Value = false;
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
    }

    /// <summary>
    /// ���j���[UI�̕\��/��\����؂�ւ���
    /// </summary>
    public void ToggleMenuUI()
    {
        _model.IsMenuOpen.Value = !_model.IsMenuOpen.Value;

        // ESC�L�[�����������{�I�Ƀ��j���[�ȊO���ׂĂ�UI���\���ɂ���
        if (!_model.IsMenuOpen.Value)
        {
            _model.CurrentUIObject.Value = "";
            _view.SetSettingVisibility(false);
            _model.IsCursorVisible.Value = false;
            for (int i = 0; i < _view.LinkedUIObjects.Length; i++)
            {
                _view.SetLinkedUIVisibility(i, false);
            }
        }
    }

    /// <summary>
    /// �f�[�^�x�[�X�ڑ��̏�����
    /// </summary>
    private void SetupDatabase()
    {
        var databasePath = $"{Application.persistentDataPath}/ui_database.db";
        _repository = new UIRepository(databasePath);
    }
}
