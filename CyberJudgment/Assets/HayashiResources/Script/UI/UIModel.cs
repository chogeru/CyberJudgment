using R3;
using UnityEngine;

/// <summary>
/// UIモデルクラス。メニューの開閉状態を管理する。
/// </summary>
public class UIModel
{
    public ReactiveProperty<bool> IsMenuOpen { get; } = new ReactiveProperty<bool>(false);
}