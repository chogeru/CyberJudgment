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
    /// �C�x���g������
    /// </summary>
    private void HandleEvent(bool condition, Collider collider)
    {
        if (!condition || !collider.CompareTag(m_DetectionTag)) return;

        ExecuteEventActions();

        OnEventTriggered?.Invoke();
    }

    /// <summary>
    /// �C�x���g�A�N�V�����̎��s
    /// </summary>
    private void ExecuteEventActions()
    {
        StopBGM();
        SetObjectsActiveState(m_ObjectsToActivate, true);
        SetObjectsActiveState(m_ObjectsToDeactivate, false);
    }

    /// <summary>
    /// BGM��~����
    /// </summary>
    private void StopBGM()
    {
        if (BGMManager.Instance == null)
        {
            DebugUtility.Log("BGMManager��������");
            return;
        }

        BGMManager.Instance.StopBGM();
        DebugUtility.Log("BGM��~");
    }

    /// <summary>
    /// �z����̃I�u�W�F�N�g�̏�Ԃ�ݒ�
    /// </summary>
    private void SetObjectsActiveState(GameObject[] objects, bool isActive)
    {
        if (objects == null || objects.Length == 0) return;

        foreach (var obj in objects.Where(obj => obj != null))
        {
            obj.SetActive(isActive);
            DebugUtility.Log($"{obj.name} �� {(isActive ? "�A�N�e�B�u" : "��A�N�e�B�u")}");
        }
    }
}
