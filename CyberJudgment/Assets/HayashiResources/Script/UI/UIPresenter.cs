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
    /// UI�̃v���[���^�[�N���X
    /// </summary>
    public class UIPresenter : SingletonMonoBehaviour<UIPresenter>
    {
        [SerializeField] private Button[] buttons;
        [Header("Button Navigation Settings")]
        [SerializeField] private Vector3 defaultScale = new Vector3(1f, 1f, 1f); // �f�t�H���g�̃X�P�[��
        [SerializeField] private Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f); // �I�𒆂̃X�P�[��
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
            DebugUtility.Log("=== Checking Database Connection ===");

            // �f�[�^�x�[�X�ڑ����L�����m�F
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

            // �f�[�^�x�[�X���̃����N�����O�ɏo��
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

            // _linkedUIObjects�̖��O�����O�ɏo��
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
        /// ��������ɌĂяo�����
        /// ���t���[��Esc�L�[�̓��͂��`�F�b�N���A���j���[�̕\��/��\����؂�ւ���
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
            // �Q�[���p�b�h�����݂��Ȃ���΃i�r�Q�[�V�������Ȃ�
            if (Gamepad.current == null) return;

            HandleGamepadNavigation();
        }
        /// <summary>
        /// �{�^����I��
        /// </summary>
        private void SelectButton(int index)
        {
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }

        /// <summary>
        /// ���̃{�^����I��
        /// </summary>
        private void MoveToNextButton()
        {
            currentIndex = (currentIndex + 1) % buttons.Length;
            HighlightButton(currentIndex);
        }

        /// <summary>
        /// �O�̃{�^����I��
        /// </summary>
        private void MoveToPreviousButton()
        {
            currentIndex = (currentIndex - 1 + buttons.Length) % buttons.Length;
            HighlightButton(currentIndex);
        }

        /// <summary>
        /// ���݂̃{�^��������
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
        /// �\���L�[���E�ł̃i�r�Q�[�V��������
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
        /// ���ݑI�𒆂̃{�^�����n�C���C�g���A���̑��̃{�^�������Z�b�g
        /// </summary>
        /// <param name="index">�I�𒆂̃{�^���̃C���f�b�N�X</param>
        private void HighlightButton(int index)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == index)
                {
                    // �I�𒆂̃{�^���������i�X�P�[����ύX�j
                    SetButtonScale(buttons[i], selectedScale);
                    EventSystem.current.SetSelectedGameObject(buttons[i].gameObject);
                }
                else
                {
                    // ���̃{�^�����f�t�H���g�ɖ߂�
                    SetButtonScale(buttons[i], defaultScale);
                }
            }
        }

        /// <summary>
        /// �{�^���̃X�P�[����ݒ�
        /// </summary>
        /// <param name="button">�Ώۂ̃{�^��</param>
        /// <param name="scale">�{�^���̃X�P�[��</param>
        private void SetButtonScale(Button button, Vector3 scale)
        {
            if (button == null) return;
            button.transform.localScale = scale;
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
        /// ���j���[UI�����
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
        /// �v���C���[�L�����o�X�̃A���t�@�l��ݒ�
        /// </summary>
        /// <param name="alpha">�ݒ肷��A���t�@�l</param>
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
                DebugUtility.LogError("PlayerCanvasGroup���ݒ肳��Ă��܂���I");
            }
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
            var databasePath = $"{Application.streamingAssetsPath}/ui_database.db";
            _repository = new UIRepository(databasePath);
            await UniTask.SwitchToMainThread();
        }

    }
}
