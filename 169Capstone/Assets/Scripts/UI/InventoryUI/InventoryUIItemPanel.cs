﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class InventoryUIItemPanel : MonoBehaviour
{
    // Values that descriptions can include as variables based on stats
    private enum DescriptionVariableCode{
        // Primary Stats
        STR,
        DEX,
        INT,
        WIS,
        CHA,
        CON,

        // Damage Values
        SD,     // STR Damage
        DD,     // DEX Damage
        WD,     // WIS Damage
        ID,     // INT Damage

        // Secondary Stats
        ATS,    // Attack Speed
        MOS,    // Move Speed
        DEF,    // Defense
        DOC,    // Dodge Chance
        CRC,    // Crit Chance
        CRD,    // Crit Damage
        TDR,    // Trap Damage Resist
        HAS,    // Haste (Cooldown Reduction)

        enumSize
    }

    public enum ItemPanelType{
        EquippedGeneric,
        CurrentlyEquippedCompareItem,
        NewItemToCompare
    }

    public const string STAT_VARIABLE_PATTERN = @"({(STR|DEX|INT|WIS|CHA|CON|SD|DD|WD|ID|ATS|MOS|DEF|DOC|CRC|CRD|TDR|HAS)(:\d+)?(,(\*|\+|-)(\d?.)?\d+(,(\*|\+|-)(\d?.)?\d+)?)?})";

    [SerializeField] private InventoryItemSlot itemSlot;
    [HideInInspector] public ItemRarity rarity;

    public Image itemIcon;
    public TMP_Text itemName;
    public TMP_Text itemTypeRarity;
    public TMP_Text itemDescription;

    public Image statIcon;

    private string shortDescription = "";    // 1-2 lines
    private string expandedDescription = ""; // Detailed additions

    public GameObject descriptionPanel;
    public FlexibleGridLayout textGrid;
    public HorizontalLayoutGroup horizontalLayoutGroup;

    private GeneratedEquipment itemData;

    [SerializeField] private Toggle toggle;

    private ItemPanelType itemPanelType;

    public void SetItemPanelValues(GeneratedEquipment _itemData, ItemPanelType panelType)
    {
        itemPanelType = panelType;

        itemData = _itemData;

        EquipmentBaseData baseData = itemData.equipmentBaseData;

        if(itemSlot != _itemData.equipmentBaseData.ItemSlot()){
            Debug.Log("Panel item slot != equipment data slot! Setting it now (necessary if new item panel in compare UI)");
            itemSlot = _itemData.equipmentBaseData.ItemSlot();
        }

        rarity = itemData.rarity;

        itemName.text = "<color=" + UIUtils.GetColorFromRarity(rarity) + ">" + baseData.ItemName() + "</color>";

        string statString = "";
        if(itemData.equipmentBaseData.PrimaryStat() != PlayerStatName.size){
            statString = itemData.equipmentBaseData.PrimaryStat().ToString();
        }
        itemTypeRarity.text = rarity.ToString() + " " + statString + " " + baseData.ItemSlot().ToString();

        shortDescription = baseData.ShortDescription();
        expandedDescription = GenerateExpandedDescription();
        itemDescription.text = shortDescription;

        itemIcon.sprite = baseData.Icon();
        itemIcon.preserveAspect = true;

        if( baseData.PrimaryStat() != PlayerStatName.size ){
            statIcon.color = new Color(255,255,255,255);
            statIcon.sprite = InGameUIManager.instance.GetSpriteFromStatType( baseData.PrimaryStat() );
        }
        else{
            statIcon.color = new Color(255,255,255,0);
        }
        
        // Check bc compare item panel doesn't have a toggle
        if(toggle){
            toggle.interactable = true;
        }
    }

    public void SetDefaultItemPanelValues()
    {
        itemName.text = "";
        rarity = ItemRarity.enumSize;

        itemTypeRarity.text = itemSlot.ToString();

        shortDescription = "EMPTY";
        expandedDescription = "";
        itemDescription.text = shortDescription;

        itemIcon.sprite = InGameUIManager.instance.GetDefaultItemIconForSlot(itemSlot);
        itemIcon.preserveAspect = true;

        statIcon.color = new Color(255,255,255,0);

        if(toggle){
            toggle.interactable = false;
        }
    }

    public InventoryItemSlot GetItemSlot()
    {
        return itemSlot;
    }

    public void SetExpandedDescription(bool set)
    {
        if(set){
            itemDescription.text = expandedDescription;
        }
        else{
            itemDescription.text = shortDescription;
        }
    }

    private string GenerateExpandedDescription()
    {
        string generatedDescription = itemData.equipmentBaseData.LongDescription();

        Regex rgx = new Regex(STAT_VARIABLE_PATTERN);
        foreach(Match match in rgx.Matches(generatedDescription)){
            // Get the value and location in the string
            string matchString = match.Value;
            int matchIndex = match.Index;

            // Generate the value
            string newStringValue = GetStatVariableValue(matchString);

            // Swap out that part of the string for the value
            generatedDescription = generatedDescription.Replace(matchString, newStringValue);
        }

        if(itemSlot != InventoryItemSlot.Weapon){
            // TEMP (will need to do stuff to factor in haste, but in a way similar to calculating damage cuz it depends on current/potential equips/unequips)
            generatedDescription += "\n\n<b>Cooldown:</b> " + GetCooldownValueWithHasteMod(itemData.equipmentBaseData.BaseCooldownValue());
        }
        
        generatedDescription += GetStatModifierDescription();

        return generatedDescription;
    }

    private string GetCooldownValueWithHasteMod(float baseCooldownValue)
    {
        PlayerStats stats = Player.instance.stats;

        // The haste bonus this item gives
        float thisItemHaste = itemData.GetLineValueFromStatType(StatType.Haste);

        // If this is the currently equipped item, just get the haste color mod and return factoring in current haste
        if( itemPanelType == ItemPanelType.CurrentlyEquippedCompareItem || itemPanelType == ItemPanelType.EquippedGeneric ){
            return "<color=" + InGameUIManager.TURQUOISE_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(baseCooldownValue / stats.getHaste()) + "s</color>";
        }
        
        // If this is the new item we're looking at...
        float newItemCooldownValue;

        // Get your current haste stats
        float baseHasteValue = stats.hasteBase;
        float hasteMultiplier = stats.hasteMultiplier;
        float hasteFlatBonus = stats.hasteFlatBonus;

        // If there's nothing equipped to compare to (nothing equipped in that slot), set it to green and factor in both current haste and this item's haste
        if(!PlayerInventory.instance.ItemSlotIsFull(itemSlot)){
            newItemCooldownValue = baseCooldownValue / ((baseHasteValue + (hasteFlatBonus + thisItemHaste) + (stats.Wisdom() * PlayerStats.hastePerWisdomPoint)) * hasteMultiplier);
        }
        else{
            // Now knowing we have something equipped, get the haste value from the currently equipped item
            GeneratedEquipment currentlyEquippedItem = PlayerInventory.instance.gear[itemSlot].data;
            float? currentItemHasteBonus = stats.GetBonusForStat( currentlyEquippedItem.equipmentBaseData, StatType.Haste, EntityStats.BonusType.multiplier );

            // If the currently equipped item has haste, make sure to factor it OUT of the calculation
            if(currentItemHasteBonus.HasValue){
                newItemCooldownValue = baseCooldownValue / ((baseHasteValue + (hasteFlatBonus - currentItemHasteBonus.Value + thisItemHaste) + (stats.Wisdom() * PlayerStats.hastePerWisdomPoint)) * hasteMultiplier);
            }
            else{
                newItemCooldownValue = baseCooldownValue / ((baseHasteValue + (hasteFlatBonus + thisItemHaste) + (stats.Wisdom() * PlayerStats.hastePerWisdomPoint)) * hasteMultiplier);
            }
        }
        
        return "<color=" + InGameUIManager.TURQUOISE_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(newItemCooldownValue) + "s</color>";
    }

    private string GetStatModifierDescription()
    {
        string s = "\n";

        // Add primary line text
        StatType primaryLine = itemData.equipmentBaseData.PrimaryItemLine();
        float primaryLineValue = itemData.primaryLineValue;
        if(primaryLineValue > 0){
            s += SetStringForStatValue(primaryLine, primaryLineValue, true, true) + "\n";
        }

        // Add secondary lines
        s += "<size=70%>";
        for(int i = 0; i < EntityStats.numberOfSecondaryLineOptions; i++){
            float bonusValue = itemData.GetLineValueFromStatType((StatType)i);

            bool percent = true;
            if((StatType)i == StatType.HitPoints){
                percent = false;
            }

            if(bonusValue != 0){
                s += SetStringForStatValue((StatType)i, bonusValue, percent);
            }
        }

        return s;
    }

    private string SetStringForStatValue(StatType type, float bonusValue, bool percent=true, bool primaryLine = false)
    {
        string returnString = "\n<b>" + itemData.GetPlayerFacingStatName(type) + ":</b> <color=" + GetColorModForStatValue(bonusValue, type, primaryLine) + ">+";

        // If %
        if(percent){
            returnString += UIUtils.GetTruncatedDecimalForUIDisplay( (bonusValue*100) ) + "%";
        }
        // If flat
        else{
            returnString += UIUtils.GetTruncatedDecimalForUIDisplay( bonusValue );
        }

        return returnString + "</color>";
    }

    // Returns green if increase in value, magenta if decrease, gray if same
    private string GetColorModForStatValue(float thisBonusValue, StatType type, bool primaryLine = false)
    {
        // If nothing to compare to, return green
        if(itemPanelType == ItemPanelType.EquippedGeneric)
            return InGameUIManager.SLIME_GREEN_COLOR;
            
        // If compare and it's a DIFFERENT PRIMARY LINE, return green
        if(primaryLine){
            if(itemPanelType == ItemPanelType.NewItemToCompare && PlayerInventory.instance.ItemSlotIsFull(itemSlot) && type != PlayerInventory.instance.gear[itemSlot].data.equipmentBaseData.PrimaryItemLine())
                return InGameUIManager.SLIME_GREEN_COLOR;
            if(itemPanelType == ItemPanelType.CurrentlyEquippedCompareItem && type != GearSwapUI.newItem.equipmentBaseData.PrimaryItemLine())
                return InGameUIManager.SLIME_GREEN_COLOR;
        }

        float compareItemValue;
        GeneratedEquipment compareItem;

        // If this is the NEW item, compare to currently equipped
        if( itemPanelType == ItemPanelType.NewItemToCompare ){
            // If there's nothing equipped to compare to, return green
            if(!PlayerInventory.instance.ItemSlotIsFull(itemSlot)){
                return InGameUIManager.SLIME_GREEN_COLOR;
            }
            // Get the value from the currently equipped item
            compareItem = PlayerInventory.instance.gear[itemSlot].data;
            compareItemValue = compareItem.GetLineValueFromStatType(type, primaryLine);
        }
        // If this is the CURRENT item, compare to the new item
        else{
            compareItem = GearSwapUI.newItem;
            compareItemValue = compareItem.GetLineValueFromStatType(type, primaryLine);
        }

        // Check for overlap with primary/secondary lines (non-weapons only)
        // This way if something has 2 movement speed values, we factor in both when picking colors
        if(itemSlot == InventoryItemSlot.Helmet){
            if( type == StatType.HitPoints ){
                float maxHPWithoutCurrentHelmet = GetPlayerMaxHPWithoutCurrentHelmet();

                // If this is the % HP increase from a primary line, set the compare values to the flat HP increase from the % * max HP
                if(primaryLine){
                    compareItemValue = maxHPWithoutCurrentHelmet * compareItemValue;
                    thisBonusValue = maxHPWithoutCurrentHelmet * thisBonusValue;
                }

                // Add bonus values
                compareItemValue += GetPrimaryLineOverlapForHelmets( compareItem, primaryLine, maxHPWithoutCurrentHelmet );
                thisBonusValue += GetPrimaryLineOverlapForHelmets( itemData, primaryLine, maxHPWithoutCurrentHelmet );
            }
        }
        else if( itemSlot != InventoryItemSlot.Weapon ){
            compareItemValue += GetPrimaryLineOverlapForNonWeapons( compareItem, type, primaryLine );
            thisBonusValue += GetPrimaryLineOverlapForNonWeapons( itemData, type, primaryLine );
        }

        return GetColorCodeFromComparison(thisBonusValue, compareItemValue);
    }

    #region Handle HP Bonus Overlap For Helmets
        private float GetPlayerMaxHPWithoutCurrentHelmet()
        {
            // If nothing's equipped, just return your current HP
            if( !PlayerInventory.instance.ItemSlotIsFull(InventoryItemSlot.Helmet) ){
                return Player.instance.health.maxHitpoints;
            }

            PlayerStats stats = Player.instance.stats;

            float con = stats.Constitution();
            float hpBonusPerCon = PlayerStats.maxHitPointBonusPerConstitutionPoint;
            float hpMultiplier = stats.maxHitPointsMultiplier;
            float hpFlatBonus = stats.maxHitPointsFlatBonus;

            GeneratedEquipment currentlyEquippedHelmet = PlayerInventory.instance.gear[InventoryItemSlot.Helmet].data;
            float? currentHelmetHPMultiplier = stats.GetBonusForStat( currentlyEquippedHelmet.equipmentBaseData, StatType.HitPoints, EntityStats.BonusType.multiplier );
            float? currentHelmetHPFlatBonus = stats.GetBonusForStat( currentlyEquippedHelmet.equipmentBaseData, StatType.HitPoints, EntityStats.BonusType.flat );

            if(!currentHelmetHPMultiplier.HasValue){
                currentHelmetHPMultiplier = 0f;
            }
            if(!currentHelmetHPFlatBonus.HasValue){
                currentHelmetHPFlatBonus = 0f;
            }

            return (con * hpBonusPerCon) * (hpMultiplier - currentHelmetHPMultiplier.Value) + (hpFlatBonus - currentHelmetHPFlatBonus.Value);
        }

        // Handle things differently if HP bc 1 is a % and the other flat
        private float GetPrimaryLineOverlapForHelmets( GeneratedEquipment item, bool primaryLine, float maxHP )
        {
            // If this is the primary line % HP increase, check for secondary flat HP increase and add that
            if(primaryLine){
                // flatHPIncrease
                return item.GetLineValueFromStatType( StatType.HitPoints, false );
            }

            // If this is the secondary version of the primary line, add the primary
            else{
                float percentHPIncrease = item.GetLineValueFromStatType( StatType.HitPoints, true );
                return maxHP * percentHPIncrease;
            }
        }
    #endregion

    private float GetPrimaryLineOverlapForNonWeapons( GeneratedEquipment item, StatType type, bool primaryLine )
    {
        StatType primaryLineType = item.equipmentBaseData.PrimaryItemLine();

        // If this is the primary line, check for secondary and add that
        if(primaryLine){
            return item.GetLineValueFromStatType( primaryLineType, false );
        }
        // If this is the secondary version of the primary line, add the primary
        else{
            return item.GetLineValueFromStatType( primaryLineType, true );
        }
    }

    private string GetStatVariableValue(string matchString)
    {
        DescriptionVariableCode code = GetCodeFromString(matchString);
        if(code == DescriptionVariableCode.enumSize){
            return "enumSize ERROR";
        }
        
        if(matchString.Contains("+-") || matchString.Contains("-+") || matchString.Contains("++") || matchString.Contains("--") || matchString.Contains("**")){
            Debug.LogError("Variable string cannot contain consecutive operators of the same type. Item Name: " + itemName.text);
            return "ERROR";
        }
        if(matchString.Contains("-*") || matchString.Contains("+*")){
            Debug.LogError("Variable string cannot contain additive operator followed by a *. Item Name: " + itemName.text);
            return "ERROR";
        }
        if(matchString.Contains("*+")){
            Debug.LogError("Variable string cannot contain * followed by \"+\". Item Name: " + itemName.text);
            return "ERROR";
        }

        if((matchString.Contains("+") && matchString.IndexOf("+", matchString.IndexOf("+")) > 0) || (matchString.Contains("-") && !matchString.Contains("*-") && matchString.IndexOf("-", matchString.IndexOf("-")) > 0)){
            Debug.LogError("Invalid description variable: Two matching operators found. Ignoring second operator. Item Name: " + itemName.text);
        }
        if(matchString.Contains("+") && matchString.Contains("-") && !matchString.Contains("*-") && (matchString.IndexOf("+", matchString.IndexOf("-")) > 0 || matchString.IndexOf("-", matchString.IndexOf("+")) > 0)){
            Debug.LogError("Invalid description variable: Cannot use both add and subtract. Ignoring second operator. Item Name: " + itemName.text);
        }

        // === Set numeric values ===
        // Get the percent value (after the ":")
        float percent = 100;  // Set default percent value to 100% (if no # specified, assume the entire stat value)
        int colonIndex = matchString.IndexOf(":");
        int commaIndex = matchString.IndexOf(",");  // Get the index of the first comma, if there is one
        if(colonIndex > -1){
            percent = GetNumberFromMatchStringGivenIndices(colonIndex, commaIndex, matchString);
        }

        // Get the * value
        float multiplier = 1;  // Set default value to 1 (if no # specified, assume nothing is being multiplied)
        int starIndex = matchString.IndexOf("*");
        commaIndex = matchString.IndexOf(",",commaIndex+1);  // Get the index of the next comma, if there is one
        if(starIndex > -1){
            multiplier = GetNumberFromMatchStringGivenIndices(starIndex, commaIndex, matchString);
        }

        // Get the + value
        float flatAddition = 0;  // Set default value to 0 (if no # specified, assume nothing is being added)
        int additionIndex = matchString.IndexOf("+");
        if(additionIndex < 0 && matchString.Contains("-")){      // If there is no + operator, try for -
            if( matchString.Contains("*-") ){
                // If the string contains "*-", search starting from beyond that point
                additionIndex = matchString.IndexOf("-", matchString.IndexOf("*-")+1);
            }
            else{
                additionIndex = matchString.IndexOf("-");
            }            
        }
        if(additionIndex > -1){
            flatAddition = GetNumberFromMatchStringGivenIndices(additionIndex, -1, matchString);
        }

        // Set the starting value to the stat
        float totalValue = GetStartingValueFromStatCode(code);

        totalValue = (totalValue * (percent * 0.01f)) * multiplier + flatAddition;

        string startColor = "<color=";
        if( itemPanelType == ItemPanelType.EquippedGeneric ){
            startColor += InGameUIManager.SLIME_GREEN_COLOR + ">";
        }
        // Only weapons scale with rarity
        else if(itemSlot != InventoryItemSlot.Weapon){
            // If it's the same item, make it neutral; otherwise, green
            ItemID itemID = itemData.equipmentBaseData.ItemID();
            startColor += GetColorCodeFromComparison(itemID) + ">";
        }
        // For weapons, set the color of the text according to comparison (if necessary) (otherwise, default to green)
        else{
            ItemRarity compareRarity = ItemRarity.enumSize;
            if( itemPanelType == ItemPanelType.NewItemToCompare ){
                if(!PlayerInventory.instance.ItemSlotIsFull(itemSlot)){
                    startColor += InGameUIManager.SLIME_GREEN_COLOR + ">";
                }
                else{
                    compareRarity = PlayerInventory.instance.gear[itemSlot].data.rarity;
                }
            }
            else{
                compareRarity = GearSwapUI.newItem.rarity;
            }

            if(compareRarity != ItemRarity.enumSize){
                startColor += GetColorCodeFromComparison((int)rarity, (int)compareRarity) + ">";
            }
        }
        return startColor + UIUtils.GetTruncatedDecimalForUIDisplay(Mathf.Abs(totalValue)) + "</color>";
    }

    #region Color Compare Helpers
        private string GetColorCodeFromComparison( float thisValue, float otherValue )
        {
            if( thisValue < otherValue){
                return InGameUIManager.MAGENTA_COLOR;
            }
            else if( thisValue == otherValue ){
                return InGameUIManager.TURQUOISE_COLOR;
            }
            else{
                return InGameUIManager.SLIME_GREEN_COLOR;
            }
        }

        private string GetColorCodeFromComparison(ItemID thisID)
        {
            if( itemPanelType == ItemPanelType.CurrentlyEquippedCompareItem && thisID == GearSwapUI.newItem.equipmentBaseData.ItemID() ){
                return InGameUIManager.TURQUOISE_COLOR;
            }
            else if( itemPanelType == ItemPanelType.NewItemToCompare && PlayerInventory.instance.ItemSlotIsFull(itemSlot) && thisID == PlayerInventory.instance.gear[itemSlot].data.equipmentBaseData.ItemID() ){
                return InGameUIManager.TURQUOISE_COLOR;
            }
            else{
                return InGameUIManager.SLIME_GREEN_COLOR;
            }
        }
    #endregion

    private float GetNumberFromMatchStringGivenIndices(int startingIndex, int commaIndex, string matchString)
    {
        int valueIndex = startingIndex + 1;
        int length = commaIndex - valueIndex;
        if(commaIndex < 0){
            length = matchString.IndexOf("}") - valueIndex;
        }
        System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
        return float.Parse(matchString.Substring(valueIndex,length), styles);
    }

    private DescriptionVariableCode GetCodeFromString(string matchString)
    {
        // Loop through all possible codes; if there's a match, return it
        for(int i = 0; i < (int)DescriptionVariableCode.enumSize; i++){
            if(matchString.Contains( ((DescriptionVariableCode)i).ToString() )){
                return (DescriptionVariableCode)i;
            }
        }
        Debug.LogError("No code match found for description variable: " + matchString);
        return DescriptionVariableCode.enumSize;
    }

    private float GetStartingValueFromStatCode(DescriptionVariableCode code)
    {
        switch(code){
            // Primary Stats
            case DescriptionVariableCode.STR:
                return Player.instance.stats.Strength();
            case DescriptionVariableCode.DEX:
                return Player.instance.stats.Dexterity();
            case DescriptionVariableCode.INT:
                return Player.instance.stats.Intelligence();
            case DescriptionVariableCode.WIS:
                return Player.instance.stats.Wisdom();
            case DescriptionVariableCode.CHA:
                return Player.instance.stats.Charisma();
            case DescriptionVariableCode.CON:
                return Player.instance.stats.Constitution();

            // Damage Values
            case DescriptionVariableCode.SD:
            case DescriptionVariableCode.DD:
            case DescriptionVariableCode.ID:
            case DescriptionVariableCode.WD:
                return CalculateDamageValue(code);

            // Secondary Stats
            case DescriptionVariableCode.ATS:
                return Player.instance.stats.getAttackSpeed();
            case DescriptionVariableCode.MOS:
                return Player.instance.stats.getMoveSpeed();
            case DescriptionVariableCode.DEF:
                return Player.instance.stats.getDefense();
            case DescriptionVariableCode.DOC:
                return Player.instance.stats.getDodgeChance();
            case DescriptionVariableCode.CRC:
                return Player.instance.stats.getCritChance();
            case DescriptionVariableCode.CRD:
                return Player.instance.stats.getCritDamage();
            case DescriptionVariableCode.TDR:
                return Player.instance.stats.getTrapDamageResist();
            case DescriptionVariableCode.HAS:
                return Player.instance.stats.getHaste();
        }
        Debug.LogError("No stat found for: " + code);
        return -1;
    }

    private float CalculateDamageValue(DescriptionVariableCode code)
    {
        // If this is not a weapon or not a new item to compare OR it IS a new item to compare but we don't currently have anything equipped in that slot,
        // just calculate the value normally
        if( itemSlot != InventoryItemSlot.Weapon || itemPanelType != ItemPanelType.NewItemToCompare || !PlayerInventory.instance.ItemSlotIsFull(itemSlot) ){
            switch(code){
                case DescriptionVariableCode.SD:
                    return Player.instance.stats.getSTRDamage(false).damageValue;
                case DescriptionVariableCode.DD:
                    return Player.instance.stats.getDEXDamage(false).damageValue;
                case DescriptionVariableCode.ID:
                    return Player.instance.stats.getINTDamage(false).damageValue;
                case DescriptionVariableCode.WD:
                    return Player.instance.stats.getWISDamage(false).damageValue;
            }
        }

        // Just for weapons -> factoring in damage multiplier primary line

        PlayerStats stats = Player.instance.stats;

        float baseValue = 1f;
        float multiplier = 1f;
        float flatBonus = 0f;
        float? currentItemBonus = 0f;

        GeneratedEquipment currentlyEquippedWeapon = PlayerInventory.instance.gear[itemSlot].data;

        switch(code){
            case DescriptionVariableCode.SD:
                baseValue = stats.Strength();
                multiplier = stats.STRDamageMultiplier;
                flatBonus = stats.STRDamageFlatBonus;
                currentItemBonus = stats.GetBonusForStat( currentlyEquippedWeapon.equipmentBaseData, StatType.STRDamage, EntityStats.BonusType.multiplier );
                break;
            case DescriptionVariableCode.DD:
                baseValue = stats.Dexterity();
                multiplier = stats.DEXDamageMultiplier;
                flatBonus = stats.DEXDamageFlatBonus;
                currentItemBonus = stats.GetBonusForStat( currentlyEquippedWeapon.equipmentBaseData, StatType.DEXDamage, EntityStats.BonusType.multiplier );
                break;
            case DescriptionVariableCode.ID:
                baseValue = stats.Intelligence();
                multiplier = stats.INTDamageMultiplier;
                flatBonus = stats.INTDamageFlatBonus;
                currentItemBonus = stats.GetBonusForStat( currentlyEquippedWeapon.equipmentBaseData, StatType.INTDamage, EntityStats.BonusType.multiplier );
                break;
            case DescriptionVariableCode.WD:
                baseValue = stats.Wisdom();
                multiplier = stats.WISDamageMultiplier;
                flatBonus = stats.WISDamageFlatBonus;
                currentItemBonus = stats.GetBonusForStat( currentlyEquippedWeapon.equipmentBaseData, StatType.WISDamage, EntityStats.BonusType.multiplier );
                break;
            default:
                Debug.LogError("No stat damage value found for: " + code);
                return -1;
        }

        float newItemBonus = itemData.primaryLineValue;

        

        if(currentItemBonus.HasValue){
            return baseValue * (multiplier - currentItemBonus.Value + newItemBonus) + flatBonus;
        }
        else{
            return baseValue * (multiplier + newItemBonus) + flatBonus;
        }
    }
}

