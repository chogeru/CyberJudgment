using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VInspector;
using AbubuResouse.Singleton;

public class PlayerHealth : MonoBehaviour
{
    [Header("アニメーター")]
    [SerializeField]
    private Animator animator;

    [Header("体力設定")]
    [SerializeField]
    private int maxHealth = 100;

    [Header("UI")]
    [SerializeField]
    private Slider healthSlider;

    [Tab("音声")]
    [Foldout("音声設定")]
    [SerializeField, Header("被弾ボイス")]
    private string[] hitVoices;
    [SerializeField, Header("死亡ボイス")]
    private string[] deathVoices;
    [Range(0f, 1f)]
    [SerializeField, Header("音量")]
    private float volume = 1f;
    [EndFoldout]
    [EndTab]

    [Tab("シールド被弾")]
    [Foldout("シールド被弾設定")]
    [SerializeField, Header("シールド被弾エフェクト")]
    private GameObject shieldHitEffectPrefab;
    [SerializeField, Header("シールド被弾音")]
    private string[] shieldHitSounds;
    [Range(0f, 1f)]
    [SerializeField, Header("シールド被弾音量")]
    private float shieldHitVolume = 0.8f;
    [EndFoldout]
    [EndTab]

    private int currentHealth;
    private bool isDead = false;
    private bool isHit = false;

    public Action<int> OnHealthChanged;
    public Action OnPlayerDeath; // プレイヤー死亡時のイベント
    public Action OnShieldHit; // シールド被弾時のイベント

    private PlayerManager playerManager;
    private PlayerController playerController;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerManager = GetComponent<PlayerManager>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        InitializeHealthUI();

        // OnHealthChanged イベントに対する購読を行い、健康バーを更新
        OnHealthChanged += UpdateHealthUI;
    }

    private void OnDestroy()
    {
        // イベントの購読解除
        OnHealthChanged -= UpdateHealthUI;
    }

    /// <summary>
    /// 体力を初期化する
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
    /// ダメージを受ける
    /// </summary>
    /// <param name="damageAmount">ダメージ量</param>
    /// <param name="hitPosition">攻撃を受けた位置（シールドエフェクト用）</param>
    public void TakeDamage(int damageAmount, Vector3 hitPosition = default)
    {
        if (isDead) return;

        // シールド展開中の処理
        if (playerManager.IsGuarding)
        {
            HandleShieldHit(hitPosition).Forget();
            return;
        }

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
    /// シールド被弾時の処理
    /// </summary>
    /// <param name="hitPosition">攻撃を受けた位置</param>
    private async UniTaskVoid HandleShieldHit(Vector3 hitPosition)
    {
        Debug.Log("シールドで攻撃をブロック！");

        // シールド被弾イベントを発火
        OnShieldHit?.Invoke();

        // PlayerControllerのシールド被弾処理を呼び出し
        if (playerController != null)
        {
            playerController.TriggerShieldHit(hitPosition);
        }

        // シールド被弾音を再生
        PlayShieldHitSound();

        // シールド被弾エフェクトを再生
        PlayShieldHitEffect(hitPosition);

        await UniTask.Yield();
    }

    /// <summary>
    /// シールド被弾音を再生
    /// </summary>
    private void PlayShieldHitSound()
    {
        if (shieldHitSounds == null || shieldHitSounds.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, shieldHitSounds.Length);
        string clip = shieldHitSounds[randomIndex];

        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySound(clip, shieldHitVolume);
        }
    }

    /// <summary>
    /// シールド被弾エフェクトを再生
    /// </summary>
    /// <param name="hitPosition">攻撃を受けた位置</param>
    private void PlayShieldHitEffect(Vector3 hitPosition)
    {
        if (shieldHitEffectPrefab == null) return;

        // 攻撃位置が指定されていない場合はプレイヤーの前方に設定
        Vector3 effectPosition = hitPosition;
        if (hitPosition == Vector3.zero)
        {
            effectPosition = transform.position + transform.forward * 1.5f;
        }

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayEffect(shieldHitEffectPrefab, effectPosition, Quaternion.LookRotation(-transform.forward));
        }
    }

    /// <summary>
    /// 被弾時の処理
    /// </summary>
    private async UniTaskVoid HandleHit()
    {
        if (isHit || IsAnimationPlaying("GetHit")) return;

        isHit = true;
        playerManager.SetHitState(true);

        PlayRandomSound(hitVoices);
        animator.SetBool("GetHit", true);

        // 一定時間後に被弾ステートを解除
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        isHit = false;
        playerManager.SetHitState(false);
        animator.SetBool("GetHit", false);
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        playerManager.SetDeadState(true);

        PlayRandomSound(deathVoices);
        animator.CrossFade("Die", 0.5f);

        OnPlayerDeath?.Invoke();
    }

    /// <summary>
    /// 指定したアニメーションが再生中かどうか確認
    /// </summary>
    private bool IsAnimationPlaying(string animationName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) || animator.IsInTransition(0);
    }

    /// <summary>
    /// ランダムな音声を再生
    /// </summary>
    /// <param name="audioClips">音声クリップの配列</param>
    private void PlayRandomSound(string[] audioClips)
    {
        if (audioClips == null || audioClips.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);
        string clip = audioClips[randomIndex];
        VoiceManager.Instance.PlaySound(clip, volume);
    }

    /// <summary>
    /// プレイヤーの状態をリセットします。リスポーン時に呼び出します。
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
    /// 健康バーを更新します。
    /// </summary>
    /// <param name="newHealth">新しい健康値</param>
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

    /// <summary>
    /// 外部からダメージを与える際に使用（攻撃位置を指定可能）
    /// </summary>
    /// <param name="damageAmount">ダメージ量</param>
    /// <param name="attackerPosition">攻撃者の位置</param>
    public void TakeDamageFromPosition(int damageAmount, Vector3 attackerPosition)
    {
        // 攻撃者の位置からプレイヤーへの方向を計算
        Vector3 directionToPlayer = (transform.position - attackerPosition).normalized;
        Vector3 hitPosition = transform.position + directionToPlayer * 1.2f; // プレイヤーの少し前方

        TakeDamage(damageAmount, hitPosition);
    }
}