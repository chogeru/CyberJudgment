using UnityEngine;

/// <summary>
/// ボタンがクリックされたときにリンクされたUIを表示するためのクラス
/// </summary>
public class SetUI : MonoBehaviour
{
    public void OnButtonClicked(string buttonName)
    {
        UIPresenter.Instance.ShowLinkedUI(buttonName);
    }
}
