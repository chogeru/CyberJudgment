using UnityEngine;

/// <summary>
/// �����N���ꂽUI�̖߂�{�^�����N���b�N���ꂽ�Ƃ��ɐݒ��ʂ�\�����邽�߂̃N���X
/// </summary>
public class LinkedUI : MonoBehaviour
{
    public void OnBackButtonClicked()
    {
        UIPresenter.Instance.ShowSettingUI();
    }
}
