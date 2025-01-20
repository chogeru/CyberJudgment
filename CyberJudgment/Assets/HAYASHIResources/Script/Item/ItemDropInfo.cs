using UnityEngine;
using System;

/// <summary>
/// ドロップアイテムごとの情報をまとめた構造体
/// </summary>
[Serializable]
public struct ItemDropInfo
{
    [Header("ドロップアイテムのプレハブ")]
    public GameObject itemPrefab;

    [Header("ドロップ最小数")]
    public int minDropCount;

    [Header("ドロップ最大数")]
    public int maxDropCount;

    [Range(0f, 100f), Header("ドロップ確率(%)")]
    public float dropRate;
}
