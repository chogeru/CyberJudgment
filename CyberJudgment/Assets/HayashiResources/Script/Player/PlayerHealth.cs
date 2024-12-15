using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField, Header("�A�j���[�^�[")]
    private Animator _animator;

    [SerializeField, Header("�ő�̗�")]
    private int _maxHealth = 100;
    [SerializeField, Header("���݂̗̑�")]
    private int _currentHealth;

    // �̗͂��ω������ۂɒʒm���邽�߂�Reactive�v���p�e�B
    private ReactiveProperty<int> health;

    [SerializeField, Header("�̗̓X���C�_�[")]
    private Slider healthSlider;

    private PlayerManager playerManager;

    private bool isDead = false; // ���S���
    private bool isHit = false; // ��_���[�W���̏��

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
        if (isDead) return; // ���Ɏ��S���Ă���ꍇ�͏������Ȃ�

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
    /// ���݂̃A�j���[�V�����X�e�[�g���w�肳�ꂽ���O���ǂ������`�F�b�N���郁�\�b�h
    /// </summary>
    /// <param name="animationName">�`�F�b�N����A�j���[�V�����̖��O</param>
    /// <returns>�A�j���[�V�������Đ����ł����true�A�����łȂ����false</returns>
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
    /// �̗͂��񕜂��郁�\�b�h
    /// </summary>
    /// <param name="healAmount">�񕜗�</param>
    public void Heal(int healAmount)
    {
        if (isDead) return; // ���S��͉񕜂����Ȃ�

        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        health.Value = _currentHealth;
    }

    /// <summary>
    /// ���S���̏������s�����\�b�h
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        playerManager.SetDeadState(true);  // ���S�t���O���Z�b�g
        _animator.CrossFade("Die", 0.5f);
    }

    private void DieEnd()
    {
        // ���S��̏����������ɒǉ�
    }

    /// <summary>
    /// �̗͂��ω������ۂ�UI���X�V���郁�\�b�h
    /// </summary>
    /// <param name="newHealth">�X�V��̗̑�</param>
    private void UpdateHealthUI(int newHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = newHealth;
        }
    }
}
