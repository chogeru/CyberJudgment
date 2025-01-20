using System.Collections.Generic;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "武器/武器データ作成")]
public class WeaponData : ScriptableObject
{
    [Tab("詳細")]
    [SerializeField, Header("武器Id")]
    public int _weaponID;

    [SerializeField, Header("武器の名前")]
    public string _weaponName;

    [Tab("デザイン")]
    [SerializeField, Header("エフェクトのプレハブ")]
    public List<GameObject> EffectPrefabs;

    [SerializeField, Header("攻撃時SE")]
    public List<string> SoundEffects;

    [SerializeField, Header("クリティカルヒット時SE")]
    public List<string> CriticalSoundEffects;

    [Tab("ステータス")]
    [SerializeField, Header("最小攻撃力")]
    public int _minAttackPower;

    [SerializeField, Header("最大攻撃力")]
    public int _maxAttackPower;

    [SerializeField, Header("クリティカル率 (%) [0 ~ 100]")]
    [Range(0f, 100f)]
    public float _criticalRate;

    /// <summary>
    /// もし既存の固定攻撃力を利用していない場合は削除してもOK
    /// </summary>
    [SerializeField, Header("固定攻撃力")]
    public int _attackPower;

    [SerializeField, Header("攻撃速度")]
    public float _attackSpeed;

    [SerializeField, Header("武器重量")]
    public float _weaponWeight;

    [Tab("ドロップ情報")]
    [SerializeField,Header("武器のレアリティ")]
    public WeaponRarity rarity;
    
    [EndTab]
    [Tab("その他")]
    [SerializeField, Header("武器のタイプ")]
    public WeaponType _weaponType;

    [SerializeField, Header("武器の属性")]
    public WeaponAttribute _weaponAttribute;

    [TextArea, SerializeField, Header("武器説明文")]
    public string _weaponDescription;


}

public enum WeaponType
{
    Sword,
    Bow,
    Axe,
    Hammer,
    Magic,
    Gun
}

public enum WeaponAttribute
{
    None,
    Fire,
    Water,
    Wind,
    Darkness,
    Light,
    Earth
}
