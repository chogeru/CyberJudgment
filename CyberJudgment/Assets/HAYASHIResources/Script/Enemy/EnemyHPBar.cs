using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    [Header("HPバー設定")]
    [SerializeField] private Slider immediateSlider; // 即時変動スライダー
    [SerializeField] private Slider smoothSlider;    // 滑らか変動スライダー

    [Header("滑らか変動の速度")]
    [SerializeField] private float smoothSpeed = 2f; // 滑らかに変動する速度

    [Header("敵の名前表示")]
    [SerializeField] private TextMeshProUGUI enemyNameText; // TextMeshProを使用する場合

    private Coroutine smoothCoroutine;

    private void Awake()
    {
        // スライダーの参照を取得
        if (immediateSlider == null)
        {
            immediateSlider = transform.Find("HPBarImmediate").GetComponent<Slider>();
            if (immediateSlider == null)
            {
                Debug.LogError("HPBarImmediateスライダーが見つかりません。プレハブの階層を確認してください。");
            }
        }

        if (smoothSlider == null)
        {
            smoothSlider = transform.Find("HPBarSmooth").GetComponent<Slider>();
            if (smoothSlider == null)
            {
                Debug.LogError("HPBarSmoothスライダーが見つかりません。プレハブの階層を確認してください。");
            }
        }

        // 名前テキストの参照を取得
        if (enemyNameText == null)
        {
            enemyNameText = transform.Find("EnemyNameText").GetComponent<TextMeshProUGUI>();
            // enemyNameText = transform.Find("EnemyNameText").GetComponent<Text>(); // 通常のTextを使用する場合
            if (enemyNameText == null)
            {
                Debug.LogError("EnemyNameTextが見つかりません。プレハブの階層を確認してください。");
            }
        }
    }

    /// <summary>
    /// HPバーを更新する
    /// </summary>
    /// <param name="currentHealth">現在のHP</param>
    /// <param name="maxHealth">最大HP</param>
    /// <param name="enemyName">敵の名前</param>
    public void UpdateHPBar(float currentHealth, float maxHealth, string enemyName)
    {
        if (immediateSlider == null || smoothSlider == null || enemyNameText == null) return;

        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);

        // 即時変動部分を更新
        immediateSlider.value = fillAmount;

        // 滑らか変動部分を更新
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
        }
        smoothCoroutine = StartCoroutine(SmoothFill(fillAmount));

        // 敵の名前を更新
        enemyNameText.text = enemyName;
    }

    /// <summary>
    /// 滑らかにHPバーを変動させるコルーチン
    /// </summary>
    /// <param name="targetFillAmount">目標のfillAmount</param>
    /// <returns></returns>
    private IEnumerator SmoothFill(float targetFillAmount)
    {
        float currentFill = smoothSlider.value;
        while (!Mathf.Approximately(currentFill, targetFillAmount))
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFillAmount, smoothSpeed * Time.deltaTime);
            smoothSlider.value = currentFill;
            yield return null;
        }
    }

    /// <summary>
    /// HPバーの表示を有効化または無効化する
    /// </summary>
    /// <param name="isVisible">表示する場合はtrue、非表示にする場合はfalse</param>
    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
