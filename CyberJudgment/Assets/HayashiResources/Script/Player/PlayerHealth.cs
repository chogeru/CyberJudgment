using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField,Header("アニメーター")]
    private Animator _animator;

    [SerializeField, Header("最大体力")]
    private int _maxHealth = 100;
    [SerializeField, Header("現在の体力")]
    private int _currentHealth;
    
    // 体力が変化した際に通知するためのReactiveプロパティ
    private ReactiveProperty<int> health;

    [SerializeField, Header("体力スライダー")]
    private Slider healthSlider;

    [SerializeField, Header("体力テキスト")]
    private Text healthText;

    private void Awake()
    {
        _currentHealth = _maxHealth;
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
        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth); 
        health.Value = _currentHealth;
        if (_currentHealth > 0)
        {
            // まだ生きている場合はGetHitアニメーションを再生
            _animator.SetBool("GetHit",true);
        }
        else
        {
            // HPが0以下になった場合はDieアニメーションを再生
            Die();
        }
    }

    private void HitEnd()
    {
        _animator.SetBool("GetHit", false);
    }

    /// <summary>
    /// 体力を回復するメソッド
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        health.Value = _currentHealth;
    }

    /// <summary>
    /// 死亡時の処理を行うメソッド
    /// </summary>
    private void Die()
    {
       _animator.CrossFade("Die",0.5f);
    }

    private void DieEnd()
    {
        
    }

    /// <summary>
    /// 体力が変化した際にUIを更新するメソッド (仮)
    /// </summary>
    /// <param name="newHealth">更新後の体力</param>
    private void UpdateHealthUI(int newHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = newHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{newHealth}/{_maxHealth}";
        }
    }
}
