using UnityEngine;

/// <summary>
/// Echoフィルタを制御するクラス
/// </summary>
public class EchoFilterController : IAudioFilterController
{
    private AudioEchoFilter filter;

    public EchoFilterController(AudioEchoFilter filter)
    {
        this.filter = filter;
    }

    public void SetEnable(bool enable)
    {
        if (filter == null) return;
        filter.enabled = enable;
    }

    /// <summary>
    /// Echo フィルタのパラメータ設定
    /// </summary>
    /// <param name="delay">ディレイ(ms)</param>
    /// <param name="decayRatio">減衰率(0~1)</param>
    /// <param name="dryMix">原音レベル</param>
    /// <param name="wetMix">エコーレベル</param>
    public void SetParameters(
        float delay = 500f,
        float decayRatio = 0.5f,
        float dryMix = 1.0f,
        float wetMix = 1.0f
    )
    {
        if (filter == null) return;
        filter.delay = delay;
        filter.decayRatio = decayRatio;
        filter.dryMix = dryMix;
        filter.wetMix = wetMix;
    }
}
