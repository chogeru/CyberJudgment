using UnityEngine;
using UnityEngine.InputSystem;
using AbubuResouse.MVP.Presenter;
using AbubuResouse.Log;

namespace AbubuResouse.MVP.View
{
    /// <summary>
    /// UI�̖߂�{�^�����N���b�N���ꂽ�Ƃ��ɐݒ��ʂ�\������N���X
    /// </summary>
    public class LinkedUI : MonoBehaviour
    {
        private void Update()
        {
            HandleGamepadInput();
        }

        /// <summary>
        /// �Q�[���p�b�h�̓��͂�����
        /// </summary>
        private void HandleGamepadInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            // B�{�^���������ꂽ�Ƃ�
            if (gamepad.buttonEast.wasPressedThisFrame) // B�{�^�� (East button)
            {
                OnBackButtonClicked();
            }
        }
        public void OnBackButtonClicked()
        {
            UIPresenter.Instance.ShowSettingUI();
        }
    }
}
