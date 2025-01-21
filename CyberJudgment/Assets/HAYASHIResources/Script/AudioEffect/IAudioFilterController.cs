/// <summary>
/// 任意のフィルタを ON / OFF ＆ パラメータ適用できる抽象インターフェース
/// </summary>
public interface IAudioFilterController
{
    /// <summary>
    /// フィルタを有効化または無効化する
    /// </summary>
    void SetEnable(bool enable);
}
