using UnityEngine;

/// <summary>
/// ボタンがクリックされたときにリンクされたUIを表示するためのクラス
/// </summary>
public class SetUI : MonoBehaviour
{
    public void OnButtonClicked(int index)
    {
        UIPresenter.Instance.ShowLinkedUI(index);
    }
}
