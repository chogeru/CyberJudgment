using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.UI;
using AbubuResouse.Log;
using Zenject;
using SQLite4Unity3d;
using System.Collections.Generic;

/// <summary>
/// UIのプレゼンタークラス。UIの初期化と更新を管理するシングルトン
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
    /// オブジェクトの初期化時に呼び出される
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetupDatabase();
        SetupSubscriptions();
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
        int index = int.Parse(uiObject);
        _view.SetSettingVisibility(false);
        _view.SetLinkedUIVisibility(index, true);
        _model.IsCursorVisible.Value = true;

        // SEを再生
        SEManager.Instance?.PlaySound("OpenLinkedUISE", 1.0f);
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
    }
}
