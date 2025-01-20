using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AbubuResouse.Log;

public class RecoveryItem : MonoBehaviour
{
    enum ItemType
    {
        HP,
        MP,
    }

    [SerializeField, Header("アイテムの種類")]
    private ItemType itemType;

    [SerializeField, Header("HP回復力")]
    private int hpRecover;

    [SerializeField, Header("MP回復力")]
    private int mpRecover;

    [SerializeField, Header("接触時のエフェクト")]
    private GameObject hitEffect;

    [SerializeField, Header("ItemHitSE")]
    private string itemHitSE;

    [SerializeField, Header("音量")]
    private float volume;

    [SerializeField, Header("エフェクトの高さオフセット")]
    private float effectHeightOffset = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        HandleItemPickup(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleItemPickup(collision.gameObject);
    }

    private void HandleItemPickup(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            var playerSystem = obj.GetComponent<PlayerHealth>();
            if (playerSystem == null) return;

            Vector3 effectPosition = transform.position + new Vector3(0, effectHeightOffset, 0);

            switch (itemType)
            {
                case ItemType.HP:
                    EffectManager.Instance.PlayEffect(hitEffect, effectPosition, Quaternion.identity);
                    playerSystem.HpRecovery(hpRecover);
                    break;
                case ItemType.MP:
                    EffectManager.Instance.PlayEffect(hitEffect, effectPosition, Quaternion.identity);
                    //playerSystem.MpRecovery(mpRecover);
                    break;
            }

            PlaySoundEffect(itemHitSE, volume);
            Destroy(gameObject);
        }
    }

    public void PlaySoundEffect(string soundName, float volume)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            DebugUtility.LogWarning("Sound name is null or empty.");
            return;
        }

        SEManager.Instance.PlaySound(soundName, volume);
    }
}
