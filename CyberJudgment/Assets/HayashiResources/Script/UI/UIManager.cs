using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.UI;
using System.Diagnostics;
using VInspector;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    #region �I�u�W�F�N�g
    [Tab("�I�u�W�F�N�g")]
    [SerializeField, Header("���j���[���")]
    private GameObject m_MenuUI;
    [SerializeField ,Header("�ݒ���")]
    private GameObject m_SettingUI;
    [EndTab]
    #endregion

    #region �g���K�[
    [Tab("�g���K�[")]
    [ReadOnly, Header("���j���[���Ђ炢�Ă��邩�ǂ���")]
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
