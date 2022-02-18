﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    [SerializeField] private PlayerStats stats;
    private EntityHealth health;

    public int currentStr;
    public int currentDex;
    public int currentCon;
    public int currentInt;
    public int currentWis;
    public int currentCha;

    public float currentAttackSpeed;

    [SerializeField] public Transform handPos;

    //public Timer timer;
    //public bool test = true;

    [SerializeField] private SpeakerData speakerData;
    [SerializeField] private GameObject swordPrefab;

    void Awake()
    {
        if( instance ){
            Destroy(gameObject);
        }
        else{
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        health = gameObject.GetComponent<EntityHealth>();
        health.maxHitpoints = 30;
        health.currentHitpoints = 30;

        health.SetStartingHealthUI();
        //health.maxHitpoints = stats.getMaxHitPoints();
        //health.currentHitpoints = stats.getMaxHitPoints();

        currentStr = stats.Strength();
        currentDex = stats.Dexterity();
        currentCon = stats.Constitution();
        currentInt = stats.Intelligence();
        currentWis = stats.Wisdom();
        currentCha = stats.Charisma();
        currentAttackSpeed = stats.getAttackSpeed();
        //currentHitpoints = stats.getMaxHitPoints();

        if(DialogueManager.instance != null && !DialogueManager.instance.DialogueManagerHasSpeaker(speakerData)){
            DialogueManager.instance.AddSpeaker(speakerData);
        }

        Equipment gen = Instantiate(swordPrefab).GetComponent<Equipment>();
        PlayerInventory.instance.EquipItem(InventoryItemSlot.Weapon, gen);

        StartCoroutine(DetectFall());
    }

    public float CurrentAttackSpeed()
    {
        return currentAttackSpeed;
    }

    private IEnumerator DetectFall()
    {
        yield return new WaitUntil(() => transform.position.y <= -6);
        health.Damage(health.maxHitpoints);
    }
}
