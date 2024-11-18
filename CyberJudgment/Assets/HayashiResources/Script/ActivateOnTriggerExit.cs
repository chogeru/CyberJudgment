using UnityEngine;
using AbubuResouse.Singleton;

public class ActivateOnTriggerExit : MonoBehaviour
{
    // �A�N�e�B�u�ɂ������I�u�W�F�N�g
    [SerializeField] private GameObject objectToActivate;

    [SerializeField] private GameObject objectToDeactivate;
    [SerializeField] private GameObject objectToDeactivate2;
    private void OnTriggerEnter(Collider other)
    {
        // ���蔲�����I�u�W�F�N�g���uPlayer�v�^�O���`�F�b�N
        if (other.CompareTag("Player"))
        {
            // �I�u�W�F�N�g���A�N�e�B�u�ɂ���
            if (objectToActivate != null)
            {
                BGMManager.Instance.StopBGM();
                objectToActivate.SetActive(true); 
                objectToDeactivate.SetActive(false);
                objectToDeactivate2.SetActive(false);
                Debug.Log("�I�u�W�F�N�g���A�N�e�B�u�ɂȂ�܂���: " + objectToActivate.name);
            }
            else
            {
                Debug.LogWarning("�A�N�e�B�u�ɂ���I�u�W�F�N�g���ݒ肳��Ă��܂���I");
            }
        }
    }
}
