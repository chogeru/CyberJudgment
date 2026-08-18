using UnityEngine;

namespace AbubuResouse.MVP.View
{
    /// <summary>
    /// UIビューのインターフェース、メニューや設定画面の表示状態を管理
    /// </summary>
    using UnityEngine;

    public interface IUIView
    {
        void SetMenuVisibility(GameObject menuUI, bool isVisible);
        void SetSettingVisibility(GameObject settingUI, bool isVisible);
        void SetLinkedUIVisibility(GameObject[] linkedUIObjects, int index, bool isVisible);
        void SetCursorVisibility(bool isVisible);
    }
}
