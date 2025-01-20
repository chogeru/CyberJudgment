using AbubuResouse.Singleton;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;

    public void Attack(EnemyBase enemy, WeaponData weaponData)
    {
        if (!IsInAttackAnimation()) return;
        if (enemy == null || weaponData == null) return;

        int damage = Random.Range(weaponData._minAttackPower, weaponData._maxAttackPower + 1);

        float criticalRand = Random.Range(0f, 100f);
        bool isCritical = (criticalRand < weaponData._criticalRate);
        if (isCritical)
        {
            damage *= 3;
        }

        enemy.TakeDamage(damage);

        if (DamageUIManager.Instance != null)
        {
            DamageUIManager.Instance.ShowDamageText(damage, transform.position, isCritical);
        }

        if (EffectManager.Instance != null && weaponData.EffectPrefabs != null && weaponData.EffectPrefabs.Count > 0)
        {
            GameObject randomEffect = weaponData.EffectPrefabs[Random.Range(0, weaponData.EffectPrefabs.Count)];
            EffectManager.Instance.PlayEffect(randomEffect, transform.position, Quaternion.identity);
        }

        if (isCritical)
        {
            if (weaponData.CriticalSoundEffects != null && weaponData.CriticalSoundEffects.Count > 0)
            {
                string randomCriticalSE = weaponData.CriticalSoundEffects[Random.Range(0, weaponData.CriticalSoundEffects.Count)];
                SEManager.Instance.PlaySound(randomCriticalSE, 1.0f);
            }
            else
            {
                Debug.LogWarning("�N���e�B�J���q�b�g�p�̃T�E���h�G�t�F�N�g���ݒ肳��Ă��܂���B");
            }
        }
        else
        {
            // �ʏ�U�����̃T�E���h�Đ�
            if (weaponData.SoundEffects != null && weaponData.SoundEffects.Count > 0)
            {
                string randomSE = weaponData.SoundEffects[Random.Range(0, weaponData.SoundEffects.Count)];
                SEManager.Instance.PlaySound(randomSE, 1.0f);
            }
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
