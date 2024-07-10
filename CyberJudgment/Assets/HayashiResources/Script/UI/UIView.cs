using UnityEngine;
using VInspector;

/// <summary>
/// UI�r���[�N���X�A���j���[�Ɛݒ��ʂ̕\����Ԃ��Ǘ�����
/// </summary>
public class UIView : MonoBehaviour
{
    [Tab("�I�u�W�F�N�g")]
    [SerializeField, Header("���j���[���")]
    private GameObject m_MenuUI;
    [SerializeField, Header("�ݒ���")]
    private GameObject m_SettingUI;
    [SerializeField, Header("�R�Â���ꂽUI�I�u�W�F�N�g")]
    private GameObject[] m_LinkedUIObjects;
    public GameObject[] LinkedUIObjects => m_LinkedUIObjects;
    [EndTab]

    /// <summary>
    /// ���j���[��ʂ̕\��/��\����ݒ肷��
    /// </summary>
   �@/// <param name="isVisible">�\������ꍇ��true�B</param>
    public void SetMenuVisibility(bool isVisible)
    {
        m_MenuUI.SetActive(isVisible);
    }

    /// <summary>
    /// �ݒ��ʂ̕\��/��\����ݒ肷��
    /// </summary>
    /// <param name="isVisible">�\������ꍇ��true�B</param>
    public void SetSettingVisibility(bool isVisible)
    {
        m_SettingUI.SetActive(isVisible);
    }

    /// �w�肳�ꂽ�����N���ꂽUI�I�u�W�F�N�g�̕\��/��\����ݒ肷��
    /// </summary>
    /// <param name="index">�����N���ꂽUI�I�u�W�F�N�g�̃C���f�b�N�X�B</param>
    /// <param name="isVisible">�\������ꍇ��true�B</param>
    public void SetLinkedUIVisibility(int index, bool isVisible)
    {
        if (index >= 0 && index < m_LinkedUIObjects.Length)
        {
            m_LinkedUIObjects[index].SetActive(isVisible);
        }
    }

    /// <summary>
    /// �J�[�\���̕\��/��\����ݒ肷��
    /// </summary>
    /// <param name="isVisible">�\������ꍇ��true�B</param>
    public void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

}
