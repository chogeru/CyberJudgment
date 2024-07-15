using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "敵/敵データ作成")]
public class EnemyData : ScriptableObject
{
    [SerializeField, Header("敵ID")]
    public int enemyID;

    [SerializeField, Header("敵の名前")]
    public string enemyName;

    [SerializeField, Header("体力")]
    public float health;

    [SerializeField, Header("移動速度")]
    public float moveSpeed;

    [SerializeField, Header("索敵範囲")]
    public float detectionRange;

    [SerializeField, Header("ドロップアイテムのプレハブ")]
    public GameObject dropItemPrefab;

    [SerializeField, Header("ドロップアイテムの数")]
    public int dropItemCount;

    [SerializeField, Header("属性")]
    public EnemyAttribute enemyAttribute;

    [SerializeField, Header("敵タイプ")]
    public EnemyType enemyType;

    [SerializeField, Header("戦闘武器種類")]
    public WeaponType weaponType;

    [SerializeField, Header("近接攻撃力")]
    public float meleeAttackPower;

    [SerializeField, Header("弾速")]
    public float projectileSpeed;

    [SerializeField, Header("弾の威力")]
    public float projectilePower;

    [SerializeField, Header("弾数")]
    public int projectileCount;

    [SerializeField, Header("魔力")]
    public float magicPower;

    [SerializeField, Header("魔法攻撃間隔")]
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
