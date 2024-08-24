using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AbubuResouse.Singleton;
using VInspector;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MapNameStationManager : SingletonMonoBehaviour<MapNameStationManager>
{
    [Tab("UI")]
    [SerializeField, Header("�}�b�v����UI")]
    private CanvasGroup _mapNameUI;
    [SerializeField, Header("�}�b�v����Text")]
    private TextMeshProUGUI _textMeshProUGUI;

    [Tab("�T�E���h")]
    [SerializeField, Header("UI�\������SE��")]
    private string _uiActiveOnSE;

    [SerializeField, Header("����")]
    private float _volume;

    public void SetMapNameUI(string mapName)
    {
        _mapNameUI.alpha = 1;
        _textMeshProUGUI.text = mapName;
        SEManager.Instance.PlaySound(_uiActiveOnSE, _volume);
    }
}
