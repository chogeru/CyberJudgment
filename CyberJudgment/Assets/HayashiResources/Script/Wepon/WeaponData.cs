using System.Collections.Generic;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "NewWepon", menuName = "í/íf[^ì¬")]
public class WeaponData : ScriptableObject
{
    [Tab("Ú×")]
    [SerializeField,Header("íId")]
    public int _weaponID;
    [SerializeField,Header("íÌ¼O")]
    public string _weaponName;
    [Tab("fUC")]
    [SerializeField,Header("GtFNgÌvnu")]
    public List<GameObject> EffectPrefabs;
    [SerializeField,Header("USE")]
    public List<string> SoundEffects;
    [Tab("Xe[^X")]
    [SerializeField, Header("UÍ")]
    public int _attackPower;
    [SerializeField, Header("U¬x")]
    public float _attackSpeed;
    [SerializeField, Header("ídÊ")]
    public float _weaponWeight;
    [Tab("»Ì¼")]
    [SerializeField,Header("íÌ^Cv")]
    public WeaponType _weaponType;
    [SerializeField,Header("íÌ®«")]
    public WeaponAttribute _weaponAttribute;
    [TextArea,SerializeField,Header("íà¾¶")]
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
