using UnityEngine;
using SQLite4Unity3d;
using System;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// サウンド関連の基本機能クラス
    /// （フィルタ制御を AudioFilterManager に委譲し、プロパティでキャッシュを公開）
    /// </summary>
    public abstract class AudioManagerBase<T> : SingletonMonoBehaviour<T> where T : SingletonMonoBehaviour<T>
    {
        protected AudioSource audioSource;
        protected SQLiteConnection connection;

        /// <summary>
        /// AudioFilterManager を一度だけ取得してキャッシュ
        /// </summary>
        private AudioFilterManager filterManager;

        /// <summary>
        /// フィルタ制御用のプロパティ。
        /// 呼び出し側はこれを通してフィルタを設定できる。
        /// </summary>
        public AudioFilterManager FilterManager => filterManager;

        protected override void Awake()
        {
            base.Awake();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // データベース初期化
            InitializeDatabase(GetDatabaseName());

            filterManager = GetComponent<AudioFilterManager>();
            if (filterManager == null)
            {
                filterManager = gameObject.AddComponent<AudioFilterManager>();
            }
        }

        /// <summary>
        /// 指定されたデータベース名を基にデータベースに接続する
        /// </summary>
        /// <param name="databaseName">データベース名</param>
        protected void InitializeDatabase(string databaseName)
        {
            var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, databaseName).Replace("\\", "/");
            try
            {
                connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
                DebugUtility.Log($"データベース接続に成功!パス: {databasePath}");
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"データベースの接続に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// データベース名を取得するための抽象関数
        /// </summary>
        protected abstract string GetDatabaseName();

        /// <summary>
        /// 指定されたサウンドクリップを再生する抽象関数
        /// </summary>
        /// <param name="clipName">サウンドクリップ名</param>
        /// <param name="volume">音量</param>
        public abstract void PlaySound(string clipName, float volume);

        /// <summary>
        /// 指定されたリソースパスのサウンドクリップをロードし、再生する
        /// </summary>
        /// <param name="resourcePath">リソースパス</param>
        /// <param name="volume">音量</param>
        protected virtual void LoadAndPlayClip(string resourcePath, float volume)
        {
            try
            {
                AudioClip clip = Resources.Load<AudioClip>(resourcePath);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.volume = volume;
                    audioSource.Play();
                    DebugUtility.Log($"サウンドクリップを再生: {resourcePath}");
                }
                else
                {
                    DebugUtility.LogError($"サウンドファイルが見つからない: {resourcePath}");
                }
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"サウンドファイルのロード時にエラー発生: {ex.Message}");
            }
        }
    }
}
