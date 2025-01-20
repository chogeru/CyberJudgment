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

    [SerializeField, Header("�A�C�e���̎��")]
    private ItemType itemType;

    [SerializeField, Header("HP�񕜗�")]
    private int hpRecover;

    [SerializeField, Header("MP�񕜗�")]
    private int mpRecover;

    [SerializeField, Header("�ڐG���̃G�t�F�N�g")]
    private GameObject hitEffect;

    [SerializeField, Header("ItemHitSE")]
    private string itemHitSE;

    [SerializeField, Header("����")]
    private float volume;

    [SerializeField, Header("�G�t�F�N�g�̍����I�t�Z�b�g")]
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
