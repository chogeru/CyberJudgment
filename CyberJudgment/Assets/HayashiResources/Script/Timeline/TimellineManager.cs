using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager : MonoBehaviour
{
    [SerializeField, Header("アクティブにするオブジェクト")]
    public GameObject[] _objectsToActivate;

    [SerializeField, Header("ムービー後に非アクティブにするオブジェクト")]
    public GameObject[] _objectsToDeactivate;

    [SerializeField, Header("ムービー前に非アクティブにするオブジェクト")]
    private GameObject[] _objectsToRemove;

    [SerializeField, Header("アクティブにするときの位置")]
    private Vector3[] _activatePositions;

    [SerializeField, Header("アクティブにするときの回転(EulerAngles)")]
    private Vector3[] _activateRotations;


    [SerializeField, Header("非アクティブにするときの位置")]
    private Vector3[] _deactivatePositions;

    [SerializeField, Header("非アクティブにするときの回転(EulerAngles)")]
    private Vector3[] _deactivateRotations;


    [SerializeField, Header("再生開始時に非アクティブにするときの位置")]
    private Vector3[] _removePositions;

    [SerializeField, Header("再生開始時に非アクティブにするときの回転(EulerAngles)")]
    private Vector3[] _removeRotations;

    [SerializeField, Header("スキップするタイムラインのPlayableDirector")]
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
    /// PlayableDirectorが再生を開始したときに呼び出されるメソッド
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
    /// シグナルを受信したときに呼び出されるメソッド
    /// </summary>
    public void OnNotify()
    {
        ActivateObjects();
        DeactivateObjects();
    }

    /// <summary>
    /// 指定したオブジェクトをアクティブにするメソッド
    /// </summary>
    private void ActivateObjects()
    {
        // 配列数のチェックを忘れずに（安全策）
        for (int i = 0; i < _objectsToActivate.Length; i++)
        {
            GameObject obj = _objectsToActivate[i];

            // 座標設定
            if (_activatePositions != null && i < _activatePositions.Length)
            {
                obj.transform.position = _activatePositions[i];
            }

            // 回転設定（eulerAnglesを直接代入）
            if (_activateRotations != null && i < _activateRotations.Length)
            {
                obj.transform.eulerAngles = _activateRotations[i];
            }

            // アクティブ化
            obj.SetActive(true);
        }
    }

    /// <summary>
    /// 指定したオブジェクトを非アクティブにするメソッド
    /// </summary>
    private void DeactivateObjects()
    {
        for (int i = 0; i < _objectsToDeactivate.Length; i++)
        {
            GameObject obj = _objectsToDeactivate[i];

            // 座標設定
            if (_deactivatePositions != null && i < _deactivatePositions.Length)
            {
                obj.transform.position = _deactivatePositions[i];
            }

            // 回転設定
            if (_deactivateRotations != null && i < _deactivateRotations.Length)
            {
                obj.transform.eulerAngles = _deactivateRotations[i];
            }

            // 非アクティブ化
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// タイムライン再生時に非アクティブにするメソッド 
    /// </summary>
    private void StartInactive()
    {
        for (int i = 0; i < _objectsToRemove.Length; i++)
        {
            GameObject obj = _objectsToRemove[i];

            // 座標設定
            if (_removePositions != null && i < _removePositions.Length)
            {
                obj.transform.position = _removePositions[i];
            }

            // 回転設定
            if (_removeRotations != null && i < _removeRotations.Length)
            {
                obj.transform.eulerAngles = _removeRotations[i];
            }

            // 非アクティブ化
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// タイムラインをスキップするメソッド
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
