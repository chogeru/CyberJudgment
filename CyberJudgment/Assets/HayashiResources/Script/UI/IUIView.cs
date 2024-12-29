using UnityEngine;

namespace AbubuResouse.MVP.View
{
    /// <summary>
    /// UI�r���[�̃C���^�[�t�F�[�X�A���j���[��ݒ��ʂ̕\����Ԃ��Ǘ�
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
