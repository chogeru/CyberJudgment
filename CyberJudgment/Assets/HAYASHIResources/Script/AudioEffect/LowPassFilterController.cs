using UnityEngine;

/// <summary>
/// LowPassフィルタを制御するクラス
/// </summary>
public class LowPassFilterController : IAudioFilterController
{
    private AudioLowPassFilter filter;

    public LowPassFilterController(AudioLowPassFilter filter)
    {
        this.filter = filter;
    }

    public void SetEnable(bool enable)
    {
        if (filter == null) return;
        filter.enabled = enable;
    }

    /// <summary>
    /// 低域通過フィルタのパラメータ設定
    /// </summary>
    /// <param name="cutoffFrequency">カットオフ周波数(10~22000)</param>
    /// <param name="resonanceQ">共鳴Q(1~10くらい)</param>
    public void SetParameters(
        float cutoffFrequency = 5000f,
        float resonanceQ = 1.0f
    )
    {
        if (filter == null) return;
        filter.cutoffFrequency = cutoffFrequency;
        filter.lowpassResonanceQ = resonanceQ;
    }
}
