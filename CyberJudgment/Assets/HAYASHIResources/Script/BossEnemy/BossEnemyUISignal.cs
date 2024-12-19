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

        // ����UI�̃Z�b�g�A�b�v
        BossUIManager.Instance.SetBossName(_enemy.enemyData.enemyName);
        BossUIManager.Instance.SetBossHealth(_enemy._currentHealth, _enemy.enemyData.health);
        await BossUIManager.Instance.StartBossUI();
        // Enemy�̗͕̑ύX�C�x���g��o�^
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
    private void UpdateBossHealthUI(float currentHealth, float maxHealth)
    {
        BossUIManager.Instance.SetBossHealth(currentHealth, maxHealth);
    }
}
