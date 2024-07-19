using System;
using UnityEngine;
using R3;
using UnityEngine.UI;

public class TitleUIPresenter : MonoBehaviour
{
    private ITitleUIView _view;
    private TitleUIModel _model;

    [SerializeField]
    private GameObject[] _linkedUIObjects;

    private void Start()
    {
        _view = new TitleUIView(_linkedUIObjects);
        _model = new TitleUIModel();

        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            .Subscribe(_ => ChangeSelectedButtonIndex())
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Return))
            .Subscribe(_ => ExecuteSelectedButtonAction())
            .AddTo(this);

        SetupSubscriptions();
    }

    private void ChangeSelectedButtonIndex()
    {
        int currentIndex = _model.SelectedButtonIndex.Value;

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
