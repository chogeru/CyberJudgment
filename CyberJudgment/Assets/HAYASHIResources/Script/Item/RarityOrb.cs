using UnityEngine;
using System.Collections.Generic;
using AbubuResouse.Singleton;

public class RarityOrb : MonoBehaviour
{
    [Header("レアリティ玉のレアリティ")]
    public WeaponRarity rarity;

    [Header("ピックアップ時のパーティクルエフェクト")]
    public GameObject pickupEffect;

    [Header("レアリティごとのピックアップサウンド")]
    public List<RaritySound> raritySounds;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // レアリティに応じた音を再生
            PlayPickupSound();

            // ピックアップエフェクトを再生
            PlayPickupEffect();

            // 武器名を取得
            string weaponName = WeaponNameGenerator.Instance.GetRandomWeaponName(rarity);
            if (!string.IsNullOrEmpty(weaponName))
            {
                // インベントリに追加
                Inventory.Instance.AddWeaponToInventory(weaponName);
                Debug.Log($"武器名「{weaponName}」がインベントリに追加されました。");
            }
            else
            {
                Debug.LogWarning("武器名の取得に失敗しました。");
            }

            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// レアリティに応じたピックアップ音を再生する
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
            Debug.LogWarning($"レアリティ「{rarity}」に対応する音声が定義されていません。");
        }
    }

    /// <summary>
    /// ピックアップエフェクトを再生する
    /// </summary>
    private void PlayPickupEffect()
    {
        if (pickupEffect != null)
        {
            EffectManager.Instance.PlayEffect(pickupEffect,transform.position,Quaternion.identity,1f);
        }
        else
        {
            Debug.LogWarning("Pickup Effect が設定されていません。");
        }
    }

    /// <summary>
    /// レアリティに応じた音声名を取得する
    /// </summary>
    /// <param name="rarity">武器のレアリティ</param>
    /// <returns>対応する音声名</returns>
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
