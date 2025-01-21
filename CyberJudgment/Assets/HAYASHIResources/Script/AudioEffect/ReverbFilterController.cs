using UnityEngine;

/// <summary>
/// Reverbフィルタを制御するクラス
/// </summary>
public class ReverbFilterController : IAudioFilterController
{
    private AudioReverbFilter filter;

    public ReverbFilterController(AudioReverbFilter filter)
    {
        this.filter = filter;
    }

    public void SetEnable(bool enable)
    {
        if (filter == null) return;
        filter.enabled = enable;
    }

    /// <summary>
    /// リバーブフィルタのプリセット設定
    /// </summary>
    /// <param name="preset">AudioReverbPreset (Hall, Room, Cave,など)</param>
    public void SetPreset(AudioReverbPreset preset = AudioReverbPreset.Concerthall)
    {
        if (filter == null) return;
        filter.reverbPreset = preset;
    }
}
