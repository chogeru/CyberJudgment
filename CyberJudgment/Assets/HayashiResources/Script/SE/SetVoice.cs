using UnityEngine;
using AbubuResouse.Singleton;

namespace AbubuResouse
{
    /// <summary>
    /// 指定されたボイスを再生するクラス
    /// </summary>
    public class SetVoice : MonoBehaviour
    {
        [SerializeField, Header("開始時に再生するボイス名")]
        private string _voiceName;

        [SerializeField, Header("ボイス音量")]
        private float _Volume;

        [SerializeField, Header("開始時に自動再生")]
        private bool _StartOnPlay;

        private void Start()
        {
            if (_StartOnPlay)
            {
                VoiceManager.Instance.PlaySound(_voiceName, _Volume);
            }
        }

        /// <summary>
        /// 任意のタイミングでボイス再生用関数
        /// </summary>
        public void PlayVoice()
        {
            VoiceManager.Instance.PlaySound(_voiceName, _Volume);
        }
    }
}