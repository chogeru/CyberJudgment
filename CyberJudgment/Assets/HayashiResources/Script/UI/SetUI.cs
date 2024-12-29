using UnityEngine;
using AbubuResouse.Singleton;
using AbubuResouse.MVP.Presenter;

/// <summary>
/// �{�^�����N���b�N���ꂽ�Ƃ��ɁA
/// �{�^���ɕR�Â���ꂽUI��\�����邽�߂̃N���X
/// </summary>
public class SetUI : MonoBehaviour
{
    public void OnButtonClicked(string buttonName)
    {
        UIPresenter.Instance.ShowLinkedUI(buttonName);
    }
}
