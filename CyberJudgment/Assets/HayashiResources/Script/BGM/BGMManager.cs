using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;
using UnityEngine.SceneManagement;
using AbubuResouse.Log;
using System.Linq;
using Cysharp.Threading.Tasks;

public class BGMManager : SingletonMonoBehaviour<BGMManager>
{
    private AudioSource m_AudioSource;
    [SerializeField]
    private SQLiteConnection m_Connection;

    //シングルトンパターン
    protected override void Awake()
    {
        base.Awake();

        m_AudioSource = GetComponent<AudioSource>();
        InitializeDatabase();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void InitializeDatabase()
    {
        var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "bgm_data.db").Replace("\\", "/");

        try
        {
            m_Connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
            DebugUtility.Log("データベース接続に成功!パス: " + databasePath);
        }
        catch (Exception ex)
        {
            DebugUtility.LogError("データベースの接続に失敗!!: " + ex.Message);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StopBGM();

    public void StopBGM()
    {
        if (m_AudioSource.isPlaying)
        {
            m_AudioSource.Stop();
            m_AudioSource.clip = null;
            DebugUtility.Log("BGM停止");
        }
    }

    public async void PlayBGM(string bgmName, float volume)
    {
        var query = m_Connection.Table<BGM>().FirstOrDefault(x => x.BGMName == bgmName);
        if (query != null)
        {
            DebugUtility.Log("BGMデータが見つかりました: " + query.BGMName);
            await LoadAndPlayClipAsync(query.BGMFileName, volume);
        }
        else
        {
            DebugUtility.Log("指定されたBGM名に一致するレコードがデータベースに存在しない");
        }
    }

    private async UniTask LoadAndPlayClipAsync(string fileName, float volume)
    {
        try
        {
            AudioClip clip = await Resources.LoadAsync<AudioClip>("BGM/" + fileName) as AudioClip;
            if (clip != null)
            {
                m_AudioSource.clip = clip;
                m_AudioSource.volume = volume;
                m_AudioSource.Play();
                DebugUtility.Log("BGMを再生中: " + fileName);
            }
            else
            {
                DebugUtility.Log("BGMファイルが見つからない: " + fileName);
            }
        }
        catch (Exception ex)
        {
            DebugUtility.LogError("BGMファイルのロード時にエラー発生: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private class BGM
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string BGMName { get; set; }
        public string BGMFileName { get; set; }
    }
}
