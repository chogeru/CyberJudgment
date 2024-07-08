using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite4Unity3d;
using Cysharp.Threading.Tasks;
using System;

public class SEManager : MonoBehaviour
{
    public static SEManager instance;
    private AudioSource audioSource;
    [SerializeField]
    public SQLiteConnection connection;

    // シングルトンパターン
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "se_data.db").Replace("\\", "/");

        try
        {
            // データベース接続の初期化
            connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
            DebugUtility.Log("データベース接続に成功!パス: " + databasePath);
        }
        catch (Exception ex)
        {
            DebugUtility.LogError("データベースの接続に失敗しました: " + ex.Message);
        }
    }

    public void PlaySound(string clipName)
    {
        try
        {
            // データベースからクリップ名に一致するレコードを取得
            var query = connection.Table<SoundClip>().Where(x => x.ClipName == clipName).FirstOrDefault();

            if (query != null)
            {
                DebugUtility.Log("サウンドクリップが見つかりました: " + query.ClipName);

                // Resourcesフォルダからサウンドファイルをロード
                AudioClip clip = Resources.Load<AudioClip>("SE/" + query.ClipPath);

                if (clip != null)
                {
                    // サウンドを再生
                    audioSource.PlayOneShot(clip);
                }
                else
                {
                    DebugUtility.Log("サウンドファイルが見つからない " + query.ClipPath);
                }
            }
            else
            {
                DebugUtility.Log("指定されたサウンドクリップ名に一致するレコードがデータベースに存在しない: " + clipName);
            }
        }
        catch (System.Exception ex)
        {
            DebugUtility.LogError("サウンドファイルのロード時にエラー発生: " + ex.Message);
        }
    }

    // サウンドクリップ情報を保持するクラス
    class SoundClip
    {
        [PrimaryKey, AutoIncrement] // 主キー、自動インクリメント
        public int Id { get; set; }
        public string ClipName { get; set; }  // サウンドクリップの名前
        public string ClipPath { get; set; }  // サウンドクリップファイル名
    }
}