using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyUISignal : MonoBehaviour
{
    [SerializeField, Header("Boss��Enemy.cs")]
    private Enemy _enemy;

    private async void Start()
    {
        if (_enemy == null)
        {
            Debug.LogError("Enemy ���ݒ肳��Ă��܂���I");
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
            // �C�x���g�o�^������
            _enemy.OnHealthChanged -= UpdateBossHealthUI;
        }
    }

    /// <summary>
    /// �̗͂��ω���������UI���X�V����
    /// </summary>
    /// <param name="currentHealth">���݂̗̑�</param>
    /// <param name="maxHealth">�ő�̗�</param>
    private async void UpdateBossHealthUI(float currentHealth, float maxHealth)
    {
        await BossUIManager.Instance.SetBossHealth(currentHealth, maxHealth);
    }
}
