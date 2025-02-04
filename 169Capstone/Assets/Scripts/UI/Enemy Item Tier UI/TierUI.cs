﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TierUI : MonoBehaviour
{
    public enum TierUIType{
        ThreatLevel,
        LootFactor
    }

    [SerializeField] private TierUIType tierUIType;

    [SerializeField] private Slider tierSlider;
    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text leftText;
    [SerializeField] private TMP_Text rightText;

    private int currentTier;
    private bool lootTierIsLegendary = false;

    void Start()
    {
        currentTier = 0;
        
        // GEAR TIERS
        if(tierUIType == TierUIType.LootFactor){
            tierSlider.maxValue = GameManager.instance.enemiesKilledToGearTierUp;
            GameManager.instance.OnTierIncreased.AddListener(UpdateLootTierUIOnTierUp);
            UpdateTierUIOnTierUp(1 + "", 2 + "", UIUtils.GetColorFromRarity(ItemRarity.Common));

            if(GameManager.instance.currentSceneName != GameManager.GAME_LEVEL1_STRING_NAME){
                UpdateLootTierUIOnTierUp(GameManager.instance.gearTier);
                tierSlider.value = GameManager.instance.gearTierUISaveValue;
            }
        }

        // ENEMY TIERS
        else if(tierUIType == TierUIType.ThreatLevel){
            GameManager.instance.gameTimer.SetEnemyTierUI(this);
            tierSlider.maxValue = GameTimer.secondsPerEnemyTier;

            GameManager.instance.gameTimer.OnTierIncrease.AddListener(UpdateEnemyTierUIOnTierUp);
            // int startingEnemyTier = GameManager.instance.gameTimer.enemyTier;
            // UpdateTierUIOnTierUp(startingEnemyTier+1 + "", startingEnemyTier+2 + "", InGameUIManager.MAGENTA_COLOR);
            
            UpdateEnemyTierUIOnTierUp(GameManager.instance.gameTimer.enemyTier);

            if(GameManager.instance.currentSceneName != GameManager.GAME_LEVEL1_STRING_NAME){
                // UpdateEnemyTierUIOnTierUp(GameManager.instance.gameTimer.enemyTier);
                tierSlider.value = GameManager.instance.gameTimer.enemyTierUISaveValue;
            }
        }
    }

    private void UpdateTierUIOnTierUp(string leftLabel, string rightLabel)
    {
        leftText.text = leftLabel;
        rightText.text = rightLabel;

        tierSlider.value = tierSlider.minValue;
    }

    private void UpdateTierUIOnTierUp(string leftLabel, string rightLabel, string hexCode)
    {
        if(!hexCode.Contains("#")){
            Debug.LogError("Invalid hexcode provided to set slider fill color");
            return;
        }

        UIUtils.SetImageColorFromHex(fillImage, hexCode);
        UpdateTierUIOnTierUp(leftLabel, rightLabel);
    }

    private void UpdateEnemyTierUIOnTierUp(int newEnemyTier)
    {
        if(tierUIType != TierUIType.ThreatLevel){
            Debug.LogError("Failed to update enemy tier UI for non-enemy type tier UI");
            return;
        }

        currentTier = newEnemyTier;
        UpdateTierUIOnTierUp(currentTier+1+"", currentTier+2+"");
    }

    private void UpdateLootTierUIOnTierUp(int newLootTier)
    {
        if(tierUIType != TierUIType.LootFactor){
            Debug.LogError("Failed to update loot tier UI for non-loot type tier UI");
            return;
        }

        currentTier = newLootTier;
        lootTierIsLegendary = (ItemRarity)currentTier == ItemRarity.Legendary;

        string rightLabel;
        if(lootTierIsLegendary)
            rightLabel = "";
        else
            rightLabel = currentTier + 2 + "";  // Not using 0, starting at 1, so we have to add one to both left and right for the labels

        UpdateTierUIOnTierUp(currentTier + 1 + "", rightLabel, UIUtils.GetColorFromRarity((ItemRarity)currentTier));

        if(lootTierIsLegendary){
            tierSlider.value = tierSlider.maxValue;
        }
    }

    public void IncrementTierUI(float value = 1)
    {
        if(lootTierIsLegendary || GameManager.instance.currentSceneName == GameManager.LICH_ARENA_STRING_NAME){
            return;
        }

        if(tierSlider.value == tierSlider.maxValue){
            return;
        }
        tierSlider.value += value;
    }

    // When the scene changes, save our current slider position in case we're going to the lich arena
    void OnDisable()
    {
        if(GameManager.instance.InSceneWithRandomGeneration()){
            if(tierUIType == TierUIType.ThreatLevel)
                GameManager.instance.gameTimer.enemyTierUISaveValue = tierSlider.value;
            else
                GameManager.instance.gearTierUISaveValue = tierSlider.value;
        }
    }
}
