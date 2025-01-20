using UnityEngine;
using System;
using VInspector;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "�G/�G�f�[�^�쐬")]
public class EnemyData : ScriptableObject
{
    [Tab("��{���")]
    [SerializeField, Header("�GID")]
    public int enemyID;

    [SerializeField, Header("�G�̖��O")]
    public string enemyName;

    [Tab("�X�e�[�^�X")]
    [SerializeField, Header("�ŏ��̗�")]
    public int minHealth;

    [SerializeField, Header("�ő�̗�")]
    public int maxHealth;

    [SerializeField, Header("�ړ����x")]
    public float moveSpeed;

    [SerializeField, Header("���G�͈�")]
    public float detectionRange;

    [SerializeField, Header("�p�j�|�C���g��~����")]
    public float patrolPointWaitTime;

    [SerializeField, Header("���씻��")]
    public float visionRange;

    [SerializeField, Header("�U������")]
    public float attackRange;

    [SerializeField, Header("�U���Ԋu")]
    public float attackCooldown = 2f;

    // �����u�Œ�̗́v�t�B�[���h���s�v�ł���΍폜���Ă�������
    [SerializeField, Header("�Œ�̗�(�s�v�Ȃ�폜�\)")]
    public float health;

    [Tab("�h���b�v���")]
    [SerializeField, Header("�h���b�v�A�C�e����񃊃X�g")]
    public ItemDropInfo[] dropItemInfos;
    [Header("����h���b�v���")]
    public WeaponDropInfo weaponDropInfo;

    [Tab("�퓬�ݒ�")]
    [SerializeField, Header("�s���^�C�v")]
    public BehaviorType behaviorType;

    [SerializeField, Header("����")]
    public EnemyAttribute enemyAttribute;

    [SerializeField, Header("�G�^�C�v")]
    public EnemyType enemyType;

    [SerializeField, Header("�퓬������")]
    public WeaponType weaponType;

    [SerializeField, Header("�ߐڒʏ�U����")]
    public int meleeAttackPower;

    [SerializeField, Header("�ߐڋ��U���U����")]
    public int meleeStringAttackpower;

    [Tab("�������U��")]
    [SerializeField, Header("�e��")]
    public float projectileSpeed;

    [SerializeField, Header("�e�̈З�")]
    public float projectilePower;

    [SerializeField, Header("�e��")]
    public int projectileCount;

    [Tab("�������U��")]
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

public enum BehaviorType
{
    Patrol,
    Idle
}
