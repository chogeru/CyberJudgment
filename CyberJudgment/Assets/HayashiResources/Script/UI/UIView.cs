using UnityEngine;
using AbubuResouse.MVP.View;
using System.Linq;
using AbubuResouse.Log;

namespace AbubuResouse.MVP.View
{
    // <summary>
    /// UIビュークラス、メニューと設定画面の表示状態を管理する
    /// </summary>
    public class UIView : IUIView
    {
        /// <summary>
        /// メニュー画面の表示/非表示を設定する
        /// </summary>
        　/// <param name="isVisible">表示する場合はtrue </param>
        public void SetMenuVisibility(GameObject menuUI, bool isVisible)
        {
            menuUI.SetActive(isVisible);
        }

        /// <summary>
        /// 設定画面の表示/非表示を設定する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue </param>
        public void SetSettingVisibility(GameObject settingUI, bool isVisible)
        {
            settingUI.SetActive(isVisible);
        }

        /// 指定されたリンクされたUIオブジェクトの表示/非表示を設定する
        /// </summary>
        /// <param name="index">リンクされたUIオブジェクトのインデックス</param>
        /// <param name="isVisible">表示する場合はtrue </param>
        public void SetLinkedUIVisibility(GameObject[] linkedUIObjects, int index, bool isVisible)
        {
            if (index >= 0 && index < linkedUIObjects.Length)
            {
                linkedUIObjects[index].SetActive(isVisible);
            }
        }

        /// <summary>
        /// カーソルの表示/非表示を設定する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue　</param>
        public void SetCursorVisibility(bool isVisible)
        {
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
