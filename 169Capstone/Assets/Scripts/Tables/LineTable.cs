﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemLine
{
    CritChance,
    CritDamage,
    AttackSpeed,
    Defense,
    MovementSpeed,
    EffectChance,
    EffectResist,
    DodgeChance,
    HPIncrease,

    enumSize
}

[CreateAssetMenu(menuName = "Gear/LineTableObject")]
public class LineTable : ScriptableObject
{
    [SerializeField] private List<ItemRarity> itemRarityTier;

    [SerializeField] private List<List<float>> secondaryLineNumberRates = new List<List<float>>();
    [SerializeField] private List<List<float>> lineEnhancementRates = new List<List<float>>();

    [SerializeField] private List<float> CommonSecondaryLineNumberRates;
    [SerializeField] private List<float> UncommonSecondaryLineNumberRates;
    [SerializeField] private List<float> RareSecondaryLineNumberRates;
    [SerializeField] private List<float> EpicSecondaryLineNumberRates;
    [SerializeField] private List<float> LegendarySecondaryLineNumberRates;
    [SerializeField] private List<float> CommonLineEnhancementRates;
    [SerializeField] private List<float> UncommonLineEnhancementRates;
    [SerializeField] private List<float> RareLineEnhancementRates;
    [SerializeField] private List<float> EpicLineEnhancementRates;
    [SerializeField] private List<float> LegendaryLineEnhancementRates;

    [SerializeField] private List<ItemLine> primaryWeaponLine;
    [SerializeField] private List<InventoryItemSlot> itemType;
    [SerializeField] private List<ItemLine> linePool;

    public void Setup()
    {
        secondaryLineNumberRates.Add(CommonSecondaryLineNumberRates);
        secondaryLineNumberRates.Add(UncommonSecondaryLineNumberRates);
        secondaryLineNumberRates.Add(RareSecondaryLineNumberRates);
        secondaryLineNumberRates.Add(EpicSecondaryLineNumberRates);
        secondaryLineNumberRates.Add(LegendarySecondaryLineNumberRates);

        lineEnhancementRates.Add(CommonLineEnhancementRates);
        lineEnhancementRates.Add(UncommonLineEnhancementRates);
        lineEnhancementRates.Add(RareLineEnhancementRates);
        lineEnhancementRates.Add(EpicLineEnhancementRates);
        lineEnhancementRates.Add(LegendaryLineEnhancementRates);
    }

    public List<ItemRarity> ItemRarityTier()
    {
        return itemRarityTier;
    }

    public List<List<float>> SecondaryLineNumberRates()
    {
        return secondaryLineNumberRates;
    }

    public List<List<float>> LineEnhancementRates()
    {
        return lineEnhancementRates;
    }

    public List<ItemLine> PrimaryWeaponLine()
    {
        return primaryWeaponLine;
    }

    public List<InventoryItemSlot> ItemType()
    {
        return itemType;
    }

    public List<ItemLine> LinePool()
    {
        return linePool;
    }
}
