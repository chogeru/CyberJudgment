using AbubuResouse.Singleton;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    public void Attack(EnemyBase enemy, WeaponData weaponData)
    {
        if (enemy == null || weaponData == null) return;

        enemy.TakeDamage(weaponData._attackPower);
        if(DamageUIManager.Instance != null)
        {
            DamageUIManager.Instance.ShowDamageText(weaponData._attackPower,transform.position);
        }
        if (EffectManager.Instance != null && weaponData.EffectPrefabs != null && weaponData.EffectPrefabs.Count > 0)
        {
            GameObject randomEffect = weaponData.EffectPrefabs[Random.Range(0, weaponData.EffectPrefabs.Count)];
            EffectManager.Instance.PlayEffect(randomEffect, transform.position,Quaternion.identity);
        }

        if (SEManager.Instance != null && weaponData.SoundEffects != null && weaponData.SoundEffects.Count > 0)
        {
            string randomSE = weaponData.SoundEffects[Random.Range(0, weaponData.SoundEffects.Count)];
            SEManager.Instance.PlaySound(randomSE, 1.0f);
        }
    }
}
