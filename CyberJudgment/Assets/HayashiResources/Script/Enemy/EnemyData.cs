using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "G/Gf[^ì¬")]
public class EnemyData : ScriptableObject
{
    [Tab("î{îñ")]
    [SerializeField, Header("GID")]
    public int enemyID;

    [SerializeField, Header("GÌ¼O")]
    public string enemyName;

    [Tab("Xe[^X")]
    [SerializeField, Header("ÌÍ")]
    public float health;

    [SerializeField, Header("Ú®¬x")]
    public float moveSpeed;

    [SerializeField, Header("õGÍÍ")]
    public float detectionRange;

    [SerializeField, Header("pj|Cgâ~Ô")]
    public float patrolPointWaitTime;

    [SerializeField, Header("ì»è")]
    public float visionRange;

    [SerializeField, Header("U£")]
    public float attackRange;

    [SerializeField, Header("UÔu")]
    public float attackCooldown = 2f;

    [Tab("hbvîñ")]
    [SerializeField, Header("hbvACeÌvnu")]
    public GameObject[] dropItemPrefab;

    [SerializeField, Header("hbvACeÌ")]
    public int dropItemCount;

    [Tab("í¬Ýè")]
    [SerializeField, Header("s®^Cv")]
    public BehaviorType behaviorType;

    [SerializeField, Header("®«")]
    public EnemyAttribute enemyAttribute;

    [SerializeField, Header("G^Cv")]
    public EnemyType enemyType;

    [SerializeField, Header("í¬ííÞ")]
    public WeaponType weaponType;

    [SerializeField, Header("ßÚÊíUÍ")]
    public int meleeAttackPower;

    [SerializeField, Header("ßÚ­UUÍ")]
    public int meleeStringAttackpower;

    [Tab("£U")]
    [SerializeField, Header("e¬")]
    public float projectileSpeed;

    [SerializeField, Header("eÌÐÍ")]
    public float projectilePower;

    [SerializeField, Header("e")]
    public int projectileCount;

    [Tab("£U")]
    [SerializeField, Header("Í")]
    public float magicPower;

    [SerializeField, Header("@UÔu")]
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