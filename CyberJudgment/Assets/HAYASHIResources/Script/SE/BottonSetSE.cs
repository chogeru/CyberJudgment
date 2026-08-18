using UnityEngine;
using UnityEngine.UI;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// ボタンが押されたときにSEを再生するクラス
    /// </summary>

    public class BottonSetSE : MonoBehaviour
    {
        [SerializeField, Header("SE名")]
        private string _seName;
        [SerializeField, Header("音量")]
        private float _volume;

        /// <summary>
        /// 初期化時にボタンのクリックイベントを設定する
        /// </summary>
        void Start()
        {
            Button button = this.gameObject.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(() => SEManager.Instance.PlaySound(_seName, _volume));
            }
            else
            {
                Debug.LogError("ボタンコンポーネント無いっすよ");
            }
        }
    }
}