/*
    Description Variables
    =====================

    Valid Formats:
    --------------
    {CHA:85}                    ->  StatCode:%Value
    {WIS}                       ->  StatCode    (gets the flat value)
    {INT:0,*2,+5}               ->  * provides a multiplier value, + or - a flat addition or subtraction value
    {INT:0,*2}  {INT:0,+5}      ->  You CAN use just one or the other of * or -
    {DEF,+5}    {ATS,*44}       ->  You CAN get the flat StatCode value and add/subtract/multiply to it
    {WIS:0,*-10}                ->  You CAN multiply by a negative number
    {ATS:0,*0.21,+.54}          ->  Decimals okay, w/ or w/out a leading digit

    Invalid Formats:
    ----------------
    {WIS:0,*+10}                ->  CANNOT specify positive for a multiplier
    {DEX:50,}   {DEX:50,1}      ->  No stray commas; can't specify a number without an operator
    {INT:0,+2,*5}               ->  * MUST come BEFORE + or -
    {ATS,-*44}  {ATS,+*44}      ->  * cannot come after - or + in consecutive positions
    ++, --, +-, -+, **          ->  No consecutive duplicate operators
    {INT:0,-2,-5}               ->  ... Or non-consecutive duplicate operators (- and + count as the same type, so could not do - then + or vice versa)
*/