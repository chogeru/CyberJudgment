using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField, Header("アニメーター")]
    private Animator _animator;

    [SerializeField, Header("最大体力")]
    private int _maxHealth = 100;
    [SerializeField, Header("現在の体力")]
    private int _currentHealth;

    // 体力が変化した際に通知するためのReactiveプロパティ
    private ReactiveProperty<int> health;

    [SerializeField, Header("体力スライダー")]
    private Slider healthSlider;

    private PlayerManager playerManager;

    private bool isDead = false; // 死亡状態
    private bool isHit = false; // 被ダメージ中の状態

    private void Awake()
    {
        _currentHealth = _maxHealth;
        playerManager = GetComponent<PlayerManager>();
        health = new ReactiveProperty<int>(_currentHealth);
    }

    private void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = _maxHealth;
            healthSlider.value = _currentHealth;
        }

        // 体力が変更されたときにUIを更新するなどの処理を購読
        health.Subscribe(newHealth =>
        {
            UpdateHealthUI(newHealth);
        }).AddTo(this);
    }

    /// <summary>
    /// ダメージを受けるメソッド
    /// </summary>
    /// <param name="damageAmount">受けるダメージの量</param>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // 既に死亡している場合は処理しない

        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        health.Value = _currentHealth;

        if (_currentHealth > 0)
        {
            if (!isHit && !IsAnimationPlaying("GetHit"))
            {
                isHit = true;
                playerManager.SetHitState(true);
                _animator.SetBool("GetHit", true);
            }
        }
        else
        {
            Die();
        }
    }

    /// <summary>
    /// 現在のアニメーションステートが指定された名前かどうかをチェックするメソッド
    /// </summary>
    /// <param name="animationName">チェックするアニメーションの名前</param>
    /// <returns>アニメーションが再生中であればtrue、そうでなければfalse</returns>
    private bool IsAnimationPlaying(string animationName)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        bool isTransitioning = _animator.IsInTransition(0);
        return stateInfo.IsName(animationName) || isTransitioning;
    }

    private void HitEnd()
    {
        isHit = false;
        playerManager.SetHitState(false);
        _animator.SetBool("GetHit", false);
    }

    /// <summary>
    /// 体力を回復するメソッド
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void Heal(int healAmount)
    {
        if (isDead) return; // 死亡後は回復させない

        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        health.Value = _currentHealth;
    }

    /// <summary>
    /// 死亡時の処理を行うメソッド
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        playerManager.SetDeadState(true);  // 死亡フラグをセット
        _animator.CrossFade("Die", 0.5f);
    }

    private void DieEnd()
    {
        // 死亡後の処理をここに追加
    }

    /// <summary>
    /// 体力が変化した際にUIを更新するメソッド
    /// </summary>
    /// <param name="newHealth">更新後の体力</param>
    private void UpdateHealthUI(int newHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = newHealth;
        }
    }
}
