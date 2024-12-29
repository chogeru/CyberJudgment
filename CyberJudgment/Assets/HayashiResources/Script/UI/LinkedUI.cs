using UnityEngine;
using UnityEngine.InputSystem;
using AbubuResouse.MVP.Presenter;
using AbubuResouse.Log;

namespace AbubuResouse.MVP.View
{
    /// <summary>
    /// UIの戻るボタンがクリックされたときに設定画面を表示するクラス
    /// </summary>
    public class LinkedUI : MonoBehaviour
    {
        private void Update()
        {
            HandleGamepadInput();
        }

        /// <summary>
        /// ゲームパッドの入力を処理
        /// </summary>
        private void HandleGamepadInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            // Bボタンが押されたとき
            if (gamepad.buttonEast.wasPressedThisFrame) // Bボタン (East button)
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
