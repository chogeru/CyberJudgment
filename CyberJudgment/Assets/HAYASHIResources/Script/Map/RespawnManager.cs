using UnityEngine;
using Cysharp.Threading.Tasks;
public class RespawnManager : MonoBehaviour
{
    [Header("�v���C���[�Q��")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [Header("���X�|�[���ݒ�")]
    [SerializeField]
    private Transform respawnPoint;

    [Header("DeathEffect�Q��")]
    [SerializeField]
    private DeathEffect deathEffect;

    [Header("���X�|�[���ҋ@����")]
    [SerializeField]
    private float respawnDelay = 6f;

    private void Awake()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += StartRespawnSequence;
        }
        else
        {
            Debug.LogError("RespawnManager��PlayerHealth�̎Q�Ƃ����蓖�Ă��Ă��܂���B");
        }

        if (deathEffect == null)
        {
            Debug.LogError("RespawnManager��DeathEffect�̎Q�Ƃ����蓖�Ă��Ă��܂���B");
        }

        if (respawnPoint == null)
        {
            Debug.LogError("RespawnManager��RespawnPoint�����蓖�Ă��Ă��܂���B");
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= StartRespawnSequence;
        }
    }

    /// <summary>
    /// ���X�|�[���V�[�P���X���J�n���܂��B
    /// </summary>
    public void StartRespawnSequence()
    {
        RespawnSequence().Forget();
    }

    /// <summary>
    /// ���X�|�[���̃V�[�P���X����
    /// </summary>
    private async UniTaskVoid RespawnSequence()
    {
        // DeathEffect�̃G�t�F�N�g���J�n
        if (deathEffect != null)
        {
            await deathEffect.StartDeathEffectAsync();
        }

        // ���X�|�[���ҋ@���Ԃ�҂�
        await UniTask.Delay(System.TimeSpan.FromSeconds(respawnDelay));

        // ���X�|�[������
        RespawnPlayer();
    }

    /// <summary>
    /// �v���C���[�����X�|�[�������܂��B
    /// </summary>
    private void RespawnPlayer()
    {
        if (playerHealth != null && respawnPoint != null)
        {
            // �v���C���[�̈ʒu�����X�|�[���n�_�ɐݒ�
            playerHealth.transform.position = respawnPoint.position;

            // �v���C���[�̏�Ԃ����Z�b�g
            playerHealth.ResetState();

            // DeathEffect�̃G�t�F�N�g�����Z�b�g
            if (deathEffect != null)
            {
                deathEffect.ResetEffect();
            }
        }
        else
        {
            Debug.LogError("RespawnPlayer�����ɕK�v�ȎQ�Ƃ����蓖�Ă��Ă��܂���B");
        }
    }
}
