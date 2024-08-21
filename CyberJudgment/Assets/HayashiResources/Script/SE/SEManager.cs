using System.Collections.Generic;
using System.Linq;
using AbubuResouse.Log;
using UnityEngine;
namespace AbubuResouse.Singleton
{
    /// <summary>
    /// SEの再生を管理するマネージャークラス
    /// </summary>
    public class SEManager : AudioManagerBase<SEManager>
    {

        private List<AudioSource> _audioSources = new List<AudioSource>();
        [SerializeField, Header("AudioSouceザイズ")]
        private int _audioSouceSize;

        protected override void Awake()
        {
            base.Awake();
            // 必要な数のAudioSourceを生成してプールに追加
            for (int i = 0; i < _audioSouceSize; i++) 
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.loop = false;
                source.playOnAwake = false;
                _audioSources.Add(source);
            }
        }

        /// <summary>
        /// データベース名として "se_data.db" を返す
        /// </summary>
        protected override string GetDatabaseName() => "se_data.db";

        /// <summary>
        /// 指定されたSE名と同じレコードをデータベースから検索して、SEを再生する
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <param name="volume">音量</param>
        public override void PlaySound(string clipName, float volume)
        {
            var query = connection.Table<SoundClip>().FirstOrDefault(x => x.ClipName == clipName);
            if (query != null)
            {
                LoadAndPlayClip($"SE/{query.ClipPath}", volume);
            }
            else
            {
                DebugUtility.Log($"指定されたサウンドクリップ名に一致するレコードがデータベースに存在しない: {clipName}");
            }
        }


        /// <summary>
        /// 指定されたリソースパスのサウンドクリップをロードし、再生する
        /// </summary>
        /// <param name="resourcePath">リソースパス</param>
        /// <param name="volume">音量</param>
        protected override void LoadAndPlayClip(string resourcePath, float volume)
        {
            try
            {
                AudioClip clip = Resources.Load<AudioClip>(resourcePath);
                if (clip != null)
                {
                    // 空いているAudioSourceを探す
                    AudioSource source = _audioSources.FirstOrDefault(a => !a.isPlaying);
                    if (source != null)
                    {
                        source.clip = clip;
                        source.volume = volume;
                        source.Play();
                        DebugUtility.Log($"サウンドクリップを再生: {resourcePath}");
                    }
                    else
                    {
                        DebugUtility.LogError("再生可能なAudioSourceがありません");
                    }
                }
                else
                {
                    DebugUtility.LogError($"サウンドファイルが見つからない: {resourcePath}");
                }
            }
            catch (System.Exception ex)
            {
                DebugUtility.LogError($"サウンドファイルのロード時にエラー発生: {ex.Message}");
            }
        }


        /// <summary>
        /// データベースのサウンドクリップテーブル
        /// </summary>
        private class SoundClip
        {
            public int Id { get; set; }
            public string ClipName { get; set; }
            public string ClipPath { get; set; }
        }
    }
}
