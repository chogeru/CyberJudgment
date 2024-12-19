using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    [Header("RespawnManager�̎Q��")]
    [SerializeField]
    private RespawnManager respawnManager;

    private void OnTriggerEnter(Collider other)
    {
        // �v���C���[�ƏՓ˂����ꍇ�Ƀ��X�|�[���V�[�P���X���J�n
        if (respawnManager != null && other.CompareTag("Player"))
        {
            Debug.Log("���X�|�[���g���K�[���������܂����B");
            respawnManager.StartRespawnSequence();
        }
    }
}
