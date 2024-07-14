using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager: MonoBehaviour
{
    [SerializeField, Header("�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToActivate;
    [SerializeField, Header("��A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToDeactivate;


    /// <summary>
    /// �V�O�i������M�����Ƃ��ɌĂт�����郁�\�b�h
    /// </summary>
    public void OnNotify()
    {
        ActivateObjects();
        DeactivateObjects();
    }

    /// <summary>
    /// �w�肵���I�u�W�F�N�g���A�N�e�B�u�ɂ��郁�\�b�h
    /// </summary>
    private void ActivateObjects()
    {
        foreach (var obj in _objectsToActivate)
        {
            obj.SetActive(true);
        }
    }

    /// <summary>
    /// �w�肵���I�u�W�F�N�g���A�N�e�B�u�ɂ��郁�\�b�h
    /// </summary>
    private void DeactivateObjects()
    {
        foreach (var obj in _objectsToDeactivate)
        {
            obj.SetActive(false);
        }
    }
}
