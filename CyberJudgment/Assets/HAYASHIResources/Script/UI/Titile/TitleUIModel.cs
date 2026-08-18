using UniRx;
using UnityEngine;

public class TitleUIModel
{
    public ReactiveProperty<int> SelectedButtonIndex { get; } = new ReactiveProperty<int>(0); // 新しいプロパティ
}

