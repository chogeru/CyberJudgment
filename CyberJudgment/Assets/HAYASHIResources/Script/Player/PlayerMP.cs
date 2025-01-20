using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class PlayerMP : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField]
    private float maxMP = 100f; // 最大MP
    private float currentMP;      // 現在のMP

    [Header("UI Settings")]
    [SerializeField]
    private Slider mpSlider;      // MPを表示するスライダー

    // マナの変更を通知するReactiveProperty
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
    /// MPの初期化を行います。
    /// </summary>
    private void InitializeMP()
    {
        currentMP = maxMP;
        mp.Value = currentMP;
    }

    /// <summary>
    /// MP UIの初期化と更新の購読を行います。
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
            Debug.LogError("PlayerMP: Slider が割り当てられていません。");
        }
    }

    /// <summary>
    /// スライダーの初期設定を行います。
    /// </summary>
    private void SetupMPSlider()
    {
        mpSlider.maxValue = maxMP;
        mpSlider.value = currentMP;
    }

    /// <summary>
    /// MPの変化をスライダーに反映する購読を設定します。
    /// </summary>
    private void SubscribeToMPChanges()
    {
        mp.Subscribe(UpdateMPSlider)
          .AddTo(this);
    }

    /// <summary>
    /// スライダーの値を更新します。
    /// </summary>
    /// <param name="currentMP">現在のMPの値</param>
    private void UpdateMPSlider(float currentMP)
    {
        mpSlider.value = currentMP;
    }

    /// <summary>
    /// 指定量のMPを消費します。成功すればtrueを返します。
    /// </summary>
    /// <param name="amount">消費するMPの量</param>
    /// <returns>消費に成功した場合はtrue、失敗した場合はfalse</returns>
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
    /// 指定量のMPを回復します。
    /// </summary>
    /// <param name="amount">回復するMPの量</param>
    public void RecoverMP(float amount)
    {
        currentMP = Mathf.Min(currentMP + amount, maxMP);
        mp.Value = currentMP;
    }

    /// <summary>
    /// 現在のMPを取得します。
    /// </summary>
    /// <returns>現在のMPの値</returns>
    public float GetCurrentMP()
    {
        return currentMP;
    }

    /// <summary>
    /// 最大MPを取得します。
    /// </summary>
    /// <returns>最大MPの値</returns>
    public float GetMaxMP()
    {
        return maxMP;
    }
}
