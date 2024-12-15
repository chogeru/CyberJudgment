using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class SceneTransitionHandler : MonoBehaviour
{
    // 遷移先のシーン名を設定
    public string sceneNameToLoad;

    // Signalが受信されたときに呼ばれるメソッド
    public void OnSceneChangeSignal()
    {
        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.LogError("シーン名が設定されていません！");
        }
    }
}
