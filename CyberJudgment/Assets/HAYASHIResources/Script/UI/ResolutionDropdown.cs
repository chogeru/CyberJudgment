using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ResolutionDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private List<Vector2Int> customResolutions = new List<Vector2Int>()
    {
        new Vector2Int(640, 480),
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440)
    };

    private void Start()
    {
        // Dropdown を初期化
        InitializeDropdown();
    }

    /// <summary>
    /// Dropdown項目の初期化
    /// </summary>
    private void InitializeDropdown()
    {
        // ドロップダウンのリストをクリア
        resolutionDropdown.ClearOptions();

        // 表示用テキスト
        List<string> options = new List<string>();

        // 解像度リストを走査してDropdownに表示
        foreach (var res in customResolutions)
        {
            options.Add($"{res.x} x {res.y}");
        }

        // Dropdownにリストを設定
        resolutionDropdown.AddOptions(options);

        // 値が変更されたときのコールバックを設定
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    /// <summary>
    /// Dropdownの選択変更時に呼び出される関数
    /// </summary>
    /// <param name="index">選択された要素のインデックス</param>
    private void OnResolutionChanged(int index)
    {
        Vector2Int selectedRes = customResolutions[index];

        // フルスクリーンを切り替える場合はここでフラグを指定
        // （例）フルスクリーンをOFFにしてウィンドウモードで解像度変更
        bool isFullScreen = false;

        // 解像度変更
        Screen.SetResolution(selectedRes.x, selectedRes.y, isFullScreen);
    }
}
