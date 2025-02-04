﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Item Enums

public enum InventoryItemSlot
{
    Weapon,
    Helmet,
    Accessory,
    Legs,

    enumSize
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,

    enumSize    // Also "none"
}

// Every item has its own unique ID (name)
// NO LONGER IN ORDER OF TYPE - if adding stuff, add stuff AT THE END regardless of type or else IDs will get messed up in scriptable objects and stuff
// Also don't remove things, but changing the name of something is OK
public enum ItemID
{
    BerserkersBlade,
    BowAndArrows,
    NanoKnuckles,
    Rapier,
    RayGun,
    HelmOfTheRam,
    HoloGlasses,
    WristRocket,
    HelmOfSnowstorms,
    MurphysClaw,
    QuantumKunai,
    WingedBoots,
    QuantumLeggings,
    PropulsionHeels,
    TrousersOfFortitude,

    // For looping and stuff
    enumSize
}

#endregion

[CreateAssetMenu(menuName = "Gear/EquipmentBaseData")]
public class EquipmentBaseData : ScriptableObject
{
    [Tooltip("Player facing item name")]
    [SerializeField] private string itemName;
    
    [Tooltip("Internal ID")]
    [SerializeField] private ItemID itemID;

    [Tooltip("IF equipping this item, unlocks a journal content entry (or multiple), put that here; if not, leave it empty")]
    [SerializeField] private JournalContentID[] journalEntriesUnlocked;

    [Tooltip("Just the model, for dropping on the ground")]
    [SerializeField] private GameObject itemDropModelPrefab;

    [Tooltip("Actual prefab of the item (no model) created once EQUIPPED")]
    [SerializeField] private GameObject equippedItemPrefab;

    [Tooltip("Just the model, sized and angled for attaching to the player. WEAPONS ONLY!")]
    [SerializeField] private GameObject equippedWeaponModelPrefab;

    [SerializeField] private InventoryItemSlot itemSlot;
    private int itemBaseCost = 20;  // Same value for all items

    [Header("--- UI STUFF ---")]
    [TextArea(4,20)]
    [SerializeField] private string shortDescription;
    [TextArea(15,20)]
    [SerializeField] private string longDescription;

    [SerializeField] private Sprite icon;

    [Header("--- STAT VALUES ---")]
    [SerializeField] private PlayerStatName primaryStat;

    [SerializeField] private StatType primaryItemLine;

    [Header("--- ACTIVATED ITEM DATA ---")]
    [Tooltip("FOR NON-WEAPONS")]
    [SerializeField] private float duration;
    [Tooltip("FOR NON-WEAPONS; 'Base' because it's then affected by Haste")]
    [SerializeField] private float baseCooldownValue;
    [Tooltip("Most items should calculate damage when attacking from player stats, rather than using a pre-set damage value. Leave this 0 unless you have a very specific reason for using it!")]
    [SerializeField] private float damage;
    [SerializeField] private int radius;
    [SerializeField] private float range;

    public string ItemName()
    {
        return itemName;
    }

    public ItemID ItemID()
    {
        return itemID;
    }

    public GameObject ItemDropModelPrefab()
    {
        return itemDropModelPrefab;
    }

    public GameObject EquippedWeaponModelPrefab()
    {
        if(itemSlot != InventoryItemSlot.Weapon){
            Debug.LogError("Cannot retrieve weapon model prefab for item type: " + itemSlot);
            return null;
        }
        return equippedWeaponModelPrefab;
    }

    public GameObject EquippedItemPrefab()
    {
        return equippedItemPrefab;
    }

    public InventoryItemSlot ItemSlot()
    {
        return itemSlot;
    }

    public int BaseCost()
    {
        return itemBaseCost;
    }

    public string ShortDescription()
    {
        return shortDescription;
    }

    public string LongDescription()
    {
        return longDescription;
    }

    public Sprite Icon()
    {
        return icon;
    }

    public PlayerStatName PrimaryStat()
    {
        return primaryStat;
    }

    public StatType PrimaryItemLine()
    {
        return primaryItemLine;
    }

    public JournalContentID[] JournalEntriesUnlocked()
    {
        return journalEntriesUnlocked;
    }

    public float Duration()
    {
        return duration;
    }

    public float BaseCooldownValue()
    {
        return baseCooldownValue;
    }

    public float BaseDamage()
    {
        return damage;
    }

    public int Radius()
    {
        return radius;
    }  

    public float Range()
    {
        return range;
    }
}
