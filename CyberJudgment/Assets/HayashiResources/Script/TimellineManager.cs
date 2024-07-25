using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager: MonoBehaviour
{
    [SerializeField, Header("�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToActivate;
    [SerializeField, Header("��A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToDeactivate;

    [SerializeField, Header("�X�L�b�v����^�C�����C����PlayableDirector")]
    private PlayableDirector _playableDirector;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SkipTimeline();
        }
    }

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

    /// <summary>
    /// �^�C�����C�����X�L�b�v���郁�\�b�h
    /// </summary>
    private void SkipTimeline()
    {
        if (_playableDirector != null)
        {
            _playableDirector.time = _playableDirector.playableAsset.duration;
            _playableDirector.Evaluate();
            _playableDirector.Stop();
        }
    }
}
