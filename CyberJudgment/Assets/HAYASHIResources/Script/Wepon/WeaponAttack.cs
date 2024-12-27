using AbubuResouse.Singleton;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;
    public void Attack(EnemyBase enemy, WeaponData weaponData)
    {
        if (!IsInAttackAnimation())
        {
            return;
        }
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
    /// <summary>
    /// �v���C���[�� Animator ���m�F���āANormalAttack�܂���StrongAttack��Ԃ��ǂ����𔻒肷��
    /// </summary>
    private bool IsInAttackAnimation()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("NormalAttack") || stateInfo.IsName("StrongAttack");
    }
}
