using AbubuResouse.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager: MonoBehaviour
{
    [SerializeField, Header("�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToActivate;
    [SerializeField, Header("���[�r�[��ɔ�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToDeactivate;
    [SerializeField,Header("���[�r�[�O�ɔ�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    private GameObject[] _objectsToRemove;

    [SerializeField, Header("�X�L�b�v����^�C�����C����PlayableDirector")]
    private PlayableDirector _playableDirector;

    private void Start()
    {
        // PlayableDirector�̍Đ��J�n�C�x���g�Ƀ��X�i�[��ǉ�
        if (_playableDirector != null)
        {
            _playableDirector.played += OnPlayableDirectorPlayed;
        }
    }

    private void OnDestroy()
    {
        // PlayableDirector�̍Đ��J�n�C�x���g�̃��X�i�[���폜
        if (_playableDirector != null)
        {
            _playableDirector.played -= OnPlayableDirectorPlayed;
        }
    }

    /// <summary>
    /// PlayableDirector���Đ����J�n�����Ƃ��ɌĂяo����郁�\�b�h
    /// </summary>
    /// <param name="director"></param>
    private void OnPlayableDirectorPlayed(PlayableDirector director)
    {
        StartInactive();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
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
    ///�^�C�����C���Đ����ɔ�A�N�e�B�u�ɂ��郁�\�b�h 
    /// </summary>
   private void StartInactive()
    {
        foreach(var obj in _objectsToRemove)
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
