using UnityEngine;
using System.Collections.Generic;
using AbubuResouse.Singleton;

public class ObjectDestroyHandler : MonoBehaviour
{
    [Header("破棄時にアクティブ化するオブジェクト")]
    public List<GameObject> objectsToActivate;

    [Header("破棄時に非アクティブ化するオブジェクト")]
    public List<GameObject> objectsToDeactivate;

    private void OnDestroy()
    {
        // アクティブ化するオブジェクトを有効化
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"{obj.name} をアクティブ化しました。");
            }
            else
            {
                Debug.LogWarning("アクティブ化対象のオブジェクトが参照されていません。");
            }
        }

        // 非アクティブ化するオブジェクトを無効化
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"{obj.name} を非アクティブ化しました。");
            }
            else
            {
                Debug.LogWarning("非アクティブ化対象のオブジェクトが参照されていません。");
            }
        }
        BossUIManager.Instance.ResetBossUI();
    }
}
