using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField]
    private WeaponData _weaponData;


    private void OnTriggerEnter(Collider other)
    {
        // すり抜けた対象がEnemyタグを持つか確認
        if (other.CompareTag("Enemy"))
        {
            // EnemyBase クラスを持っているか確認してダメージを与える
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // 武器データの攻撃力でダメージを与える
                enemy.TakeDamage(_weaponData._attackPower);
            }
        }
    }
}
