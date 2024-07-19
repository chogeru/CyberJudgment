using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite4Unity3d;
using Cysharp.Threading.Tasks;
using System;
using AbubuResouse.Log;

public class SceneManager : SingletonMonoBehaviour<SceneManager>
{
    private SQLiteConnection connection;
    private string nextScene;
    private bool isSceneLoading = false;

    protected override void Awake()
    {
        base.Awake();
        var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "scene_data.db").Replace("\\", "/");
        connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
    }

    public void TriggerSceneLoad(string currentSceneName)
    {
        LoadNextSceneAsync(currentSceneName);
    }

    private async void LoadNextSceneAsync(string currentSceneName)
    {
        if (isSceneLoading)
        {
            DebugUtility.LogWarning("シーンがすでにロード中");
            return;
        }
        isSceneLoading = true;
        nextScene = GetNextSceneNameFromDB(currentSceneName);
        if (!string.IsNullOrEmpty(nextScene))
        {
            await SceneTransitionAsync(nextScene);
        }
        else
        {
            DebugUtility.LogError("次のシーン名が設定されていない");
        }
        isSceneLoading = false;
    }
    private string GetNextSceneNameFromDB(string currentSceneName)
    {
        try
        {
            var query = connection.Table<SceneTransition>().Where(x => x.CurrentScene == currentSceneName).FirstOrDefault();
            if (query != null)
            {
                return query.NextScene;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("" + ex.Message);
        }
        return null;
    }

    private async UniTask SceneTransitionAsync(string sceneName)
    {
        // シーンの非同期読み込みを開始
        var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        // シーンの読み込みが完了するまで待機
        while (!asyncOperation.isDone)
        {
            await UniTask.Yield();
        }
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    class SceneTransition
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CurrentScene { get; set; }
        public string NextScene { get; set; }
    }
}
