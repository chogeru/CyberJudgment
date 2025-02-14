using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Singleton;
using UnityEngine.Playables;
public class ButtonSceneManager : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector timelineDirector;
    private string targetSceneName;
    public void SetScene(string sceneName)
    {
        targetSceneName = sceneName;
        if (timelineDirector != null)
        {
            timelineDirector.Play();

            timelineDirector.stopped += OnTimelineStopped;
        }
        else
        {
            LoadScene();
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        LoadScene();

        timelineDirector.stopped -= OnTimelineStopped;
    }

    private void LoadScene()
    {
        AbubuResouse.Singleton.SceneManager.Instance.TriggerSceneLoad(targetSceneName);
    }
}
