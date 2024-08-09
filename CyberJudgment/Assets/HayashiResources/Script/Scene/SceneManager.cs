using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite4Unity3d;
using Cysharp.Threading.Tasks;
using System;
using AbubuResouse.Log;
using UnityEngine.UI;
using TMPro;

namespace AbubuResouse.Singleton
{
    public class SceneManager : SingletonMonoBehaviour<SceneManager>
    {
        private SQLiteConnection _connection;
        private string _nextScene;
        private bool isSceneLoading = false;

        [SerializeField, Header("ロード時のキャンバス")]
        private GameObject _loadingCanvas;
        [SerializeField, Header("ロード進捗バー")]
        private Image _loadingBar;
        [SerializeField, Header("ロード時に差し替える画像")]
        private Image _loadingSprite;
        [SerializeField, Header("TipsのText")]
        private TMP_Text _loadingDescription;
        [SerializeField, Header("Mapの名前Text")]
        private TMP_Text _mapNameText;
        [SerializeField, Header("各シーンのデータ")]
        private List<SceneLoadData> _sceneLoadDataList;

        protected override void Awake()
        {
            base.Awake();
            var databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "scene_data.db").Replace("\\", "/");
            _connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
            _loadingCanvas.SetActive(false);
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
            // 次のシーン名を非同期で取得
            _nextScene = await GetNextSceneNameFromDBAsync(currentSceneName);

            if (!string.IsNullOrEmpty(_nextScene))
            {
                UpdateLoadingScreen(_nextScene);
                _loadingCanvas.SetActive(true);
                await SceneTransitionAsync(_nextScene);
                _loadingCanvas.SetActive(false);
            }
            else
            {
                DebugUtility.LogError("次のシーン名が設定されていない");
            }
            isSceneLoading = false;
        }
        private async UniTask<string> GetNextSceneNameFromDBAsync(string currentSceneName)
        {
            try
            {
                return await UniTask.RunOnThreadPool(() =>
                {
                    var query = _connection.Table<SceneTransition>()
                                           .Where(x => x.CurrentScene == currentSceneName)
                                           .FirstOrDefault();
                    return query?.NextScene;
                });
            }
            catch (System.Exception ex)
            {
                DebugUtility.LogError(ex.Message);
            }

            return null;
        }


        private void UpdateLoadingScreen(string sceneName)
        {
            var sceneData = _sceneLoadDataList.Find(data => data.sceneName == sceneName);
            if (sceneData != null)
            {
                _loadingSprite.sprite = sceneData.loadingSprite;
                _loadingDescription.text = sceneData.loadingDescription;
                _mapNameText.text = sceneData.mapName;
            }
        }

        /// <summary>
        /// シーン読み込み用関数
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public async UniTask SceneTransitionAsync(string sceneName)
        {
            _loadingBar.fillAmount = 0;
            // シーンの非同期読み込みを開始
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName); // シーンの読み込みが完了するまで待機
            while (!asyncOperation.isDone)
            {
                _loadingBar.fillAmount = asyncOperation.progress;
                await UniTask.Yield();
            }
            _loadingBar.fillAmount = 1f;
        }
        public void ExitGame()
        {
            Application.Quit();
        }

        [Serializable]
        public class SceneLoadData
        {
            public string sceneName;
            public Sprite loadingSprite;
            [TextArea(3, 10)]
            public string loadingDescription;
            public string mapName;
        }

        class SceneTransition
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string CurrentScene { get; set; }
            public string NextScene { get; set; }
        }
    }
}