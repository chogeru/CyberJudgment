using AbubuResouse.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager: MonoBehaviour
{
    [SerializeField, Header("アクティブにするオブジェクト")]
    public GameObject[] _objectsToActivate;
    [SerializeField, Header("ムービー後に非アクティブにするオブジェクト")]
    public GameObject[] _objectsToDeactivate;
    [SerializeField,Header("ムービー前に非アクティブにするオブジェクト")]
    private GameObject[] _objectsToRemove;

    [SerializeField, Header("スキップするタイムラインのPlayableDirector")]
    private PlayableDirector _playableDirector;

    private void Start()
    {
        // PlayableDirectorの再生開始イベントにリスナーを追加
        if (_playableDirector != null)
        {
            _playableDirector.played += OnPlayableDirectorPlayed;
        }
    }

    private void OnDestroy()
    {
        // PlayableDirectorの再生開始イベントのリスナーを削除
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
        if (Input.GetKeyDown(KeyCode.Z) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            SkipTimeline();
        }
    }

    /// <summary>
    /// シグナルを受信したときに呼びだされるメソッド
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
        foreach (var obj in _objectsToActivate)
        {
            obj.SetActive(true);
        }
    }

    /// <summary>
    /// 指定したオブジェクトを非アクティブにするメソッド
    /// </summary>
    private void DeactivateObjects()
    {
        foreach (var obj in _objectsToDeactivate)
        {
            obj.SetActive(false);
        }
    }

    /// <summary>
    ///タイムライン再生時に非アクティブにするメソッド 
    /// </summary>
   private void StartInactive()
    {
        foreach(var obj in _objectsToRemove)
        {
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
