using UnityEngine;
using AbubuResouse.Singleton;

public class ParticleDamageHandler : MonoBehaviour
{
    [SerializeField, Header("ダメージを与えるパーティクルシステム")]
    private ParticleSystem damageParticleSystem;

    [SerializeField, Header("武器のステータスデータ")]
    private WeaponData weaponData;

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                ApplyHalfDamage(enemy);
            }
        }
    }

    private void ApplyHalfDamage(EnemyBase enemy)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("WeaponDataが設定されていません。");
            return;
        }

        // 通常攻撃のダメージを半分に
        int damage = Random.Range(weaponData._minAttackPower, weaponData._maxAttackPower + 1);

        // クリティカル判定
        float criticalRand = Random.Range(0f, 100f);
        bool isCritical = (criticalRand < weaponData._criticalRate);
        if (isCritical)
        {
            damage *= 3;
        }

        // 敵にダメージを適用
        enemy.TakeDamage(damage);
        // ダメージUIの表示
        if (DamageUIManager.Instance != null)
        {
            DamageUIManager.Instance.ShowDamageText(damage, transform.position, isCritical);
        }

        // エフェクトの再生
        if (EffectManager.Instance != null && weaponData.EffectPrefabs != null && weaponData.EffectPrefabs.Count > 0)
        {
            GameObject randomEffect = weaponData.EffectPrefabs[Random.Range(0, weaponData.EffectPrefabs.Count)];
            EffectManager.Instance.PlayEffect(randomEffect, transform.position, Quaternion.identity);
        }

        // サウンドエフェクトの再生
        if (isCritical)
        {
            if (weaponData.CriticalSoundEffects != null && weaponData.CriticalSoundEffects.Count > 0)
            {
                string randomCriticalSE = weaponData.CriticalSoundEffects[Random.Range(0, weaponData.CriticalSoundEffects.Count)];
                SEManager.Instance.PlaySound(randomCriticalSE, 1.0f);
            }
            else
            {
                Debug.LogWarning("クリティカルヒット用のサウンドエフェクトが設定されていません。");
            }
        }
        else
        {
            // 通常攻撃時のサウンド再生
            if (weaponData.SoundEffects != null && weaponData.SoundEffects.Count > 0)
            {
                string randomSE = weaponData.SoundEffects[Random.Range(0, weaponData.SoundEffects.Count)];
                SEManager.Instance.PlaySound(randomSE, 1.0f);
            }
        }
        damageParticleSystem.Stop();
    }
}
