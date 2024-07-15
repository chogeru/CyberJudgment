using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "�G/�G�f�[�^�쐬")]
public class EnemyData : ScriptableObject
{
    [SerializeField, Header("�GID")]
    public int enemyID;

    [SerializeField, Header("�G�̖��O")]
    public string enemyName;

    [SerializeField, Header("�̗�")]
    public float health;

    [SerializeField, Header("�ړ����x")]
    public float moveSpeed;

    [SerializeField, Header("���G�͈�")]
    public float detectionRange;

    [SerializeField, Header("�h���b�v�A�C�e���̃v���n�u")]
    public GameObject dropItemPrefab;

    [SerializeField, Header("�h���b�v�A�C�e���̐�")]
    public int dropItemCount;

    [SerializeField, Header("����")]
    public EnemyAttribute enemyAttribute;

    [SerializeField, Header("�G�^�C�v")]
    public EnemyType enemyType;

    [SerializeField, Header("�퓬������")]
    public WeaponType weaponType;

    [SerializeField, Header("�ߐڍU����")]
    public float meleeAttackPower;

    [SerializeField, Header("�e��")]
    public float projectileSpeed;

    [SerializeField, Header("�e�̈З�")]
    public float projectilePower;

    [SerializeField, Header("�e��")]
    public int projectileCount;

    [SerializeField, Header("����")]
    public float magicPower;

    [SerializeField, Header("���@�U���Ԋu")]
    public float magicAttackInterval;
}

public enum EnemyAttribute
{
    None,
    Fire,
    Water,
    Wind,
    Darkness,
    Light,
    Earth
}

public enum EnemyType
{
    Ground,
    Air
}
