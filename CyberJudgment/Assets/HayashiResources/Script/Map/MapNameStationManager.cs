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
    [SerializeField, Header("マップ名のUI")]
    private CanvasGroup _mapNameUI;
    [SerializeField, Header("マップ名のText")]
    private TextMeshProUGUI _textMeshProUGUI;

    [Tab("サウンド")]
    [SerializeField, Header("UI表示時のSE名")]
    private string _uiActiveOnSE;

    [SerializeField, Header("音量")]
    private float _volume;

    public void SetMapNameUI(string mapName)
    {
        _mapNameUI.alpha = 1;
        _textMeshProUGUI.text = mapName;
        SEManager.Instance.PlaySound(_uiActiveOnSE, _volume);
    }
}
