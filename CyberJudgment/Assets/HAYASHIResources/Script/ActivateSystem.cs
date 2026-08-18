using UnityEngine;
using AbubuResouse.Singleton;
using System.Linq;
using AbubuResouse.Log;

public class ActivateSystem : MonoBehaviour
{
    [SerializeField] 
    private string m_DetectionTag = "Player";
    [SerializeField] 
    private bool isUseCollisionDetection = false;
    [SerializeField]
    private bool isUseTriggerDetection = true;
    [SerializeField]
    private bool isEnableOnEnter = true;
    [SerializeField] 
    private bool isEnableOnStay = false;
    [SerializeField] 
    private bool isEnableOnExit = true;

    [SerializeField] 
    private GameObject[] m_ObjectsToActivate = null;
    [SerializeField]
    private GameObject[] m_ObjectsToDeactivate = null;


    public event System.Action OnEventTriggered;

    private void OnCollisionEnter(Collision collision) => HandleEvent(isUseCollisionDetection && isEnableOnEnter, collision.collider);
    private void OnCollisionStay(Collision collision) => HandleEvent(isUseCollisionDetection && isEnableOnStay, collision.collider);
    private void OnCollisionExit(Collision collision) => HandleEvent(isUseCollisionDetection && isEnableOnExit, collision.collider);

    private void OnTriggerEnter(Collider other) => HandleEvent(isUseTriggerDetection && isEnableOnEnter, other);
    private void OnTriggerStay(Collider other) => HandleEvent(isUseTriggerDetection && isEnableOnStay, other);
    private void OnTriggerExit(Collider other) => HandleEvent(isUseTriggerDetection && isEnableOnExit, other);

    /// <summary>
    /// イベントを処理
    /// </summary>
    private void HandleEvent(bool condition, Collider collider)
    {
        if (!condition || !collider.CompareTag(m_DetectionTag)) return;

        ExecuteEventActions();

        OnEventTriggered?.Invoke();
    }

    /// <summary>
    /// イベントアクションの実行
    /// </summary>
    private void ExecuteEventActions()
    {
        StopBGM();
        SetObjectsActiveState(m_ObjectsToActivate, true);
        SetObjectsActiveState(m_ObjectsToDeactivate, false);
    }

    /// <summary>
    /// BGM停止処理
    /// </summary>
    private void StopBGM()
    {
        if (BGMManager.Instance == null)
        {
            DebugUtility.Log("BGMManagerが無いよ");
            return;
        }

        BGMManager.Instance.StopBGM();
        DebugUtility.Log("BGM停止");
    }

    /// <summary>
    /// 配列内のオブジェクトの状態を設定
    /// </summary>
    private void SetObjectsActiveState(GameObject[] objects, bool isActive)
    {
        if (objects == null || objects.Length == 0) return;

        foreach (var obj in objects.Where(obj => obj != null))
        {
            obj.SetActive(isActive);
            DebugUtility.Log($"{obj.name} は {(isActive ? "アクティブ" : "非アクティブ")}");
        }
    }
}
