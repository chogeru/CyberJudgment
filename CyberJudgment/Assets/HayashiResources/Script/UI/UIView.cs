using UnityEngine;
using AbubuResouse.MVP.View;
using System.Linq;
using AbubuResouse.Log;

namespace AbubuResouse.MVP.View
{
    // <summary>
    /// UI�r���[�N���X�A���j���[�Ɛݒ��ʂ̕\����Ԃ��Ǘ�����
    /// </summary>
    public class UIView : IUIView
    {
        /// <summary>
        /// ���j���[��ʂ̕\��/��\����ݒ肷��
        /// </summary>
        �@/// <param name="isVisible">�\������ꍇ��true </param>
        public void SetMenuVisibility(GameObject menuUI, bool isVisible)
        {
            menuUI.SetActive(isVisible);
        }

        /// <summary>
        /// �ݒ��ʂ̕\��/��\����ݒ肷��
        /// </summary>
        /// <param name="isVisible">�\������ꍇ��true </param>
        public void SetSettingVisibility(GameObject settingUI, bool isVisible)
        {
            settingUI.SetActive(isVisible);
        }

        /// �w�肳�ꂽ�����N���ꂽUI�I�u�W�F�N�g�̕\��/��\����ݒ肷��
        /// </summary>
        /// <param name="index">�����N���ꂽUI�I�u�W�F�N�g�̃C���f�b�N�X</param>
        /// <param name="isVisible">�\������ꍇ��true </param>
        public void SetLinkedUIVisibility(GameObject[] linkedUIObjects, int index, bool isVisible)
        {
            if (index >= 0 && index < linkedUIObjects.Length)
            {
                linkedUIObjects[index].SetActive(isVisible);
            }
        }

        /// <summary>
        /// �J�[�\���̕\��/��\����ݒ肷��
        /// </summary>
        /// <param name="isVisible">�\������ꍇ��true�@</param>
        public void SetCursorVisibility(bool isVisible)
        {
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
