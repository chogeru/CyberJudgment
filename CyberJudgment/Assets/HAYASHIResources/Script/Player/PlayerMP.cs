using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class PlayerMP : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField]
    private float maxMP = 100f; // �ő�MP
    private float currentMP;      // ���݂�MP

    [Header("UI Settings")]
    [SerializeField]
    private Slider mpSlider;      // MP��\������X���C�_�[

    // �}�i�̕ύX��ʒm����ReactiveProperty
    private readonly ReactiveProperty<float> mp = new ReactiveProperty<float>();

    private void Awake()
    {
        InitializeMP();
    }

    private void Start()
    {
        InitializeMPUI();
    }

    /// <summary>
    /// MP�̏��������s���܂��B
    /// </summary>
    private void InitializeMP()
    {
        currentMP = maxMP;
        mp.Value = currentMP;
    }

    /// <summary>
    /// MP UI�̏������ƍX�V�̍w�ǂ��s���܂��B
    /// </summary>
    private void InitializeMPUI()
    {
        if (mpSlider != null)
        {
            SetupMPSlider();
            SubscribeToMPChanges();
        }
        else
        {
            Debug.LogError("PlayerMP: Slider �����蓖�Ă��Ă��܂���B");
        }
    }

    /// <summary>
    /// �X���C�_�[�̏����ݒ���s���܂��B
    /// </summary>
    private void SetupMPSlider()
    {
        mpSlider.maxValue = maxMP;
        mpSlider.value = currentMP;
    }

    /// <summary>
    /// MP�̕ω����X���C�_�[�ɔ��f����w�ǂ�ݒ肵�܂��B
    /// </summary>
    private void SubscribeToMPChanges()
    {
        mp.Subscribe(UpdateMPSlider)
          .AddTo(this);
    }

    /// <summary>
    /// �X���C�_�[�̒l���X�V���܂��B
    /// </summary>
    /// <param name="currentMP">���݂�MP�̒l</param>
    private void UpdateMPSlider(float currentMP)
    {
        mpSlider.value = currentMP;
    }

    /// <summary>
    /// �w��ʂ�MP������܂��B���������true��Ԃ��܂��B
    /// </summary>
    /// <param name="amount">�����MP�̗�</param>
    /// <returns>����ɐ��������ꍇ��true�A���s�����ꍇ��false</returns>
    public bool ConsumeMP(float amount)
    {
        if (currentMP >= amount)
        {
            currentMP -= amount;
            mp.Value = currentMP;
            return true;
        }
        return false;
    }

    /// <summary>
    /// �w��ʂ�MP���񕜂��܂��B
    /// </summary>
    /// <param name="amount">�񕜂���MP�̗�</param>
    public void RecoverMP(float amount)
    {
        currentMP = Mathf.Min(currentMP + amount, maxMP);
        mp.Value = currentMP;
    }

    /// <summary>
    /// ���݂�MP���擾���܂��B
    /// </summary>
    /// <returns>���݂�MP�̒l</returns>
    public float GetCurrentMP()
    {
        return currentMP;
    }

    /// <summary>
    /// �ő�MP���擾���܂��B
    /// </summary>
    /// <returns>�ő�MP�̒l</returns>
    public float GetMaxMP()
    {
        return maxMP;
    }
}
