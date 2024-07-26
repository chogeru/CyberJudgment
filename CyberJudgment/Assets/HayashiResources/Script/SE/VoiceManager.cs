using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite4Unity3d;
using Cysharp.Threading.Tasks;
using System;
using AbubuResouse.Log;
using System.Linq;

public class VoiceManager : SingletonMonoBehaviour<VoiceManager>
{
    private AudioSource audioSource;
    [SerializeField]
    private SQLiteConnection connection;

    // シングルトンパターン
    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        InitializeDatabase();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void InitializeDatabase()
    {
        var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "voice_data.db").Replace("\\", "/");
        try
        {
            connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
            DebugUtility.Log("データベース接続に成功!パス: " + databasePath);
        }
        catch (Exception ex)
        {
            DebugUtility.LogError("データベースの接続に失敗しました: " + ex.Message);
        }
    }

    /// <summary>
    /// サウンド名と音量を受け取り、dbにあれば再生させるメソッド
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    public void PlaySound(string clipName, float volume)
    {
        var query = connection.Table<VoiceClip>().FirstOrDefault(x => x.ClipName == clipName);
        if (query != null)
        {
            DebugUtility.Log("サウンドクリップが見つかりました: " + query.ClipName);
            LoadAndPlayClip(query.ClipPath, volume);
        }
        else
        {
            DebugUtility.Log("指定されたサウンドクリップ名に一致するレコードがデータベースに存在しない: " + clipName);
        }
    }

    /// <summary>
    ///ボイスを読み込んで再生するメソッド 
    /// </summary>
    /// <param name="clipPath"></param>
    /// <param name="volume"></param>
    private void LoadAndPlayClip(string clipPath, float volume)
    {
        try
        {
            AudioClip clip = Resources.Load<AudioClip>("Voice/" + clipPath);
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                audioSource.volume = volume;
            }
            else
            {
                DebugUtility.Log("サウンドファイルが見つからない: " + clipPath);
            }
        }
        catch (Exception ex)
        {
            DebugUtility.LogError("サウンドファイルのロード時にエラー発生: " + ex.Message);
        }
    }

    private class VoiceClip
    {
        [PrimaryKey, AutoIncrement] // 主キー、自動インクリメント
        public int Id { get; set; }
        public string ClipName { get; set; }  // サウンドクリップの名前
        public string ClipPath { get; set; }  // サウンドクリップファイル名
    }
}