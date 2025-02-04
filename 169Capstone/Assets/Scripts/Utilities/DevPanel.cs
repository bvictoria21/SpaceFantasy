﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevPanel : MonoBehaviour
{
    public GameObject buttonPanel;
    
    public Toggle moveSpeedToggle;
    public Toggle noDamageToggle;
    public Toggle skipStatRerollToggle;
    public Toggle superJumpToggle;
    public Toggle fastGearTierToggle;
    public Toggle maxDamageToggle;

    public Toggle devPanelToggle;

    private static bool speedBoost = false;
    private static bool godMode = false;
    private static bool superJump = false;
    private static bool fastGearTier = false;
    private static bool maxDamage = false;

    private static bool devPanelOpen = false;
    
    [Tooltip("SET TO FALSE FOR BUILDS")]
    public bool setDevPanelActiveOnStart = true;

    void Start()
    {
        devPanelToggle.isOn = setDevPanelActiveOnStart;
        ToggleDevPanel();
    }

    public void UpdateValuesThatPersistBetweenScenes()
    {
        if(speedBoost){
            moveSpeedToggle.isOn = true;
            SpeedBoost();
        }

        if(godMode){
            noDamageToggle.isOn = true;
            NoDamage();
        }

        if(superJump){
            superJumpToggle.isOn = true;   
            EnableSuperJump();
        }

        if(fastGearTier){
            fastGearTierToggle.isOn = true;   
            ToggleFastGearTier();
        }
        else{
            fastGearTierToggle.isOn = false;   
            ToggleFastGearTier();
        }

        if(maxDamage){
            maxDamageToggle.isOn = true;   
            MaxDamage();
        }
        else{
            maxDamageToggle.isOn = false;   
            MaxDamage();
        }

        if(devPanelOpen){
            devPanelToggle.isOn = true;   
            ToggleDevPanel();
        }
        else{
            devPanelToggle.isOn = false;   
            ToggleDevPanel();
        }
    }

    // Toggle
    public void ToggleFastGearTier()
    {
        if(fastGearTierToggle.isOn){
            GameManager.instance.SetEnemiesKilledToGearTierUpValue(2);
            UIUtils.SetImageColorFromHex( fastGearTierToggle.GetComponent<Image>(), InGameUIManager.TURQUOISE_COLOR );
            fastGearTier = true;
        }
        else{
            GameManager.instance.SetEnemiesKilledToGearTierUpValue();
            UIUtils.SetImageColorFromHex( fastGearTierToggle.GetComponent<Image>(), "#FFFFFF" );
            fastGearTier = false;
        }
    }

    // Toggle
    public void ToggleDevPanel()
    {
        if(devPanelToggle.isOn){
            buttonPanel.SetActive(true);
            UIUtils.SetImageColorFromHex( devPanelToggle.GetComponent<Image>(), InGameUIManager.TURQUOISE_COLOR );
            devPanelOpen = true;
        }
        else{
            buttonPanel.SetActive(false);
            UIUtils.SetImageColorFromHex( devPanelToggle.GetComponent<Image>(), "#FFFFFF" );
            devPanelOpen = false;
        }
    }

    // Toggle
    public void EnableSuperJump()
    {
        if(Player.instance == null){
            superJumpToggle.isOn = false;
            superJump = false;
            return;
        }

        if(superJumpToggle.isOn){
            Player.instance.GetComponent<Movement>().jumpSpeed = 42;
            UIUtils.SetImageColorFromHex( superJumpToggle.GetComponent<Image>(), InGameUIManager.TURQUOISE_COLOR );
            superJump = true;
        }
        else{
            Player.instance.GetComponent<Movement>().jumpSpeed = 30;
            UIUtils.SetImageColorFromHex( superJumpToggle.GetComponent<Image>(), "#FFFFFF" );
            superJump = false;
        }
    }

    // Toggle
    public void SpeedBoost()
    {
        if(Player.instance == null){
            moveSpeedToggle.isOn = false;
            speedBoost = false;
            return;
        }

        if(moveSpeedToggle.isOn){
            Player.instance.stats.SetMoveSpeedBase(2f);
            UIUtils.SetImageColorFromHex( moveSpeedToggle.GetComponent<Image>(), InGameUIManager.TURQUOISE_COLOR );
            speedBoost = true;
        }
        else{
            Player.instance.stats.SetMoveSpeedBase(1f);
            UIUtils.SetImageColorFromHex( moveSpeedToggle.GetComponent<Image>(), "#FFFFFF" );
            speedBoost = false;
        }
    }

    // Toggle
    public void MaxDamage()
    {
        if(Player.instance == null){
            maxDamageToggle.isOn = false;
            maxDamage = false;
            return;
        }

        if(maxDamageToggle.isOn){
            Player.instance.stats.SetBonusForStat(Player.instance, StatType.STRDamage, EntityStats.BonusType.flat, 1000);
            UIUtils.SetImageColorFromHex( maxDamageToggle.GetComponent<Image>(), InGameUIManager.TURQUOISE_COLOR );
            maxDamage = true;
        }
        else{
            Player.instance.stats.SetBonusForStat(Player.instance, StatType.STRDamage, EntityStats.BonusType.flat, 0);
            UIUtils.SetImageColorFromHex( maxDamageToggle.GetComponent<Image>(), "#FFFFFF" );
            maxDamage = false;
        }
    }

    // Toggle
    public void NoDamage()
    {
        if(Player.instance == null){
            noDamageToggle.isOn = false;
            godMode = false;
            return;
        }

        if(noDamageToggle.isOn){
            Player.instance.health.tempPlayerGodModeToggle = true;
            UIUtils.SetImageColorFromHex( noDamageToggle.GetComponent<Image>(), InGameUIManager.TURQUOISE_COLOR );
            godMode = true;
        }
        else{
            Player.instance.health.tempPlayerGodModeToggle = false;
            UIUtils.SetImageColorFromHex( noDamageToggle.GetComponent<Image>(), "#FFFFFF" );
            godMode = false;
        }
    }

    public void Give10Electrum()
    {
        PlayerInventory.instance.SetTempCurrency( PlayerInventory.instance.tempCurrency + 10 );
    }

    public void Give1000Electrum()
    {
        PlayerInventory.instance.SetTempCurrency( PlayerInventory.instance.tempCurrency + 1000 );
    }

    public void Give10StarShards()
    {
        PlayerInventory.instance.SetPermanentCurrency( PlayerInventory.instance.permanentCurrency + 10 );
    }

    public void MaxSTR()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetStrength(20);
        Debug.Log("set STR to 20");
    }

    public void MaxDEX()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetDexterity(20);
        Debug.Log("set DEX to 20");
    }

    public void MaxINT()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetIntelligence(20);
        Debug.Log("set INT to 20");
    }

    public void MaxWIS()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetWisdom(20);
        Debug.Log("set WIS to 20");
    }

    public void MaxCON()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetConstitution(20);
        Debug.Log("set CON to 20");
    }

    public void MaxCHA()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetCharisma(20);
        Debug.Log("set CHA to 20");
    }

    public void MinCHA()
    {
        if(Player.instance == null){
            return;
        }
        Player.instance.stats.SetCharisma(5);
        Debug.Log("set CHA to 5");
    }

    public void UnlockElevator()
    {
        if(GameManager.instance.InSceneWithRandomGeneration() || GameManager.instance.currentSceneName == GameManager.LICH_ARENA_STRING_NAME){
            SceneTransitionDoor door = FindObjectOfType<SceneTransitionDoor>();
            if(door)
                door.GetComponent<Collider>().enabled = true;
        }
    }

    public void TriggerEpilogue()
    {
        GameManager.instance.epilogueTriggered = true;
    }

    public void ToggleInteractabilityOnDeviceChange( bool set )
    {
        foreach( Selectable s in GetComponentsInChildren<Selectable>() ){
            s.interactable = set;
        }
    }

    public void UnlockAllJournalEntries()
    {
        JournalContentID[] list = new JournalContentID[(int)JournalContentID.enumSize];
        for(int i = 0; i < (int)JournalContentID.enumSize; i++){
            list[i] = (JournalContentID)i;
        }
        GameManager.instance.journalContentManager.UnlockJournalEntry(list);
    }
}
