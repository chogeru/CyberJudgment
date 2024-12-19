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

    [SerializeField, Header("UI�؂�ւ���SE")]
    private string _uISelectSE;
    [SerializeField, Header("����")]
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

        // �R���g���[���[��D-pad�㉺�̍w��
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

        // �L�[�{�[�h�̃G���^�[�L�[�ƃR���g���[���[�̑I���{�^���̍w��
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.Z) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
            .Subscribe(_ => ExecuteSelectedButtonAction())
            .AddTo(this);

        SetupSubscriptions();
    }

    /// <summary>
    /// �I�����ڂ���Ɉړ����郁�\�b�h
    /// </summary>
    private void MoveSelectionUp()
    {
        int currentIndex = _model.SelectedButtonIndex.Value;

        // �I�������Đ�
        SEManager.Instance.PlaySound(_uISelectSE, _volume);

        // �C���f�b�N�X���X�V�i���[�v�j
        _model.SelectedButtonIndex.Value = (currentIndex == 0) ? _linkedUIObjects.Length - 1 : currentIndex - 1;
    }

    /// <summary>
    /// �I�����ڂ����Ɉړ����郁�\�b�h
    /// </summary>
    private void MoveSelectionDown()
    {
        int currentIndex = _model.SelectedButtonIndex.Value;

        // �I�������Đ�
        SEManager.Instance.PlaySound(_uISelectSE, _volume);

        // �C���f�b�N�X���X�V�i���[�v�j
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
