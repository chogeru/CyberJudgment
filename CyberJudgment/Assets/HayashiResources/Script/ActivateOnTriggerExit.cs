using UnityEngine;
using AbubuResouse.Singleton;

public class ActivateOnTriggerExit : MonoBehaviour
{
    // アクティブにしたいオブジェクト
    [SerializeField] private GameObject objectToActivate;

    [SerializeField] private GameObject objectToDeactivate;
    [SerializeField] private GameObject objectToDeactivate2;
    private void OnTriggerEnter(Collider other)
    {
        // すり抜けたオブジェクトが「Player」タグかチェック
        if (other.CompareTag("Player"))
        {
            // オブジェクトをアクティブにする
            if (objectToActivate != null)
            {
                BGMManager.Instance.StopBGM();
                objectToActivate.SetActive(true); 
                objectToDeactivate.SetActive(false);
                objectToDeactivate2.SetActive(false);
                Debug.Log("オブジェクトがアクティブになりました: " + objectToActivate.name);
            }
            else
            {
                Debug.LogWarning("アクティブにするオブジェクトが設定されていません！");
            }
        }
    }
}
