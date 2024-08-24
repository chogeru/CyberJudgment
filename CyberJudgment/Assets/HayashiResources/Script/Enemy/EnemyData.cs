using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "敵/敵データ作成")]
public class EnemyData : ScriptableObject
{
    [Tab("基本情報")]
    [SerializeField, Header("敵ID")]
    public int enemyID;

    [SerializeField, Header("敵の名前")]
    public string enemyName;

    [Tab("ステータス")]
    [SerializeField, Header("体力")]
    public float health;

    [SerializeField, Header("移動速度")]
    public float moveSpeed;

    [SerializeField, Header("索敵範囲")]
    public float detectionRange;

    [SerializeField, Header("徘徊ポイント停止時間")]
    public float patrolPointWaitTime;

    [SerializeField, Header("視野判定")]
    public float visionRange;

    [SerializeField, Header("攻撃距離")]
    public float attackRange;

    [SerializeField, Header("攻撃間隔")]
    public float attackCooldown = 2f;

    [Tab("ドロップ情報")]
    [SerializeField, Header("ドロップアイテムのプレハブ")]
    public GameObject[] dropItemPrefab;

    [SerializeField, Header("ドロップアイテムの数")]
    public int dropItemCount;

    [Tab("戦闘設定")]
    [SerializeField, Header("行動タイプ")]
    public BehaviorType behaviorType;

    [SerializeField, Header("属性")]
    public EnemyAttribute enemyAttribute;

    [SerializeField, Header("敵タイプ")]
    public EnemyType enemyType;

    [SerializeField, Header("戦闘武器種類")]
    public WeaponType weaponType;

    [SerializeField, Header("近接通常攻撃力")]
    public int meleeAttackPower;

    [SerializeField, Header("近接強攻撃攻撃力")]
    public int meleeStringAttackpower;

    [Tab("遠距離攻撃")]
    [SerializeField, Header("弾速")]
    public float projectileSpeed;

    [SerializeField, Header("弾の威力")]
    public float projectilePower;

    [SerializeField, Header("弾数")]
    public int projectileCount;

    [Tab("遠距離攻撃")]
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
public enum BehaviorType
{
    Patrol,
    Idle
}