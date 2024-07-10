using UnityEngine;
using VInspector;

/// <summary>
/// UIビュークラス、メニューと設定画面の表示状態を管理する
/// </summary>
public class UIView : MonoBehaviour
{
    [Tab("オブジェクト")]
    [SerializeField, Header("メニュー画面")]
    private GameObject m_MenuUI;
    [SerializeField, Header("設定画面")]
    private GameObject m_SettingUI;
    [SerializeField, Header("紐づけられたUIオブジェクト")]
    private GameObject[] m_LinkedUIObjects;
    public GameObject[] LinkedUIObjects => m_LinkedUIObjects;
    [EndTab]

    /// <summary>
    /// メニュー画面の表示/非表示を設定する
    /// </summary>
   　/// <param name="isVisible">表示する場合はtrue。</param>
    public void SetMenuVisibility(bool isVisible)
    {
        m_MenuUI.SetActive(isVisible);
    }

    /// <summary>
    /// 設定画面の表示/非表示を設定する
    /// </summary>
    /// <param name="isVisible">表示する場合はtrue。</param>
    public void SetSettingVisibility(bool isVisible)
    {
        m_SettingUI.SetActive(isVisible);
    }

    /// 指定されたリンクされたUIオブジェクトの表示/非表示を設定する
    /// </summary>
    /// <param name="index">リンクされたUIオブジェクトのインデックス。</param>
    /// <param name="isVisible">表示する場合はtrue。</param>
    public void SetLinkedUIVisibility(int index, bool isVisible)
    {
        if (index >= 0 && index < m_LinkedUIObjects.Length)
        {
            m_LinkedUIObjects[index].SetActive(isVisible);
        }
    }

    /// <summary>
    /// カーソルの表示/非表示を設定する
    /// </summary>
    /// <param name="isVisible">表示する場合はtrue。</param>
    public void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

}
