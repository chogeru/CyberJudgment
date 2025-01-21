using UnityEngine;

/// <summary>
/// Chorusフィルタを制御するクラス
/// </summary>
public class ChorusFilterController : IAudioFilterController
{
    private AudioChorusFilter filter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="filter">AudioChorusFilter コンポーネント</param>
    public ChorusFilterController(AudioChorusFilter filter)
    {
        this.filter = filter;
    }

    /// <summary>
    /// フィルタのON/OFF
    /// </summary>
    /// <param name="enable">true: 有効, false: 無効</param>
    public void SetEnable(bool enable)
    {
        if (filter == null) return;
        filter.enabled = enable;
    }

    /// <summary>
    /// Chorus フィルタのパラメータ設定
    /// </summary>
    /// <param name="dryMix">原音の割合</param>
    /// <param name="wetMix1">コーラス音1の割合</param>
    /// <param name="wetMix2">コーラス音2の割合</param>
    /// <param name="delay">遅延時間 (ms)</param>
    /// <param name="rate">モジュレーション速度</param>
    /// <param name="depth">モジュレーションの深さ</param>
    /// <param name="feedback">フィードバック量</param>
    public void SetParameters(
        float dryMix = 0.5f,
        float wetMix1 = 0.5f,
        float wetMix2 = 0.5f,
        float delay = 40f,
        float rate = 0.8f,
        float depth = 0.03f,
        float feedback = 0.0f
    )
    {
        if (filter == null) return;

        filter.dryMix = dryMix;
        filter.wetMix1 = wetMix1;
        filter.wetMix2 = wetMix2;
        filter.delay = delay;
        filter.rate = rate;
        filter.depth = depth;
        filter.feedback = feedback;
    }
}
