using System.Collections.Generic;
using System.Linq;
using AbubuResouse.Log;
using UnityEngine;
using UnityEngine.Audio;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// SEの再生を管理するマネージャークラス
    /// </summary>
    public class SEManager : AudioManagerBase<SEManager>
    {
        private List<AudioSource> _audioSources = new List<AudioSource>();
        [SerializeField, Header("AudioSourceの数")]
        private int _audioSourceSize = 10;

        private Dictionary<string, AudioClip> _loadedClips = new Dictionary<string, AudioClip>();

        protected override void Awake()
        {
            base.Awake();

            // 自身のオブジェクトにアタッチされているすべての AudioSource を取得
            AudioSource[] existingSources = GetComponents<AudioSource>();
            if (existingSources.Length == 0)
            {
                DebugUtility.LogError("AudioSource が見つかりません。オブジェクトにあらかじめ AudioSource を設定してください。");
                return;
            }

            // 既存のAudioSourceをプールに追加
            foreach (var source in existingSources)
            {
                _audioSources.Add(source);
            }

            DebugUtility.Log($"既存のAudioSourceをプールに追加しました: {existingSources.Length}個");

            // 不足している分の AudioSource を生成して追加
            int sourcesToAdd = _audioSourceSize - _audioSources.Count;
            for (int i = 0; i < sourcesToAdd; i++)
            {
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                _audioSources.Add(newSource);
            }

            DebugUtility.Log($"{_audioSourceSize}個のAudioSourceを確保しました（既存: {existingSources.Length}個, 追加: {sourcesToAdd}個）。");
        }

        /// <summary>
        /// データベース名として "se_data.db" を返す
        /// </summary>
        protected override string GetDatabaseName() => "se_data.db";

        /// <summary>
        /// 指定されたSE名と同じレコードをデータベースから検索して、SEを再生する
        /// </summary>
        /// <param name="clipName">サウンドクリップ名</param>
        /// <param name="volume">音量</param>
        public override void PlaySound(string clipName, float volume)
        {
            var query = connection.Table<SoundClip>().FirstOrDefault(x => x.ClipName == clipName);
            if (query != null)
            {
                PlayFromPath(query.ClipPath, volume);
            }
            else
            {
                DebugUtility.Log($"指定されたサウンドクリップ名に一致するレコードがデータベースに存在しない: {clipName}");
            }
        }

        /// <summary>
        /// 指定されたパスのサウンドを再生
        /// </summary>
        /// <param name="path">リソースパス</param>
        /// <param name="volume">音量</param>
        private void PlayFromPath(string path, float volume)
        {
            if (!_loadedClips.ContainsKey(path))
            {
                // リソースをロードしてキャッシュ
                AudioClip clip = Resources.Load<AudioClip>($"SE/{path}");
                if (clip == null)
                {
                    DebugUtility.LogError($"指定されたサウンドクリップが見つかりません: {path}");
                    return;
                }

                _loadedClips[path] = clip;
            }

            // 空いているAudioSourceを探して再生
            AudioSource source = _audioSources.FirstOrDefault(a => !a.isPlaying);
            if (source != null)
            {
                source.clip = _loadedClips[path];
                source.volume = volume;
                source.Play();
                DebugUtility.Log($"サウンドクリップを再生: {path}");
            }
            else
            {
                DebugUtility.LogError("再生可能なAudioSourceがありません");
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