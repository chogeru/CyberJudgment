using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SQLite4Unity3d;
using UnityEngine.SceneManagement;
using AbubuResouse.Log;

public class BGMManager : SingletonMonoBehaviour<BGMManager>
{
    public static BGMManager instance;

    private AudioSource m_AudioSource;
    [SerializeField]
    public SQLiteConnection m_Connection;
    //シングルトンパターン
    protected override void Awake()
    {
        base.Awake();

        m_AudioSource = GetComponent<AudioSource>();
        //データベースのパスを設定
        var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "bgm_data.db").Replace("\\", "/");

        try
        {
            //データベース接続の初期化
            m_Connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
            DebugUtility.Log("データベース接続に成功!パス: " + databasePath); 
        }
        catch (Exception ex)
        {
            DebugUtility.LogError("データベースの接続に失敗!!: " + ex.Message); 
        }
    }

    public void PlayBGM(string bgmName ,float volume)
    {
        //データベースからBGM名に一致するレコードを所得
        var query = m_Connection.Table<BGM>().Where(x => x.BGMName == bgmName).FirstOrDefault();
        //クエリ結果がnullでなければ
        if (query != null)
        {

            DebugUtility.Log("BGMデータが見つかりました: " + query.BGMName);
            try
            {
                //ResourcesフォルダからBGMファイルをロード
                AudioClip clip = Resources.Load<AudioClip>("BGM/" + query.BGMFileName);

                //クリップがnullでない場合
                if (clip != null)
                {
                    //クリップを設定
                    m_AudioSource.clip = clip;
                    //音量も設定
                    m_AudioSource.volume = volume;
                    //再生
                    m_AudioSource.Play();
                }
                else
                {
                    DebugUtility.Log("BGMファイルが見つからない " + query.BGMFileName);
                }
            }
            //例外発生時
            catch(Exception ex)
            {
                DebugUtility.LogError("BGMファイルのロード時にエラー発生"+ex.Message);
            }
        }
        else
        {
            DebugUtility.Log("指定されたBGM名に一致するレコードがデータベースに存在しない"); 
        }
    }
    // BGM情報を保持するクラス
    class BGM
    {
        [PrimaryKey, AutoIncrement] // 主キー、自動インクリメント
        public int Id { get; set; }
        public string BGMName { get; set; }  // BGMの名前
        public string BGMFileName { get; set; } // BGMファイル名
    }
}
