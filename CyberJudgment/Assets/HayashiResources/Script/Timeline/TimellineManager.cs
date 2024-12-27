using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager : MonoBehaviour
{
    [SerializeField, Header("�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToActivate;

    [SerializeField, Header("���[�r�[��ɔ�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    public GameObject[] _objectsToDeactivate;

    [SerializeField, Header("���[�r�[�O�ɔ�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    private GameObject[] _objectsToRemove;

    [SerializeField, Header("�A�N�e�B�u�ɂ���Ƃ��̈ʒu")]
    private Vector3[] _activatePositions;

    [SerializeField, Header("�A�N�e�B�u�ɂ���Ƃ��̉�](EulerAngles)")]
    private Vector3[] _activateRotations;


    [SerializeField, Header("��A�N�e�B�u�ɂ���Ƃ��̈ʒu")]
    private Vector3[] _deactivatePositions;

    [SerializeField, Header("��A�N�e�B�u�ɂ���Ƃ��̉�](EulerAngles)")]
    private Vector3[] _deactivateRotations;


    [SerializeField, Header("�Đ��J�n���ɔ�A�N�e�B�u�ɂ���Ƃ��̈ʒu")]
    private Vector3[] _removePositions;

    [SerializeField, Header("�Đ��J�n���ɔ�A�N�e�B�u�ɂ���Ƃ��̉�](EulerAngles)")]
    private Vector3[] _removeRotations;

    [SerializeField, Header("�X�L�b�v����^�C�����C����PlayableDirector")]
    private PlayableDirector _playableDirector;

    private void Start()
    {
        if (_playableDirector != null)
        {
            _playableDirector.played += OnPlayableDirectorPlayed;
        }
    }

    private void OnDestroy()
    {
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
        if (Input.GetKeyDown(KeyCode.Z)
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            SkipTimeline();
        }
    }

    /// <summary>
    /// �V�O�i������M�����Ƃ��ɌĂяo����郁�\�b�h
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
        // �z�񐔂̃`�F�b�N��Y�ꂸ�Ɂi���S��j
        for (int i = 0; i < _objectsToActivate.Length; i++)
        {
            GameObject obj = _objectsToActivate[i];

            // ���W�ݒ�
            if (_activatePositions != null && i < _activatePositions.Length)
            {
                obj.transform.position = _activatePositions[i];
            }

            // ��]�ݒ�ieulerAngles�𒼐ڑ���j
            if (_activateRotations != null && i < _activateRotations.Length)
            {
                obj.transform.eulerAngles = _activateRotations[i];
            }

            // �A�N�e�B�u��
            obj.SetActive(true);
        }
    }

    /// <summary>
    /// �w�肵���I�u�W�F�N�g���A�N�e�B�u�ɂ��郁�\�b�h
    /// </summary>
    private void DeactivateObjects()
    {
        for (int i = 0; i < _objectsToDeactivate.Length; i++)
        {
            GameObject obj = _objectsToDeactivate[i];

            // ���W�ݒ�
            if (_deactivatePositions != null && i < _deactivatePositions.Length)
            {
                obj.transform.position = _deactivatePositions[i];
            }

            // ��]�ݒ�
            if (_deactivateRotations != null && i < _deactivateRotations.Length)
            {
                obj.transform.eulerAngles = _deactivateRotations[i];
            }

            // ��A�N�e�B�u��
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// �^�C�����C���Đ����ɔ�A�N�e�B�u�ɂ��郁�\�b�h 
    /// </summary>
    private void StartInactive()
    {
        for (int i = 0; i < _objectsToRemove.Length; i++)
        {
            GameObject obj = _objectsToRemove[i];

            // ���W�ݒ�
            if (_removePositions != null && i < _removePositions.Length)
            {
                obj.transform.position = _removePositions[i];
            }

            // ��]�ݒ�
            if (_removeRotations != null && i < _removeRotations.Length)
            {
                obj.transform.eulerAngles = _removeRotations[i];
            }

            // ��A�N�e�B�u��
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
