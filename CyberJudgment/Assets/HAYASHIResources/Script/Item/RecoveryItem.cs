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
    [SerializeField,Header("アイテムの種類")]
    private ItemType itemType;
    [SerializeField, Header("HP回復力")]
    private int hpRecover;
    [SerializeField,Header("MP回復力")]
    private int mpRecover;
    [SerializeField, Header("接触時のエフェクト")]
    private GameObject hitEffect;

    [SerializeField, Header("ItemHitSE")]
    private string itemHitSE;
    [SerializeField,Header("音量")]
    private float volume;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var playerSystem = other.GetComponent<PlayerHealth>();
            if (playerSystem == null) return;

            switch (itemType)
            {
                case ItemType.HP:
                    EffectManager.Instance.PlayEffect(hitEffect, transform.position, Quaternion.identity);
                    playerSystem.HpRecovery(hpRecover);
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

