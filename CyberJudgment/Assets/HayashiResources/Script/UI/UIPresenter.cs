using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.UI;
using AbubuResouse.Log;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI�̃v���[���^�[�N���X�BUI�̏������ƍX�V���Ǘ�����V���O���g��
/// </summary>
public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
{
    private UIModel _model;
    private UIView _view;
    private UIRepository _repository;

    public bool IsMenuOpen => _model.IsMenuOpen.Value;
    public string CurrentUIObject => _model.CurrentUIObject.Value;

    /// <summary>
    /// �I�u�W�F�N�g�̏��������ɌĂяo�����
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _model = new UIModel();
        _view = FindObjectOfType<UIView>();
        if (_model == null)
        {
            Debug.LogError("UIModel�̏������Ɏ��s���܂����B");
            return;
        }
        if (_view == null)
        {
            Debug.LogError("UIView�̎擾�Ɏ��s���܂����B");
            return;
        }
        else
        {
            Debug.Log("UIView������Ɏ擾����܂����B");
        }
        SetupDatabase();
        LogAllButtonLinks();
    }
    private void LogAllButtonLinks()
    {
        var allLinks = _repository.GetAllButtonLinks();
        foreach (var link in allLinks)
        {
            Debug.Log($"Id: {link.Id}, ButtonName: {link.ButtonName}, LinkedUIObjectName: {link.LinkedUIObjectName}");
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
        Debug.Log($"OpenLinkedUI called with uiObject: {uiObject}");
        var linkedObject = _view.LinkedUIObjects.FirstOrDefault(obj => obj.name == uiObject);
        if (linkedObject != null)
        {
            _view.SetSettingVisibility(false);
            linkedObject.SetActive(true);
            _model.IsCursorVisible.Value = true;

            SEManager.Instance?.PlaySound("OpenLinkedUISE", 1.0f);
        }
        else
        {
            Debug.LogError($"Linked UI object not found: {uiObject}");
        }
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
        if (_repository == null)
        {
            Debug.LogError("���|�W�g����null�ł��BSetupDatabase() ���Ăяo����Ă��邱�Ƃ��m�F���Ă��������B");
            return;
        }

        var buttonLink = _repository.GetButtonLinkByName(buttonName);
        if (buttonLink != null)
        {
            Debug.Log($"ButtonLink��������܂���: {buttonLink.LinkedUIObjectName}");
            _model.CurrentUIObject.Value = buttonLink.LinkedUIObjectName;
            if (_model.CurrentUIObject.Value == null)
            {
                Debug.LogError("CurrentUIObject��null�ł��B");
            }
            else
            {
                Debug.Log($"CurrentUIObject���ݒ肳��܂���: {_model.CurrentUIObject.Value}");
            }
        }
        else
        {
            Debug.LogError($"�w�肳�ꂽ�{�^���ɕR�Â���ꂽUI��������܂���: {buttonName}");
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
        if (_repository == null)
        {
            Debug.LogError("UIRepository�̃C���X�^���X�쐬�Ɏ��s���܂����B");
        }
        else
        {
            Debug.Log("UIRepository�̃C���X�^���X�쐬�ɐ������܂����B");
        }
    }
}
