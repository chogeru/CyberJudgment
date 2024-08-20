using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField]
    private WeaponData _weaponData;


    private void OnTriggerEnter(Collider other)
    {
        // ���蔲�����Ώۂ�Enemy�^�O�������m�F
        if (other.CompareTag("Enemy"))
        {
            // EnemyBase �N���X�������Ă��邩�m�F���ă_���[�W��^����
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // ����f�[�^�̍U���͂Ń_���[�W��^����
                enemy.TakeDamage(_weaponData._attackPower);
            }
        }
    }
}
