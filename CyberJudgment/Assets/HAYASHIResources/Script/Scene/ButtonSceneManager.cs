using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Singleton;
public class ButtonSceneManager : MonoBehaviour
{
   public void SetScene(string sceneName)
    {
        AbubuResouse.Singleton.SceneManager.Instance.TriggerSceneLoad(sceneName);
    }
}
