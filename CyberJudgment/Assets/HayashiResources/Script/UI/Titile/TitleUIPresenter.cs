using System;
using UnityEngine;
using R3;
using UnityEngine.UI;
using AbubuResouse.Singleton;
using UnityEngine.InputSystem;

public class TitleUIPresenter : MonoBehaviour
{
    private ITitleUIView _view;
    private TitleUIModel _model;

    [SerializeField]
    private GameObject[] _linkedUIObjects;

    [SerializeField, Header("UI切り替え時SE")]
    private string _uISelectSE;
    [SerializeField, Header("音量")]
    private float _volume;

    private void Start()
    {
        _view = new TitleUIView(_linkedUIObjects);
        _model = new TitleUIModel();

        Observable.EveryUpdate()
             .Where(_ => Input.GetKeyDown(KeyCode.UpArrow))
             .Subscribe(_ => MoveSelectionUp())
             .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.DownArrow))
            .Subscribe(_ => MoveSelectionDown())
            .AddTo(this);

        // コントローラーのD-pad上下の購読
        Observable.EveryUpdate()
            .Where(_ => Gamepad.current != null)
            .Where(_ => Gamepad.current.dpad.up.wasPressedThisFrame)
            .Subscribe(_ => MoveSelectionUp())
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => Gamepad.current != null)
            .Where(_ => Gamepad.current.dpad.down.wasPressedThisFrame)
            .Subscribe(_ => MoveSelectionDown())
            .AddTo(this);

        // キーボードのエンターキーとコントローラーの選択ボタンの購読
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.Z) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
            .Subscribe(_ => ExecuteSelectedButtonAction())
            .AddTo(this);

        SetupSubscriptions();
    }

    /// <summary>
    /// 選択項目を上に移動するメソッド
    /// </summary>
    private void MoveSelectionUp()
    {
        int currentIndex = _model.SelectedButtonIndex.Value;

        // 選択音を再生
        SEManager.Instance.PlaySound(_uISelectSE, _volume);

        // インデックスを更新（ループ）
        _model.SelectedButtonIndex.Value = (currentIndex == 0) ? _linkedUIObjects.Length - 1 : currentIndex - 1;
    }

    /// <summary>
    /// 選択項目を下に移動するメソッド
    /// </summary>
    private void MoveSelectionDown()
    {
        int currentIndex = _model.SelectedButtonIndex.Value;

        // 選択音を再生
        SEManager.Instance.PlaySound(_uISelectSE, _volume);

        // インデックスを更新（ループ）
        _model.SelectedButtonIndex.Value = (currentIndex == _linkedUIObjects.Length - 1) ? 0 : currentIndex + 1;
    }

    private void ChangeSelectedButtonIndex()
    {
        int currentIndex = _model.SelectedButtonIndex.Value;

        SEManager.Instance.PlaySound(_uISelectSE, _volume);

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _model.SelectedButtonIndex.Value = (currentIndex == 0) ? _linkedUIObjects.Length - 1 : currentIndex - 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _model.SelectedButtonIndex.Value = (currentIndex == _linkedUIObjects.Length - 1) ? 0 : currentIndex + 1;
        }
    }

    private void ExecuteSelectedButtonAction()
    {
        var selectedButton = _view.GetSelectedButton(_model.SelectedButtonIndex.Value);
        if (selectedButton != null)
        {
            var buttonComponent = selectedButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.Invoke();
            }
        }
    }

    private void SetupSubscriptions()
    {
        _model.SelectedButtonIndex
            .Subscribe(index => _view.HighlightButton(index))
            .AddTo(this);
    }
}
