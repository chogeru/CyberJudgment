using UnityEngine;

/// <summary>
/// �{�^�����N���b�N���ꂽ�Ƃ��Ƀ����N���ꂽUI��\�����邽�߂̃N���X
/// </summary>
public class SetUI : MonoBehaviour
{
    public void OnButtonClicked(int index)
    {
        UIPresenter.Instance.ShowLinkedUI(index);
    }
}
