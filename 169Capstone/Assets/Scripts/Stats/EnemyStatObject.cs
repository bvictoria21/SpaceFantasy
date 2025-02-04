﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unique internal IDs per enemy
public enum EnemyID
{
    // Bosses
    TimeLich,
    BeetleBoss,
    Harvester,

    // Normal Enemies
    Slime,
    EnchantedCloak,

    // Elite variants
    EliteSlime,
    EliteRobert,

    // Size
    enumSize
}

[CreateAssetMenu(menuName = "ScriptableObjects/EnemyStatObject")]
public class EnemyStatObject : ScriptableObject
{
    [SerializeField] private EnemyID enemyID;

    [SerializeField] private float maxHitPoints;    
    [SerializeField] private float attackSpeed; 
    [SerializeField] private float moveSpeed;   
    [SerializeField] private float defense; 
    [SerializeField] private float dodgeChance; 
    [SerializeField] private float critChance;  
    [SerializeField] private float critDamage;
    [SerializeField] private float trapDamageResist;

    public EnemyID EnemyID()
    {
        return enemyID;
    }

    public float MaxHitPoints()
    {
        return maxHitPoints;
    }

    public float AttackSpeed()
    {
        return attackSpeed;
    }

    public float MoveSpeed()
    {
        return moveSpeed;
    }

    public float Defense()
    {
        return defense;
    }

    public float DodgeChance()
    {
        return dodgeChance;
    }

    public float CritChance()
    {
        return critChance;
    }

    public float CritDamage()
    {
        return critDamage;
    }

    public float TrapDamageResist()
    {
        return trapDamageResist;
    }
}
