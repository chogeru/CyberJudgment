using R3;

/// <summary>
/// UIモデルクラス、メニューの開閉状態を管理する
/// </summary>
public class UIModel
{
    // メニューが開いているかどうかのReactiveProperty、初期値はfalse
    public ReactiveProperty<bool> IsMenuOpen { get; } = new ReactiveProperty<bool>(false);
    // 現在表示されているUIオブジェクトの名前のReactiveProperty、初期値は空文字列
    public ReactiveProperty<string> CurrentUIObject { get; } = new ReactiveProperty<string>("");
    // カーソルが表示されているかどうかを表すReactiveProperty、初期値はfalse
    public ReactiveProperty<bool> IsCursorVisible { get; } = new ReactiveProperty<bool>(false);
}