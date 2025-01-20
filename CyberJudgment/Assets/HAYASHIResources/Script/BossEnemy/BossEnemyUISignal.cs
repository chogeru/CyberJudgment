using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyUISignal : MonoBehaviour
{
    [SerializeField, Header("BossのEnemy.cs")]
    private Enemy _enemy;

    private async void Start()
    {
        if (_enemy == null)
        {
            Debug.LogError("Enemy が設定されていません！");
            return;
        }

        BossUIManager.Instance.SetBossName(_enemy.enemyData.enemyName);
        await BossUIManager.Instance.StartBossUI(_enemy._currentHealth, _enemy.enemyData.health);
        await BossUIManager.Instance.SetBossHealth(_enemy._currentHealth, _enemy.enemyData.health);

        _enemy.OnHealthChanged += UpdateBossHealthUI;
    }

    private void OnDestroy()
    {
        if (_enemy != null)
        {
            // イベント登録を解除
            _enemy.OnHealthChanged -= UpdateBossHealthUI;
        }
    }

    /// <summary>
    /// 体力が変化した時にUIを更新する
    /// </summary>
    /// <param name="currentHealth">現在の体力</param>
    /// <param name="maxHealth">最大体力</param>
    private async void UpdateBossHealthUI(float currentHealth, float maxHealth)
    {
        await BossUIManager.Instance.SetBossHealth(currentHealth, maxHealth);
    }
}
