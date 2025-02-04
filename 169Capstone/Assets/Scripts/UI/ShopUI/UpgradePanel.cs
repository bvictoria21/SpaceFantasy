﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradePanel : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int currentCost {get; private set;}
    public int upgradeBaseCost {get; private set;}
    public int costIncreasePerLevel {get; private set;}

    public int currentUpgradeLevel {get; private set;}
    public int totalUpgradeLevels {get; private set;}

    [SerializeField] private PermanentUpgradeType upgradeType;
    public string upgradeName {get; private set;}
    public string baseDescription {get; private set;}
    public string currentDescription {get; private set;}

    private const string TOO_BROKE_ALERT = "\n\n<i>Not enough Star Shards.</i>";
    private const string STAT_MIN_MAX_ALERT = "\n\n<i>Cannot increase stat minimum above stat maximum.</i>";
    
    private const int MIN_STAT_MAX = 15;
    private const int MAX_STAT_MAX = 20;

    private const int MIN_STAT_NUM_TIMES_PURCHASABLE = 10;
    private const int MAX_STAT_NUM_TIMES_PURCHASABLE = 5;
    
    private const int STAT_BASE_COST = 2;
    private const float STAT_MIN_COST_INCREASE = 2f;
    private const int STAT_MAX_COST_INCREASE = 4;

    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text skillLevelText;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private Image starShardIcon;

    [Tooltip("ONLY for stat upgrades; leave blank for skills bc it's set in code")]
    [SerializeField] private Sprite upgradeIconSprite;

    [SerializeField] private Button upgradeButton;

    private bool soldOut = false;
    private bool statMinEqualsMax = false;
    private bool cannotAffordUpgrade = false;

    private ShopUIStellan shopUI;

    void Start()
    {
        if(!IsStatUpgrade()){
            upgradeIconSprite = upgradeIcon.sprite;
        }
    }

    public void SetShopUI(ShopUIStellan _shop)
    {
        shopUI = _shop;
    }

    public PermanentUpgradeType GetPanelUpgradeType()
    {
        return upgradeType;
    }

    public bool IsStatUpgrade()
    {
        return (int)upgradeType < 12;
    }

    public bool IsStatMinimum()
    {
        switch(upgradeType){
            case PermanentUpgradeType.STRMin:
            case PermanentUpgradeType.DEXMin:
            case PermanentUpgradeType.INTMin:
            case PermanentUpgradeType.WISMin:
            case PermanentUpgradeType.CONMin:
            case PermanentUpgradeType.CHAMin:
                return true;

            case PermanentUpgradeType.STRMax:
            case PermanentUpgradeType.DEXMax:
            case PermanentUpgradeType.INTMax:
            case PermanentUpgradeType.WISMax:
            case PermanentUpgradeType.CONMax:
            case PermanentUpgradeType.CHAMax:
                return false;
        }
        Debug.LogWarning("Cannot determine if stat min for upgrade type: " + upgradeType);
        return false;
    }

    public void UpdateUIDisplayValues()
    {
        SetCurrentCost();

        CheckPurchaseConditions();
        UpdateBaseDescriptionValues();

        // If stat upgrade
        if( IsStatUpgrade() ){            
            skillLevelText.text = "" + currentUpgradeLevel;            
            // Update description
            if(soldOut){
                currentDescription = "Max <b>" + upgradeName + "</b> reached.";
            }
            else{
                currentDescription = baseDescription;
            }
            currentDescription += "\n\n" + GetStatValueDescriptionFromType();
        }
        // If skill
        else{            
            skillLevelText.text = currentUpgradeLevel + "/" + totalUpgradeLevels;
            currentDescription = baseDescription;
        }
        
        UpdateUIBasedOnTopPriorityCondition();

        // If this is the currently active hover panel, update the focus panel values
        if(shopUI.activeUpgradeInFocus && shopUI.activeUpgradeInFocus == this){
            shopUI.SetFocusPanelValues(upgradeName, GetFocusPanelSkillLevelString(), currentDescription, costText.text, upgradeIconSprite, IsStatUpgrade());
        }
    }

    private void SetCurrentCost()
    {
        if(IsStatUpgrade()){
            SetStatCurrentCost();
        }
        else{
            SetSkillCurrentCost();
        }
    }

    private void UpdateUIBasedOnTopPriorityCondition()
    {
        // If sold out, that was handled already so just return (higheset priority)
        if(soldOut){
            return;
        }

        // If a stat and not sold out, set UI based on Min/Max status 
        if(IsStatUpgrade() && !soldOut && !cannotAffordUpgrade){
            SetStatValuesBasedOnMinMaxStatus();
        }

        // If there's still more to purchase, set cost value + set color values based on if we can afford it or not
        if(!soldOut && !statMinEqualsMax){
            costText.text = "" + currentCost;
            SetValuesBasedOnAffordStatus();
        }

        if(cannotAffordUpgrade){
            currentDescription += TOO_BROKE_ALERT;
        }
    }

    private void SetStatValuesBasedOnMinMaxStatus()
    {
        if( !IsStatUpgrade() ){
            return;
        }

        if(statMinEqualsMax){
            currentDescription += STAT_MIN_MAX_ALERT;

            costText.color = shopUI.GetCannotAffordTextColor();
            upgradeIcon.color = shopUI.GetCannotAffordIconColor();
        }
        else{
            costText.color = shopUI.GetCanPurchaseTextColor();
            upgradeIcon.color = new Color(255,255,255,255);
        }
    }

    private void SetValuesBasedOnAffordStatus()
    {
        if(cannotAffordUpgrade){
            costText.color = shopUI.GetCannotAffordTextColor();
            upgradeIcon.color = shopUI.GetCannotAffordIconColor();
        }
        else{
            costText.color = shopUI.GetCanPurchaseTextColor();
            upgradeIcon.color = new Color(255,255,255,255);
        }
    }

    public void CheckPurchaseConditions()
    {
        cannotAffordUpgrade = currentCost > PlayerInventory.instance.permanentCurrency;        
        StatMinEqualsStatMax();
    }

    private void UpdateBaseDescriptionValues()
    {
        // If stat
        if(IsStatUpgrade()){
            baseDescription = "Increase <b>" + upgradeName + "</b> from <b>" + currentUpgradeLevel + "</b> to <color=" + InGameUIManager.SLIME_GREEN_COLOR + ">" + (currentUpgradeLevel+1) + "</color>.";
        }
        // If update-able skill
        else{
            switch(upgradeType){
                case PermanentUpgradeType.ArmorPlating:
                    float defense = PermanentUpgradeManager.instance.GetCurrentSkillValue(upgradeType)*100;
                    baseDescription = "Increase base <b>Defense</b> from <b>" + UIUtils.GetTruncatedDecimalForUIDisplay(defense) + "%</b> to <color=" + InGameUIManager.SLIME_GREEN_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(defense + PermanentUpgradeManager.instance.armorPlatingBonusPerLevel*100) + "%</color>.";
                    return;
                case PermanentUpgradeType.ExtensiveTraining:
                    float attackSpeed = PermanentUpgradeManager.instance.GetCurrentSkillValue(upgradeType) * 100;
                    baseDescription = "Increase base <b>Attack Speed</b> from <b>" + UIUtils.GetTruncatedDecimalForUIDisplay(attackSpeed) + "%</b> to <color=" + InGameUIManager.SLIME_GREEN_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(attackSpeed + PermanentUpgradeManager.instance.extensiveTrainingBonusPerLevel*100) + "%</color>.";
                    return;
                case PermanentUpgradeType.PrecisionDrive:
                    float critDamage = PermanentUpgradeManager.instance.GetCurrentSkillValue(upgradeType) * 100;
                    float newCritDamage = 0;
                    switch(currentUpgradeLevel){
                        case 0:
                            newCritDamage = PermanentUpgradeManager.instance.precisionDriveBonusPerLevel[1] * 100;
                            break;
                        case 1:
                            newCritDamage = PermanentUpgradeManager.instance.precisionDriveBonusPerLevel[2] * 100;
                            break;
                        case 2:
                            newCritDamage = PermanentUpgradeManager.instance.precisionDriveBonusPerLevel[3] * 100;
                            break;
                    }
                    baseDescription = "Increase base <b>Critical Hit Damage</b> from <b>+" + UIUtils.GetTruncatedDecimalForUIDisplay(critDamage) + "%</b> to <color=" + InGameUIManager.SLIME_GREEN_COLOR + ">+" + UIUtils.GetTruncatedDecimalForUIDisplay(newCritDamage) + "%</color>.";
                    return;
            }
        }
    }

    // Optionally can pass in an upgrade type to change the type of this panel
    public void InitializeUpgradeValues()
    {
        SetValuesByType();        
        if( IsStatUpgrade() ){    // If stat upgrade
            currentUpgradeLevel = PermanentUpgradeManager.instance.GetStatGenerationValueFromUpgradeType(upgradeType);
            upgradeBaseCost = STAT_BASE_COST;
            SetStatCurrentCost();
        }
        else{   // If skill
            currentUpgradeLevel = PermanentUpgradeManager.instance.GetSkillLevel(upgradeType);
            SetSkillCurrentCost();
        }        

        UpdateBaseDescriptionValues();        
        SetMaxUpgradesReached( currentUpgradeLevel == totalUpgradeLevels ); // Calls UpdateUIDisplayValues in here
    }

    public void PurchaseItem()
    {
        // Check conditions (if you can't purchase, clicking does nothing)
        if(soldOut || statMinEqualsMax){
            // TODO: Maybe UI feedback?
            return;
        }

        if(cannotAffordUpgrade){
            // TODO: UI feedback about being too broke to buy an item
            return;
        }

        PlayerInventory.instance.SpendPermanentCurrency(currentCost);
        currentUpgradeLevel++;

        // If stat upgrade
        if( IsStatUpgrade() ){
            UpgradeAssociatedStatValue();
        }
        // If skill
        else{
            PermanentUpgradeManager.instance.SetSkillLevel(upgradeType,currentUpgradeLevel);
        }

        if(currentUpgradeLevel == totalUpgradeLevels){
            SetMaxUpgradesReached(true);
        }

        UpdateUIDisplayValues();

        shopUI.UpdateAllUpgradePanels();
    }

    private void SetMaxUpgradesReached(bool set)
    {
        soldOut = set;

        if(set){
            upgradeIcon.color = shopUI.GetMaxUpgradesReachedIconColor();
            costText.text = "";
        }
        else{
            upgradeIcon.color = new Color(255,255,255,255);
        }

        ToggleStarShardIconActive(!set);
        UpdateUIDisplayValues();
    }

    private void ToggleStarShardIconActive(bool set)
    {
        if(set){
            starShardIcon.color = new Color(255,255,255,255);
        }
        else{
            starShardIcon.color = new Color(255,255,255,0);
        }
    }

    #region Selectable Handling

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnUpgradePanelSelect();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnUpgradePanelDeselct();
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnUpgradePanelSelect();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnUpgradePanelDeselct();
        }

        private void OnUpgradePanelSelect()
        {
            shopUI.activeUpgradeInFocus = this;
            UpdateUIDisplayValues();
            shopUI.SetFocusPanelValues(upgradeName, GetFocusPanelSkillLevelString(), currentDescription, costText.text, upgradeIconSprite, IsStatUpgrade());
        }

        private void OnUpgradePanelDeselct()
        {
            shopUI.activeUpgradeInFocus = null;
            shopUI.ClearFocusPanel();
        }

    #endregion

    private string GetFocusPanelSkillLevelString()
    {
        string skillLevel = "";
        if(IsStatUpgrade()){
            if(IsStatMinimum()){
                skillLevel = "Min";
            }
            else{
                skillLevel = "Max";
            }
            skillLevel += " Reroll Value: ";
        }
        return skillLevel + skillLevelText.text;
    }

    private void SetValuesByType()
    {
        switch(upgradeType){
            case PermanentUpgradeType.STRMin:
                upgradeName = "Strength Minimum";
                totalUpgradeLevels = MIN_STAT_MAX;
                costIncreasePerLevel = -1;      // We don't use this value for minimums so it's -1 so that if it's being used we know there's an issue!
                return;
            case PermanentUpgradeType.STRMax:
                upgradeName = "Strength Maximum";
                totalUpgradeLevels = MAX_STAT_MAX;
                costIncreasePerLevel = STAT_MAX_COST_INCREASE;
                return;
            case PermanentUpgradeType.DEXMin:
                upgradeName = "Dexterity Minimum";
                totalUpgradeLevels = MIN_STAT_MAX;
                costIncreasePerLevel = -1;
                return;
            case PermanentUpgradeType.DEXMax:
                upgradeName = "Dexterity Maximum";
                totalUpgradeLevels = MAX_STAT_MAX;
                costIncreasePerLevel = STAT_MAX_COST_INCREASE;
                return;
            case PermanentUpgradeType.INTMin:
                upgradeName = "Intelligence Minimum";
                totalUpgradeLevels = MIN_STAT_MAX;
                costIncreasePerLevel = -1;
                return;
            case PermanentUpgradeType.INTMax:
                upgradeName = "Intelligence Maximum";
                totalUpgradeLevels = MAX_STAT_MAX;
                costIncreasePerLevel = STAT_MAX_COST_INCREASE;
                return;
            case PermanentUpgradeType.WISMin:
                upgradeName = "Wisdom Minimum";
                totalUpgradeLevels = MIN_STAT_MAX;
                costIncreasePerLevel = -1;
                return;
            case PermanentUpgradeType.WISMax:
                upgradeName = "Wisdom Maximum";
                totalUpgradeLevels = MAX_STAT_MAX;
                costIncreasePerLevel = STAT_MAX_COST_INCREASE;
                return;
            case PermanentUpgradeType.CONMin:
                upgradeName = "Constitution Minimum";
                totalUpgradeLevels = MIN_STAT_MAX;
                costIncreasePerLevel = -1;
                return;
            case PermanentUpgradeType.CONMax:
                upgradeName = "Constitution Maximum";
                totalUpgradeLevels = MAX_STAT_MAX;
                costIncreasePerLevel = STAT_MAX_COST_INCREASE;
                return;
            case PermanentUpgradeType.CHAMin:
                upgradeName = "Charisma Minimum";
                totalUpgradeLevels = MIN_STAT_MAX;
                costIncreasePerLevel = -1;
                return;
            case PermanentUpgradeType.CHAMax:
                upgradeName = "Charisma Maximum";
                totalUpgradeLevels = MAX_STAT_MAX;
                costIncreasePerLevel = STAT_MAX_COST_INCREASE;
                return;
            case PermanentUpgradeType.ArmorPlating:
                upgradeName = "Armor Plating";
                upgradeBaseCost = 5;
                costIncreasePerLevel = 5;
                totalUpgradeLevels = PermanentUpgradeManager.maxArmorPlatingLevels;
                UpdateBaseDescriptionValues();
                return;
            case PermanentUpgradeType.ExtensiveTraining:
                upgradeName = "Extensive Training";
                upgradeBaseCost = 5;
                costIncreasePerLevel = 5;
                totalUpgradeLevels = PermanentUpgradeManager.maxExtensiveTrainingLevels;
                UpdateBaseDescriptionValues();
                return;
            case PermanentUpgradeType.Natural20:
                upgradeName = "Natural 20";
                upgradeBaseCost = 10;
                costIncreasePerLevel = 0;
                totalUpgradeLevels = PermanentUpgradeManager.maxNatural20Levels;
                baseDescription = "Increase base Critical Hit Chance to <color=" + InGameUIManager.SLIME_GREEN_COLOR + ">" + PermanentUpgradeManager.instance.natural20BonusPerLevel*100 + "%</color>.";
                return;
            case PermanentUpgradeType.PrecisionDrive:
                upgradeName = "Precision Drive";
                upgradeBaseCost = 10;
                costIncreasePerLevel = 10;
                totalUpgradeLevels = PermanentUpgradeManager.maxPrecisionDriveLevels;
                UpdateBaseDescriptionValues();
                return;
            case PermanentUpgradeType.StartingPotions:
                upgradeName = "Regenerative Draught";
                upgradeBaseCost = 10;
                costIncreasePerLevel = 10;
                totalUpgradeLevels = PermanentUpgradeManager.maxStartingHealthPotionQuantity;
                baseDescription = "Increase your starting health potion quantity by 1.";
                return;
            case PermanentUpgradeType.TimeLichKillerThing:
                upgradeName = "Deus Ex Machina";
                upgradeBaseCost = 90;
                costIncreasePerLevel = 0;
                totalUpgradeLevels = PermanentUpgradeManager.maxTimeLichThingLevels;
                baseDescription = "The missing piece of the puzzle: The Lich's phylactery.\n\n<color=" + InGameUIManager.MAGENTA_COLOR + ">Increases starting <b>Threat Level</b> to <b>" + (GameTimer.STARTING_ENEMY_TIER_WITH_DEUS_EX+1) + "</b>.</color>";
                return;
        }
        Debug.LogWarning("No data found for upgrade type: " + upgradeType);
    }

    private void SetSkillCurrentCost()
    {
        if(IsStatUpgrade()){
            return;
        }

        currentCost = upgradeBaseCost + (currentUpgradeLevel * costIncreasePerLevel);
    }

    private void SetStatCurrentCost()
    {
        if(!IsStatUpgrade()){
            return;
        }

        // If Stat MIN
        if( (int)upgradeType < 6 ){
            currentCost = upgradeBaseCost + Mathf.FloorToInt((MIN_STAT_NUM_TIMES_PURCHASABLE - (totalUpgradeLevels - currentUpgradeLevel)) * STAT_MIN_COST_INCREASE);
        }
        // If Stat MAX
        else{
            currentCost = upgradeBaseCost + ((MAX_STAT_NUM_TIMES_PURCHASABLE - (totalUpgradeLevels - currentUpgradeLevel)) * costIncreasePerLevel);
        }        
    }

    public void UpgradeAssociatedStatValue()
    {
        switch(upgradeType){
            case PermanentUpgradeType.STRMin:
                PermanentUpgradeManager.instance.SetStrengthMin(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.STRMax:
                PermanentUpgradeManager.instance.SetStrengthMax(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.DEXMin:
                PermanentUpgradeManager.instance.SetDexterityMin(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.DEXMax:
                PermanentUpgradeManager.instance.SetDexterityMax(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.INTMin:
                PermanentUpgradeManager.instance.SetIntMin(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.INTMax:
                PermanentUpgradeManager.instance.SetIntMax(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.WISMin:
                PermanentUpgradeManager.instance.SetWisdomMin(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.WISMax:
                PermanentUpgradeManager.instance.SetWisdomMax(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.CONMin:
                PermanentUpgradeManager.instance.SetConMin(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.CONMax:
                PermanentUpgradeManager.instance.SetConMax(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.CHAMin:
                PermanentUpgradeManager.instance.SetCharismaMin(currentUpgradeLevel);
                return;
            case PermanentUpgradeType.CHAMax:
                PermanentUpgradeManager.instance.SetCharismaMax(currentUpgradeLevel);
                return;
        }
        Debug.LogWarning("No method found for upgrade type: " + upgradeType);
    }

    private void StatMinEqualsStatMax()
    {
        switch(upgradeType){
            case PermanentUpgradeType.STRMin:
                statMinEqualsMax = currentUpgradeLevel == PermanentUpgradeManager.instance.strMax;
                return;
            case PermanentUpgradeType.DEXMin:
                statMinEqualsMax = currentUpgradeLevel == PermanentUpgradeManager.instance.dexMax;
                return;
            case PermanentUpgradeType.INTMin:
                statMinEqualsMax = currentUpgradeLevel == PermanentUpgradeManager.instance.intMax;
                return;
            case PermanentUpgradeType.WISMin:
                statMinEqualsMax = currentUpgradeLevel == PermanentUpgradeManager.instance.wisMax;
                return;
            case PermanentUpgradeType.CONMin:
                statMinEqualsMax = currentUpgradeLevel == PermanentUpgradeManager.instance.conMax;
                return;
            case PermanentUpgradeType.CHAMin:
                statMinEqualsMax = currentUpgradeLevel == PermanentUpgradeManager.instance.charismaMax;
                return;
        }
        statMinEqualsMax = false;
    }

    #region Stat Descriptions

        private const string STR_DESCRIPTION = "Increases the damage dealt by Strength-based weapons and abilities. Also increases Defense.";
        private const string DEX_DESCRIPTION = "Increases the damage dealt by Dexterity-based weapons and abilities. Also increases Dodge Chance.";
        private const string INT_DESCRIPTION = "Increases the damage dealt by Intelligence-based weapons and abilities. Also increases Critical Hit Chance.";
        private const string WIS_DESCRIPTION = "Increases the damage dealt by Wisdom-based weapons and abilities. Also reduces the length of ability cooldowns.";
        private const string CON_DESCRIPTION = "Increases Max Health and Trap Damage Resist.";
        private const string CHA_DESCRIPTION = "Reduces the Electrum prices of items in shops during runs.";

        private string GetStatValueDescriptionFromType()
        {
            switch(upgradeType){
                case PermanentUpgradeType.STRMin:
                    return STR_DESCRIPTION;
                case PermanentUpgradeType.STRMax:
                    return STR_DESCRIPTION;
                case PermanentUpgradeType.DEXMin:
                    return DEX_DESCRIPTION;
                case PermanentUpgradeType.DEXMax:
                    return DEX_DESCRIPTION;
                case PermanentUpgradeType.INTMin:
                    return INT_DESCRIPTION;
                case PermanentUpgradeType.INTMax:
                    return INT_DESCRIPTION;
                case PermanentUpgradeType.WISMin:
                    return WIS_DESCRIPTION;
                case PermanentUpgradeType.WISMax:
                    return WIS_DESCRIPTION;
                case PermanentUpgradeType.CONMin:
                    return CON_DESCRIPTION;
                case PermanentUpgradeType.CONMax:
                    return CON_DESCRIPTION;
                case PermanentUpgradeType.CHAMin:
                    return CHA_DESCRIPTION;
                case PermanentUpgradeType.CHAMax:
                    return CHA_DESCRIPTION;
            }
            Debug.LogError("No description found for upgrade type: " + upgradeType);
            return "ERROR";
        }

    #endregion

    private PlayerStatName GetStatFromUpgradeType()
    {
        switch(upgradeType){
            case PermanentUpgradeType.STRMin:
            case PermanentUpgradeType.STRMax:
                return PlayerStatName.STR;

            case PermanentUpgradeType.DEXMin:
            case PermanentUpgradeType.DEXMax:
                return PlayerStatName.DEX;

            case PermanentUpgradeType.INTMin:
            case PermanentUpgradeType.INTMax:
                return PlayerStatName.INT;

            case PermanentUpgradeType.WISMin:
            case PermanentUpgradeType.WISMax:
                return PlayerStatName.WIS;

            case PermanentUpgradeType.CONMin:
            case PermanentUpgradeType.CONMax:
                return PlayerStatName.CON;

            case PermanentUpgradeType.CHAMin:
            case PermanentUpgradeType.CHAMax:
                return PlayerStatName.CHA;
        }
        Debug.LogWarning("Cannot get player facing stat name for upgrade type: " + upgradeType);
        return PlayerStatName.size;
    }
}