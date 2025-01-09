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
    [SerializeField,Header("�A�C�e���̎��")]
    private ItemType itemType;
    [SerializeField, Header("HP�񕜗�")]
    private int hpRecover;
    [SerializeField,Header("MP�񕜗�")]
    private int mpRecover;
    [SerializeField, Header("�ڐG���̃G�t�F�N�g")]
    private GameObject hitEffect;

    [SerializeField, Header("ItemHitSE")]
    private string itemHitSE;
    [SerializeField,Header("����")]
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

