using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField,Header("�A�j���[�^�[")]
    private Animator _animator;

    [SerializeField, Header("�ő�̗�")]
    private int _maxHealth = 100;
    [SerializeField, Header("���݂̗̑�")]
    private int _currentHealth;
    
    // �̗͂��ω������ۂɒʒm���邽�߂�Reactive�v���p�e�B
    private ReactiveProperty<int> health;

    [SerializeField, Header("�̗̓X���C�_�[")]
    private Slider healthSlider;

    [SerializeField, Header("�̗̓e�L�X�g")]
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

        // �̗͂��ύX���ꂽ�Ƃ���UI���X�V����Ȃǂ̏������w��
        health.Subscribe(newHealth =>
        {
            UpdateHealthUI(newHealth);
        }).AddTo(this);
    }

    /// <summary>
    /// �_���[�W���󂯂郁�\�b�h
    /// </summary>
    /// <param name="damageAmount">�󂯂�_���[�W�̗�</param>
    public void TakeDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth); 
        health.Value = _currentHealth;
        if (_currentHealth > 0)
        {
            // �܂������Ă���ꍇ��GetHit�A�j���[�V�������Đ�
            _animator.SetBool("GetHit",true);
        }
        else
        {
            // HP��0�ȉ��ɂȂ����ꍇ��Die�A�j���[�V�������Đ�
            Die();
        }
    }

    private void HitEnd()
    {
        _animator.SetBool("GetHit", false);
    }

    /// <summary>
    /// �̗͂��񕜂��郁�\�b�h
    /// </summary>
    /// <param name="healAmount">�񕜗�</param>
    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        health.Value = _currentHealth;
    }

    /// <summary>
    /// ���S���̏������s�����\�b�h
    /// </summary>
    private void Die()
    {
       _animator.CrossFade("Die",0.5f);
    }

    private void DieEnd()
    {
        
    }

    /// <summary>
    /// �̗͂��ω������ۂ�UI���X�V���郁�\�b�h (��)
    /// </summary>
    /// <param name="newHealth">�X�V��̗̑�</param>
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
