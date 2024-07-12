using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.UI;
using AbubuResouse.Log;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UIのプレゼンタークラス。UIの初期化と更新を管理するシングルトン
/// </summary>
public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
{
    private UIModel _model;
    private IUIView _view;
    private UIRepository _repository;

    [SerializeField]
    private GameObject menuUI;
    [SerializeField]
    private GameObject settingUI;
    [SerializeField]
    private GameObject[] linkedUIObjects;

    public bool IsMenuOpen => _model.IsMenuOpen.Value;
    public string CurrentUIObject => _model.CurrentUIObject.Value;

    /// <summary>
    /// オブジェクトの初期化時に呼び出される
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _model = new UIModel();
        _view = new UIView();

        if (_view == null)
        {
            DebugUtility.LogError("UIViewの取得に失敗");
            return;
        }

        SetupDatabase().Forget();

        //デバック用
        LogAllButtonLinks();
    }
    private void LogAllButtonLinks()
    {
        var allLinks = _repository.GetAllButtonLinks();
        foreach (var link in allLinks)
        {
            DebugUtility.Log($"Id: {link.Id}, ButtonName: {link.ButtonName}, LinkedUIObjectName: {link.LinkedUIObjectName}");
        }
    }

    /// <summary>
    /// 初期化後に呼び出される
    /// 毎フレームEscキーの入力をチェックし、メニューの表示/非表示を切り替える
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
    /// UIを初期化
    /// メニュー画面を表示して、設定画面を非表示にしている
    /// </summary>
    private async UniTask InitializeUI()
    {
        _model.IsCursorVisible.Value = false;
        // メニューを開いた状態に設定
        _view.SetMenuVisibility(menuUI, true);
        _view.SetSettingVisibility(settingUI, false);
        // ゲーム開始時にメニューUIを表示し、その他のUIを非アクティブにする
        foreach (var linkedUI in linkedUIObjects)
        {
            linkedUI.SetActive(false);
        }

        await UniTask.CompletedTask;
    }

    private void SetupView(IUIView view)
    {
        _view = view;
        if (_view == null)
        {
            DebugUtility.LogError("UIViewの取得に失敗");
        }
    }

    /// <summary>
    /// モデルの状態に基づいてUIの更新を設定する
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

        // UIを初期化
        InitializeUI().Forget();
    }

    /// <summary>
    /// 指定されたリンクされたUIオブジェクトを開く
    /// </summary>
    private void OpenLinkedUI(string uiObject)
    {
        Debug.Log($"OpenLinkedUI called with uiObject: {uiObject}");
        var linkedObject = linkedUIObjects.FirstOrDefault(obj => obj.name == uiObject);
        if (linkedObject != null)
        {
            _view.SetSettingVisibility(settingUI, false);
            linkedObject.SetActive(true);
            _model.IsCursorVisible.Value = true;

            SEManager.Instance?.PlaySound("OpenLinkedUISE", 1.0f);
        }
        else
        {
            DebugUtility.LogError($"指定されたUIオブジェクト存在しない {uiObject}");
        }
    }

    /// <summary>
    /// すべてのリンクされたUIオブジェクトを閉じる
    /// </summary>
    private void CloseLinkedUI()
    {
        for (int i = 0; i < linkedUIObjects.Length; i++)
        {
            _view.SetLinkedUIVisibility(linkedUIObjects, i, false);
        }
        _view.SetSettingVisibility(settingUI, true);
        _model.IsCursorVisible.Value = true;
        SEManager.Instance?.PlaySound("CloseLinkedUISE", 1.0f);
    }

    public void ShowLinkedUI(string buttonName)
    {
        if (_repository == null)
        {
            DebugUtility.LogError("リポジトリがnull");
            return;
        }

        var buttonLink = _repository.GetButtonLinkByName(buttonName);
        if (buttonLink != null)
        {
            _model.CurrentUIObject.Value = buttonLink.LinkedUIObjectName;
        }
        else
        {
            DebugUtility.LogError($"指定されたボタンに紐づけられたUI存在しない: {buttonName}");
        }
    }

    public void ShowSettingUI()
    {
        _model.CurrentUIObject.Value = "";
    }

    /// <summary>
    /// メニューUIを開く
    /// </summary>
    private void OpenMenuUI()
    {
        SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
        _model.IsCursorVisible.Value = true;
        _view.SetMenuVisibility(menuUI, false);
        _view.SetSettingVisibility(settingUI, true);
    }

    /// <summary>
    /// メニューUIを閉じる
    /// </summary>
    private void CloseMenuUI()
    {
        SEManager.Instance.PlaySound("MenuCloseSE", 1.0f);
        _model.IsCursorVisible.Value = false;
        _view.SetMenuVisibility(menuUI, true);
        _view.SetSettingVisibility(settingUI, false);
    }

    /// <summary>
    /// メニューUIの表示/非表示を切り替える
    /// </summary>
    public void ToggleMenuUI()
    {
        _model.IsMenuOpen.Value = !_model.IsMenuOpen.Value;

        if (!_model.IsMenuOpen.Value)
        {
            _model.CurrentUIObject.Value = "";
            _view.SetSettingVisibility(settingUI, false);
            _model.IsCursorVisible.Value = false;
            foreach (var linkedUI in linkedUIObjects)
            {
                linkedUI?.SetActive(false);
            }
        }
    }

    /// <summary>
    /// データベース接続の初期化
    /// </summary>
    private async UniTask SetupDatabase()
    {
        var databasePath = $"{Application.persistentDataPath}/ui_database.db";
        _repository = new UIRepository(databasePath);
        await UniTask.SwitchToMainThread();
    }
}
