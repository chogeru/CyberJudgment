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
    private UIView _view;
    private UIRepository _repository;

    public bool IsMenuOpen => _model.IsMenuOpen.Value;
    public string CurrentUIObject => _model.CurrentUIObject.Value;

    /// <summary>
    /// オブジェクトの初期化時に呼び出される
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _model = new UIModel();
        _view = FindObjectOfType<UIView>();
        if (_model == null)
        {
            Debug.LogError("UIModelの初期化に失敗しました。");
            return;
        }
        if (_view == null)
        {
            Debug.LogError("UIViewの取得に失敗しました。");
            return;
        }
        else
        {
            Debug.Log("UIViewが正常に取得されました。");
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
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
        // ゲーム開始時にメニューUIを表示し、その他のUIを非アクティブにする
        foreach (var linkedUI in _view.LinkedUIObjects)
        {
            linkedUI.SetActive(false);
        }

        await UniTask.CompletedTask;
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
    /// すべてのリンクされたUIオブジェクトを閉じる
    /// </summary>
    private void CloseLinkedUI()
    {
        for (int i = 0; i < _view.LinkedUIObjects.Length; i++)
        {
            _view.SetLinkedUIVisibility(i, false);
        }
        _view.SetSettingVisibility(true);
        _model.IsCursorVisible.Value = true;

        // SEを再生
        SEManager.Instance?.PlaySound("CloseLinkedUISE", 1.0f);
    }

    public void ShowLinkedUI(string buttonName)
    {
        if (_repository == null)
        {
            Debug.LogError("リポジトリがnullです。SetupDatabase() が呼び出されていることを確認してください。");
            return;
        }

        var buttonLink = _repository.GetButtonLinkByName(buttonName);
        if (buttonLink != null)
        {
            Debug.Log($"ButtonLinkが見つかりました: {buttonLink.LinkedUIObjectName}");
            _model.CurrentUIObject.Value = buttonLink.LinkedUIObjectName;
            if (_model.CurrentUIObject.Value == null)
            {
                Debug.LogError("CurrentUIObjectがnullです。");
            }
            else
            {
                Debug.Log($"CurrentUIObjectが設定されました: {_model.CurrentUIObject.Value}");
            }
        }
        else
        {
            Debug.LogError($"指定されたボタンに紐づけられたUIが見つかりません: {buttonName}");
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
        if (SEManager.Instance == null || _view == null)
        {
            DebugUtility.LogError("SEManagerか_viewがnull");
            return;
        }
        SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
        _model.IsCursorVisible.Value = true;
        _view.SetMenuVisibility(false);
        _view.SetSettingVisibility(true);
    }

    /// <summary>
    /// メニューUIを閉じる
    /// </summary>
    private void CloseMenuUI()
    {
        if (SEManager.Instance == null || _view == null)
        {
            DebugUtility.LogError("SEManagerか_viewがnull");
            return;
        }
        SEManager.Instance.PlaySound("MenuCloseSE", 1.0f);
        _model.IsCursorVisible.Value = false;
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
    }

    /// <summary>
    /// メニューUIの表示/非表示を切り替える
    /// </summary>
    public void ToggleMenuUI()
    {
        _model.IsMenuOpen.Value = !_model.IsMenuOpen.Value;

        // ESCキーを押したら基本的にメニュー以外すべてのUIを非表示にする
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
    /// データベース接続の初期化
    /// </summary>
    private void SetupDatabase()
    {
        var databasePath = $"{Application.persistentDataPath}/ui_database.db";
        _repository = new UIRepository(databasePath);
        if (_repository == null)
        {
            Debug.LogError("UIRepositoryのインスタンス作成に失敗しました。");
        }
        else
        {
            Debug.Log("UIRepositoryのインスタンス作成に成功しました。");
        }
    }
}
