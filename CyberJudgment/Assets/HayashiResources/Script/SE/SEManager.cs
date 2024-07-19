using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite4Unity3d;
using Cysharp.Threading.Tasks;
using System;
using AbubuResouse.Log;
using System.Linq;

public class SEManager : SingletonMonoBehaviour<SEManager>
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

    private void InitializeDatabase()
    {
        var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "se_data.db").Replace("\\", "/");
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

    public void PlaySound(string clipName, float volume)
    {
        var query = connection.Table<SoundClip>().FirstOrDefault(x => x.ClipName == clipName);
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

    private void LoadAndPlayClip(string clipPath, float volume)
    {
        try
        {
            AudioClip clip = Resources.Load<AudioClip>("SE/" + clipPath);
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

    private class SoundClip
    {
        [PrimaryKey, AutoIncrement] // 主キー、自動インクリメント
        public int Id { get; set; }
        public string ClipName { get; set; }  // サウンドクリップの名前
        public string ClipPath { get; set; }  // サウンドクリップファイル名
    }
}