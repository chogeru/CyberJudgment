using UnityEngine;

/// <summary>
/// ボタンがクリックされたときに、
/// ボタンに紐づけられたUIを表示するためのクラス
/// </summary>
public class SetUI : MonoBehaviour
{
    public void OnButtonClicked(string buttonName)
    {
        UIPresenter.Instance.ShowLinkedUI(buttonName);
    }
}
