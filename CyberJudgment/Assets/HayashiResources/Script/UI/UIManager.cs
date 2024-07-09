using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.UI;
using System.Diagnostics;
using VInspector;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    #region オブジェクト
    [Tab("オブジェクト")]
    [SerializeField, Header("メニュー画面")]
    private GameObject m_MenuUI;
    [SerializeField ,Header("設定画面")]
    private GameObject m_SettingUI;
    [EndTab]
    #endregion

    #region トリガー
    [Tab("トリガー")]
    [ReadOnly, Header("メニューがひらいているかどうか")]
    public bool isMenuOpen=false;
    [EndTab]
    #endregion

    private void Awake()
    {
        m_MenuUI.SetActive(true);
        m_SettingUI.SetActive(false);
    }

    private void Update()
    {
        if (!isMenuOpen)
        {
            UIState();
        }
    }
    private void UIState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_MenuUI.SetActive(false);
            m_SettingUI.SetActive(true);
            isMenuOpen =true;
        }
    }
}
