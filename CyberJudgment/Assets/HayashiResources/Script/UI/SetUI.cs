using UnityEngine;

/// <summary>
/// �{�^�����N���b�N���ꂽ�Ƃ��Ƀ����N���ꂽUI��\�����邽�߂̃N���X
/// </summary>
public class SetUI : MonoBehaviour
{
    public void OnButtonClicked(string buttonName)
    {
        UIPresenter.Instance.ShowLinkedUI(buttonName);
    }
}
