using UnityEngine;
using System.Collections.Generic;
using AbubuResouse.Singleton;

public class RarityOrb : MonoBehaviour
{
    [Header("���A���e�B�ʂ̃��A���e�B")]
    public WeaponRarity rarity;

    [Header("�s�b�N�A�b�v���̃p�[�e�B�N���G�t�F�N�g")]
    public GameObject pickupEffect;

    [Header("���A���e�B���Ƃ̃s�b�N�A�b�v�T�E���h")]
    public List<RaritySound> raritySounds;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // ���A���e�B�ɉ����������Đ�
            PlayPickupSound();

            // �s�b�N�A�b�v�G�t�F�N�g���Đ�
            PlayPickupEffect();

            // ���햼���擾
            string weaponName = WeaponNameGenerator.Instance.GetRandomWeaponName(rarity);
            if (!string.IsNullOrEmpty(weaponName))
            {
                // �C���x���g���ɒǉ�
                Inventory.Instance.AddWeaponToInventory(weaponName);
                Debug.Log($"���햼�u{weaponName}�v���C���x���g���ɒǉ�����܂����B");
            }
            else
            {
                Debug.LogWarning("���햼�̎擾�Ɏ��s���܂����B");
            }

            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// ���A���e�B�ɉ������s�b�N�A�b�v�����Đ�����
    /// </summary>
    private void PlayPickupSound()
    {
        string soundName = GetSoundNameByRarity(rarity);
        if (!string.IsNullOrEmpty(soundName))
        {
            SEManager.Instance.PlaySound(soundName, 1f);
        }
        else
        {
            Debug.LogWarning($"���A���e�B�u{rarity}�v�ɑΉ����鉹������`����Ă��܂���B");
        }
    }

    /// <summary>
    /// �s�b�N�A�b�v�G�t�F�N�g���Đ�����
    /// </summary>
    private void PlayPickupEffect()
    {
        if (pickupEffect != null)
        {
            EffectManager.Instance.PlayEffect(pickupEffect,transform.position,Quaternion.identity,1f);
        }
        else
        {
            Debug.LogWarning("Pickup Effect ���ݒ肳��Ă��܂���B");
        }
    }

    /// <summary>
    /// ���A���e�B�ɉ��������������擾����
    /// </summary>
    /// <param name="rarity">����̃��A���e�B</param>
    /// <returns>�Ή����鉹����</returns>
    private string GetSoundNameByRarity(WeaponRarity rarity)
    {
        foreach (var raritySound in raritySounds)
        {
            if (raritySound.rarity == rarity)
            {
                return raritySound.soundName;
            }
        }
        return null;
    }
}
