using UnityEngine;

/// <summary>
/// リンクされたUIの戻るボタンがクリックされたときに設定画面を表示するためのクラス
/// </summary>
public class LinkedUI : MonoBehaviour
{
    public void OnBackButtonClicked()
    {
        UIPresenter.Instance.ShowSettingUI();
    }
}
