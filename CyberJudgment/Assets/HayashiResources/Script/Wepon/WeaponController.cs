using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField, Header("武器のステータスデータ")]
    private WeaponData _weaponData;

    private WeaponAttack _weaponAttack;

    private void Awake()
    {
        _weaponAttack = GetComponent<WeaponAttack>();
        if (_weaponAttack == null)
        {
            _weaponAttack = gameObject.AddComponent<WeaponAttack>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                _weaponAttack.Attack(enemy, _weaponData);
            }
        }
    }
}
