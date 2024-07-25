using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimellineManager: MonoBehaviour
{
    [SerializeField, Header("アクティブにするオブジェクト")]
    public GameObject[] _objectsToActivate;
    [SerializeField, Header("非アクティブにするオブジェクト")]
    public GameObject[] _objectsToDeactivate;

    [SerializeField, Header("スキップするタイムラインのPlayableDirector")]
    private PlayableDirector _playableDirector;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
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
