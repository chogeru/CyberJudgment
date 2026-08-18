using System.Linq;
using UnityEngine.SceneManagement;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// BGMの再生を管理するマネージャークラス
    /// </summary>
    public class BGMManager : AudioManagerBase<BGMManager>
    {

        /// <summary>
        /// データベース名として "bgm_data.db" を返す
        /// </summary>
        protected override string GetDatabaseName() => "bgm_data.db";

        protected override void Awake()
        {
            base.Awake();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// シーンがロードされた際にBGMを停止する
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StopBGM();

        /// <summary>
        /// 指定されたBGM名と同じレコードをデータベースから検索して、BGMを再生する
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <param name="volume">音量</param>
        public override void PlaySound(string bgmName, float volume)
        {
            var query = connection.Table<BGM>().FirstOrDefault(x => x.BGMName == bgmName);
            if (query != null)
            {
                LoadAndPlayClip($"BGM/{query.BGMFileName}", volume);
            }
            else
            {
                DebugUtility.Log($"指定されたBGM名に一致するレコードがデータベースに存在しない: {bgmName}");
            }
        }

        /// <summary>
        /// 現在再生中のBGMを停止
        /// </summary>
        public void StopBGM()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.clip = null;
                DebugUtility.Log("BGM停止");
            }
        }

        /// <summary>
        /// シーンロードイベントを解除
        /// </summary>
        protected override void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// データベースのBGMテーブル
        /// </summary>
        private class BGM
        {
            public int Id { get; set; }
            public string BGMName { get; set; }
            public string BGMFileName { get; set; }
        }
    }
}
