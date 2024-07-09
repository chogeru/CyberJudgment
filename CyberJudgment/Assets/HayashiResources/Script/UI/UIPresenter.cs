using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.UI;

/// <summary>
/// UIのプレゼンタークラス。UIの初期化と更新を管理するシングルトン
/// </summary>
public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
{
    public static UIPresenter Instance;

    private UIModel _model;
    private UIView _view;

    /// <summary>
    /// オブジェクトの初期化時に呼び出される
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        _model = new UIModel();
        _view = GetComponent<UIView>();

        InitializeUI();
        SetupSubscriptions();
    }

    /// <summary>
    /// 初期化後に呼び出される
    /// 毎フレームEscキーの入力をチェックし、メニューの表示/非表示を切り替える
    /// </summary>
    private void Start()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Escape))
            .Subscribe(_ => ToggleMenuUI())
            .AddTo(this);
    }

    /// <summary>
    /// UIを初期化
    /// メニュー画面を表示して、設定画面を非表示にしている
    /// </summary>
    private void InitializeUI()
    {
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
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
    }

    /// <summary>
    /// メニューUIを開く
    /// </summary>
    private void OpenMenuUI()
    {
        SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
        _view.SetCursorVisibility(true);
        _view.SetMenuVisibility(false);
        _view.SetSettingVisibility(true);
    }

    /// <summary>
    /// メニューUIを閉じる
    /// </summary>
    private void CloseMenuUI()
    {
        SEManager.Instance.PlaySound("MenuCloseSE", 1.0f);
        _view.SetCursorVisibility(false);
        _view.SetMenuVisibility(true);
        _view.SetSettingVisibility(false);
    }

    /// <summary>
    /// メニューUIの表示/非表示を切り替える
    /// </summary>
    public void ToggleMenuUI()
    {
        _model.IsMenuOpen.Value = !_model.IsMenuOpen.Value;
    }
}
