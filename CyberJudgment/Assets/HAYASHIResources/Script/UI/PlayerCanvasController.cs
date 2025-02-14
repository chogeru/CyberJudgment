using UnityEngine;
using AbubuResouse.Log; // 必要に応じてインポート

namespace AbubuResouse.Singleton
{
    public class PlayerCanvasController : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _playerCanvasGroup;

        private void Start()
        {
            if (_playerCanvasGroup == null)
            {
                _playerCanvasGroup = GetComponent<CanvasGroup>();
                if (_playerCanvasGroup == null)
                {
                    DebugUtility.LogError("CanvasGroupがプレイヤーキャンバスにアタッチされていません。");
                }
            }
        }

        private void Update()
        {
            if (StopManager.Instance != null)
            {
                if (StopManager.Instance.IsStopped)
                {
                    SetCanvasAlpha(0f);
                }
                else
                {
                    SetCanvasAlpha(1f);
                }
            }
            else
            {
                DebugUtility.LogError("StopManagerのインスタンスが存在しません。");
            }
        }

        /// <summary>
        /// キャンバスのアルファ値を設定する
        /// </summary>
        /// <param name="alpha">0で透明、1で完全不透明</param>
        private void SetCanvasAlpha(float alpha)
        {
            if (_playerCanvasGroup != null)
            {
                _playerCanvasGroup.alpha = alpha;
                // 透明にする場合、インタラクションも無効にする
                _playerCanvasGroup.interactable = alpha > 0f;
                _playerCanvasGroup.blocksRaycasts = alpha > 0f;
            }
        }
    }
}
