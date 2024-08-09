using UnityEngine;
using SQLite4Unity3d;
using System;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// サウンド関連の基本機能クラス
    /// </summary>
    public abstract class AudioManagerBase<T> : SingletonMonoBehaviour<T> where T : SingletonMonoBehaviour<T>
    {
        protected AudioSource audioSource;
        protected SQLiteConnection connection;

        protected override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            InitializeDatabase(GetDatabaseName());
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
        protected void LoadAndPlayClip(string resourcePath, float volume)
        {
            try
            {
                AudioClip clip = Resources.Load<AudioClip>(resourcePath);
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                    audioSource.volume = volume;
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
