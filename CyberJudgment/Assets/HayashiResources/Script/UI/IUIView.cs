using UnityEngine;

public interface IUIView
{
    void SetMenuVisibility(GameObject menuUI, bool isVisible);
    void SetSettingVisibility(GameObject settingUI, bool isVisible);
    void SetLinkedUIVisibility(GameObject[] linkedUIObjects, int index, bool isVisible);
    void SetCursorVisibility(bool isVisible);
}
