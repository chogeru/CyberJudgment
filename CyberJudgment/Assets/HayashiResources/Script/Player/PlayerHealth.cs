using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VInspector;
using AbubuResouse.Singleton;

public class PlayerHealth : MonoBehaviour
{
    [Header("�A�j���[�^�[")]
    [SerializeField]
    private Animator animator;

    [Header("�̗͐ݒ�")]
    [SerializeField]
    private int maxHealth = 100;

    [Header("UI")]
    [SerializeField]
    private Slider healthSlider;


    [Tab("����")]
    [Foldout("�����ݒ�")]
    [SerializeField, Header("��e�{�C�X")]
    private string[] hitVoices;
    [SerializeField, Header("���S�{�C�X")]
    private string[] deathVoices;
    [Range(0f, 1f)]
    [SerializeField, Header("����")]
    private float volume = 1f;
    [EndFoldout]
    [EndTab]

    private int currentHealth;
    private bool isDead = false;
    private bool isHit = false;

    public Action<int> OnHealthChanged;
    public Action OnPlayerDeath; // �v���C���[���S���̃C�x���g

    private PlayerManager playerManager;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerManager = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        InitializeHealthUI();

        // OnHealthChanged �C�x���g�ɑ΂���w�ǂ��s���A���N�o�[���X�V
        OnHealthChanged += UpdateHealthUI;
    }

    private void OnDestroy()
    {
        // �C�x���g�̍w�ǉ���
        OnHealthChanged -= UpdateHealthUI;
    }

    /// <summary>
    /// �̗͂�����������
    /// </summary>
    private void InitializeHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    /// <summary>
    /// �_���[�W���󂯂�
    /// </summary>
    /// <param name="damageAmount">�_���[�W��</param>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        int newHealth = Mathf.Clamp(currentHealth - damageAmount, 0, maxHealth);
        currentHealth = newHealth;
        OnHealthChanged?.Invoke(currentHealth);

        if (newHealth > 0)
        {
            HandleHit().Forget();
        }
        else
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// �̗͂��񕜂���
    /// </summary>
    /// <param name="healAmount">�񕜗�</param>
    public void Heal(int healAmount)
    {
        if (isDead) return;

        int newHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
        currentHealth = newHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// ��e���̏���
    /// </summary>
    private async UniTaskVoid HandleHit()
    {
        if (isHit || IsAnimationPlaying("GetHit")) return;

        isHit = true;
        playerManager.SetHitState(true);

        PlayRandomSound(hitVoices);
        animator.SetBool("GetHit", true);

        // ��莞�Ԍ�ɔ�e�X�e�[�g������
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        isHit = false;
        playerManager.SetHitState(false);
        animator.SetBool("GetHit", false);
    }

    /// <summary>
    /// ���S���̏���
    /// </summary>
    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        playerManager.SetDeadState(true);

        PlayRandomSound(deathVoices);
        animator.CrossFade("Die", 0.5f);

        OnPlayerDeath?.Invoke(); // ���S��ʒm
    }

    /// <summary>
    /// �w�肵���A�j���[�V�������Đ������ǂ����m�F
    /// </summary>
    private bool IsAnimationPlaying(string animationName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) || animator.IsInTransition(0);
    }

    /// <summary>
    /// �����_���ȉ������Đ�
    /// </summary>
    /// <param name="audioClips">�����N���b�v�̔z��</param>
    private void PlayRandomSound(string[] audioClips)
    {
        if (audioClips == null || audioClips.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);
        string clip = audioClips[randomIndex];
        VoiceManager.Instance.PlaySound(clip, volume);
    }

    /// <summary>
    /// �v���C���[�̏�Ԃ����Z�b�g���܂��B���X�|�[�����ɌĂяo���܂��B
    /// </summary>
    public void ResetState()
    {
        isDead = false;
        currentHealth = maxHealth;
        InitializeHealthUI();
        playerManager.SetDeadState(false);
        animator.CrossFade("Idle", 0.03f);
    }

    /// <summary>
    /// ���N�o�[���X�V���܂��B
    /// </summary>
    /// <param name="newHealth">�V�������N�l</param>
    private void UpdateHealthUI(int newHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = newHealth;
        }
    }

    public void HpRecovery(int hpRecovery)
    {
        currentHealth += hpRecovery;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI(currentHealth);
    }
}
