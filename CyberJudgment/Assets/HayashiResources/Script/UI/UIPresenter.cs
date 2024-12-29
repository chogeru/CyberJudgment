using R3;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AbubuResouse.Log;
using System.Linq;
using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AbubuResouse.MVP.Model;
using AbubuResouse.MVP.Repository;
using AbubuResouse.MVP.View;
using AbubuResouse.Singleton;
namespace AbubuResouse.MVP.Presenter
{
    /// <summary>
    /// UIのプレゼンタークラス
    /// </summary>
    public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
    {
        [SerializeField] private Button[] buttons;
        [Header("Button Navigation Settings")]
        [SerializeField] private Vector3 defaultScale = new Vector3(1f, 1f, 1f); // デフォルトのスケール
        [SerializeField] private Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f); // 選択中のスケール
        private int currentIndex = 0;

        private UIModel _model;
        private IUIView _view;
        private UIRepository _repository;

        [SerializeField]
        private GameObject _menuUI;
        [SerializeField]
        private GameObject _settingUI;
        [SerializeField]
        private CanvasGroup _playerCanvasGroup;
        [SerializeField]
        private GameObject[] _linkedUIObjects;

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
                DebugUtility.LogError("UIViewの取得に失敗！");
                return;
            }

            SetupDatabase().Forget();

            //デバック用
            LogAllButtonLinks();
        }

        private void LogAllButtonLinks()
        {
            DebugUtility.Log("=== Checking Database Connection ===");

            // データベース接続が有効か確認
            if (_repository == null)
            {
                DebugUtility.LogError("UIRepository is not initialized.");
                return;
            }

            try
            {
                var testQuery = _repository.GetAllButtonLinks();
                DebugUtility.Log("Database connection successful.");
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"Database connection failed: {ex.Message}");
                return;
            }

            // データベース内のリンクをログに出力
            DebugUtility.Log("=== Database UIButtonLinks ===");
            var allLinks = _repository.GetAllButtonLinks();
            if (allLinks != null && allLinks.Count > 0)
            {
                foreach (var link in allLinks)
                {
                    DebugUtility.Log($"Id: {link.Id}, ButtonName: {link.ButtonName}, LinkedUIObjectName: {link.LinkedUIObjectName}");
                }
            }
            else
            {
                DebugUtility.Log("No UIButtonLinks found in the database.");
            }

            // _linkedUIObjectsの名前をログに出力
            DebugUtility.Log("=== Linked UI Objects ===");
            if (_linkedUIObjects != null && _linkedUIObjects.Length > 0)
            {
                foreach (var linkedUI in _linkedUIObjects)
                {
                    if (linkedUI != null)
                    {
                        DebugUtility.Log($"UI Object Name: {linkedUI.name}");
                    }
                    else
                    {
                        DebugUtility.Log("UI Object is null");
                    }
                }
            }
            else
            {
                DebugUtility.Log("No linked UI objects found in _linkedUIObjects.");
            }
        }


        /// <summary>
        /// 初期化後に呼び出される
        /// 毎フレームEscキーの入力をチェックし、メニューの表示/非表示を切り替える
        /// </summary>
        private void Start()
        {
            if (Gamepad.current != null && buttons.Length > 0)
            {
                SelectButton(currentIndex);
                HighlightButton(currentIndex);
            }

            SetupSubscriptions();

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape) || IsGamepadHomeButtonPressed())
                .Subscribe(_ => ToggleMenuUI())
                .AddTo(this);

        }
        private void Update()
        {
            // ゲームパッドが存在しなければナビゲーションしない
            if (Gamepad.current == null) return;

            HandleGamepadNavigation();
        }
        /// <summary>
        /// ボタンを選択
        /// </summary>
        private void SelectButton(int index)
        {
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }

        /// <summary>
        /// 次のボタンを選択
        /// </summary>
        private void MoveToNextButton()
        {
            currentIndex = (currentIndex + 1) % buttons.Length;
            HighlightButton(currentIndex);
        }

        /// <summary>
        /// 前のボタンを選択
        /// </summary>
        private void MoveToPreviousButton()
        {
            currentIndex = (currentIndex - 1 + buttons.Length) % buttons.Length;
            HighlightButton(currentIndex);
        }

        /// <summary>
        /// 現在のボタンを押す
        /// </summary>
        private void PressCurrentButton()
        {
            buttons[currentIndex].onClick?.Invoke();
        }

        /// <summary>
        /// Checks if the gamepad home button is pressed.
        /// </summary>
        private bool IsGamepadHomeButtonPressed()
        {
            var gamepad = Gamepad.current;
            return gamepad != null && gamepad.startButton.wasPressedThisFrame; // Home button
        }

        /// <summary>
        /// 十字キー左右でのナビゲーション処理
        /// </summary>
        private void HandleGamepadNavigation()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            if (gamepad.dpad.left.wasPressedThisFrame)
            {
                MoveToPreviousButton();
            }
            else if (gamepad.dpad.right.wasPressedThisFrame)
            {
                MoveToNextButton();
            }

            if (gamepad.aButton.wasPressedThisFrame)
            {
                PressCurrentButton();
            }
        }

        /// <summary>
        /// 現在選択中のボタンをハイライトし、その他のボタンをリセット
        /// </summary>
        /// <param name="index">選択中のボタンのインデックス</param>
        private void HighlightButton(int index)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == index)
                {
                    // 選択中のボタンを強調（スケールを変更）
                    SetButtonScale(buttons[i], selectedScale);
                    EventSystem.current.SetSelectedGameObject(buttons[i].gameObject);
                }
                else
                {
                    // 他のボタンをデフォルトに戻す
                    SetButtonScale(buttons[i], defaultScale);
                }
            }
        }

        /// <summary>
        /// ボタンのスケールを設定
        /// </summary>
        /// <param name="button">対象のボタン</param>
        /// <param name="scale">ボタンのスケール</param>
        private void SetButtonScale(Button button, Vector3 scale)
        {
            if (button == null) return;
            button.transform.localScale = scale;
        }
        /// <summary>
        /// UIを初期化
        /// メニュー画面を表示して、設定画面を非表示にしている
        /// </summary>
        private async UniTask InitializeUI()
        {
            _model.IsCursorVisible.Value = false;
            // メニューを開いた状態に設定
            _view.SetMenuVisibility(_menuUI, true);
            _view.SetSettingVisibility(_settingUI, false);
            // ゲーム開始時にメニューUIを表示し、その他のUIを非アクティブにする
            foreach (var linkedUI in _linkedUIObjects)
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
        /// 指定された、ボタンに紐づけられたUIオブジェクトを開く
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
                DebugUtility.LogError($"指定されたUIオブジェクト存在しない!? {uiObject}");
            }
        }

        /// <summary>
        /// すべてのボタンに紐づけられたUIオブジェクトを閉じる
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
            if (StopManager.Instance.IsStopped)
            { return; }
            SEManager.Instance.PlaySound("MenuOpenSE", 1.0f);
            _model.IsCursorVisible.Value = true;
            _view.SetMenuVisibility(_menuUI, false);
            _view.SetSettingVisibility(_settingUI, true);
            StopManager.Instance.IsStopped = true;
            SetPlayerCanvasAlpha(0);
        }

        /// <summary>
        /// メニューUIを閉じる
        /// </summary>
        private void CloseMenuUI()
        {
            StopManager.Instance.IsStopped = false;
            SEManager.Instance.PlaySound("MenuCloseSE", 1.0f);
            _model.IsCursorVisible.Value = false;
            _view.SetMenuVisibility(_menuUI, true);
            _view.SetSettingVisibility(_settingUI, false);
            SetPlayerCanvasAlpha(1);
        }

        /// <summary>
        /// プレイヤーキャンバスのアルファ値を設定
        /// </summary>
        /// <param name="alpha">設定するアルファ値</param>
        private void SetPlayerCanvasAlpha(float alpha)
        {
            if (_playerCanvasGroup != null)
            {
                _playerCanvasGroup.alpha = alpha;
                _playerCanvasGroup.interactable = alpha > 0;
                _playerCanvasGroup.blocksRaycasts = alpha > 0;
            }
            else
            {
                DebugUtility.LogError("PlayerCanvasGroupが設定されていません！");
            }
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
                _view.SetSettingVisibility(_settingUI, false);
                _model.IsCursorVisible.Value = false;
                foreach (var linkedUI in _linkedUIObjects)
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
            var databasePath = $"{Application.streamingAssetsPath}/ui_database.db";
            _repository = new UIRepository(databasePath);
            await UniTask.SwitchToMainThread();
        }

    }
}
