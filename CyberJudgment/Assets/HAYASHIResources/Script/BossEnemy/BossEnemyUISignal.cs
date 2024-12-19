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

        // 初期UIのセットアップ
        BossUIManager.Instance.SetBossName(_enemy.enemyData.enemyName);
        BossUIManager.Instance.SetBossHealth(_enemy._currentHealth, _enemy.enemyData.health);
        await BossUIManager.Instance.StartBossUI();
        // Enemyの体力変更イベントを登録
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
    private void UpdateBossHealthUI(float currentHealth, float maxHealth)
    {
        BossUIManager.Instance.SetBossHealth(currentHealth, maxHealth);
    }
}
