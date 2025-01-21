using UnityEngine;

/// <summary>
/// Distortionフィルタを制御するクラス
/// </summary>
public class DistortionFilterController : IAudioFilterController
{
    private AudioDistortionFilter filter;

    public DistortionFilterController(AudioDistortionFilter filter)
    {
        this.filter = filter;
    }

    public void SetEnable(bool enable)
    {
        if (filter == null) return;
        filter.enabled = enable;
    }

    /// <summary>
    /// Distortion フィルタのパラメータ設定
    /// </summary>
    /// <param name="distortionLevel">歪みの強さ(0~1)</param>
    public void SetParameters(float distortionLevel = 0.5f)
    {
        if (filter == null) return;
        filter.distortionLevel = distortionLevel;
    }
}
