﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager instance;

    [SerializeField] private GameObject inGameUIPanel;
    [SerializeField] private GameObject inGameUIGearIconPanel;  // Sometimes toggled separately from the rest of the in game UI

    [SerializeField] private Image inGameWeaponIMG;
    [SerializeField] private Image inGameAccessoryIMG;
    [SerializeField] private Image inGameHelmetIMG;
    [SerializeField] private Image inGameBootsIMG;

    [SerializeField] private Sprite emptySlotWeaponIcon;
    [SerializeField] private Sprite emptySlotAccessoryIcon;
    [SerializeField] private Sprite emptySlotHelmetIcon;
    [SerializeField] private Sprite emptySlotBootsIcon;

    [SerializeField] private GameObject darkBackgroundPanel;

    public DeathScreenUI deathScreen;
    public PauseMenu pauseMenu;

    public InventoryUI inventoryUI;
    [SerializeField] private GameObject inventoryUIPanel;
    public bool inventoryIsOpen {get; private set;}

    public GearSwapUI gearSwapUI;
    [SerializeField] private GameObject gearSwapUIPanel;
    public bool gearSwapIsOpen {get; private set;}

    public ShopUI brynShopUI;
    public ShopUIStellan stellanShopUI;
    public ShopUI doctorShopUI;
    public ShopUI weaponsShopUI;

    [SerializeField] private TMP_Text permanentCurrencyValue;
    [SerializeField] private TMP_Text tempCurrencyValue;

    [SerializeField] private GameObject healthUIContainer;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    private float maxHealthValue;
    private float currentHPValue;

    [SerializeField] private TMP_Text healthPotionValue;

    public BossHealthBar bossHealthBar;
    public TimerUI timerUI;
    

    void Awake()
    {
        if( instance ){
            Destroy(gameObject);
        }
        else{
            instance = this;
        }
    }

    void Start()
    {
        SetTempCurrencyValue(PlayerInventory.instance.tempCurrency);
        SetPermanentCurrencyValue(PlayerInventory.instance.permanentCurrency);
        SetHealthPotionValue(PlayerInventory.instance.healthPotionQuantity);

        if(GameManager.instance.currentSceneName != GameManager.LICH_ARENA_STRING_NAME){
            ClearAllItemUI();
        }
    }

    // Called when you enter dialogue or other similar things
    public void SetGameUIActive(bool set)
    {
        inGameUIPanel.SetActive(set);
    }

    public void ToggleInGameGearIconPanel(bool set)
    {
        inGameUIGearIconPanel.SetActive(set);
    }

    public void ToggleRunUI(bool setRunUIActive)
    {
        ToggleInGameGearIconPanel(setRunUIActive);
        tempCurrencyValue.gameObject.SetActive(setRunUIActive);
        healthUIContainer.SetActive(setRunUIActive);

        // Reset timer
        InputManager.instance.RunGameTimer(setRunUIActive, setRunUIActive);
        GameManager.instance.gameTimer.ResetTimer();
    }

    // Called when player input opens or closes the inventory
    public void SetInventoryUIActive(bool set)
    {
        if(!set){
            inventoryUI.OnInventoryClose();
        }

        inGameUIGearIconPanel.SetActive(!set);
        darkBackgroundPanel.SetActive(set);
        inventoryUIPanel.SetActive(set);
        inventoryIsOpen = set;

        if(set){
            inventoryUI.OnInventoryOpen();
            AlertTextUI.instance.ToggleAlertText(false);
        }
        else{
            AlertTextUI.instance.ToggleAlertText(true);
        }
    }

    // Called when the player goes to pick up a new item
    public void SetGearSwapUIActive(bool set, GeneratedEquipment item)
    {
        inGameUIGearIconPanel.SetActive(!set);
        gearSwapUIPanel.SetActive(set);
        gearSwapIsOpen = set;

        if(set){
            gearSwapUI.OnGearSwapUIOpen(item);
            AlertTextUI.instance.DisableAlert();
        }
        else{
            AlertTextUI.instance.EnableItemPickupAlert();
        }
    }

    public void SetGearItemUI(InventoryItemSlot itemSlot, Sprite _icon)
    {
        switch(itemSlot){
            case InventoryItemSlot.Weapon:
                inGameWeaponIMG.sprite = _icon;
                inGameWeaponIMG.preserveAspect = true;
                // inGameWeaponIMG.SetNativeSize();                
                break;
            case InventoryItemSlot.Accessory:
                inGameAccessoryIMG.sprite = _icon;
                inGameAccessoryIMG.preserveAspect = true;
                // inGameAccessoryIMG.SetNativeSize();
                break;
            case InventoryItemSlot.Helmet:
                inGameHelmetIMG.sprite = _icon;
                inGameHelmetIMG.preserveAspect = true;
                // inGameHelmetIMG.SetNativeSize();
                break;
            case InventoryItemSlot.Legs:
                inGameBootsIMG.sprite = _icon;
                inGameBootsIMG.preserveAspect = true;
                // inGameBootsIMG.SetNativeSize();
                break;
            default:
                Debug.LogError("No item icon found for slot: " + itemSlot.ToString());
                return;
        }
    }

    public void ClearAllItemUI()
    {
        ClearItemUI(InventoryItemSlot.Weapon);
        ClearItemUI(InventoryItemSlot.Accessory);
        ClearItemUI(InventoryItemSlot.Helmet);
        ClearItemUI(InventoryItemSlot.Legs);
    }

    public void ClearItemUI(InventoryItemSlot itemSlot)
    {
        switch(itemSlot){
            case InventoryItemSlot.Weapon:
                inGameWeaponIMG.sprite = emptySlotWeaponIcon;
                inGameWeaponIMG.preserveAspect = true;
                // inGameWeaponIMG.SetNativeSize();
                break;
            case InventoryItemSlot.Accessory:
                inGameAccessoryIMG.sprite = emptySlotAccessoryIcon;
                inGameAccessoryIMG.preserveAspect = true;
                // inGameAccessoryIMG.SetNativeSize();
                break;
            case InventoryItemSlot.Helmet:
                inGameHelmetIMG.sprite = emptySlotHelmetIcon;
                inGameHelmetIMG.preserveAspect = true;
                // inGameHelmetIMG.SetNativeSize();
                break;
            case InventoryItemSlot.Legs:
                inGameBootsIMG.sprite = emptySlotBootsIcon;
                inGameBootsIMG.preserveAspect = true;
                // inGameBootsIMG.SetNativeSize();
                break;
            default:
                Debug.LogError("No item icon found for slot: " + itemSlot.ToString());
                return;
        }
    }

    public Sprite GetDefaultItemIconForSlot(InventoryItemSlot itemSlot)
    {
        switch(itemSlot){
            case InventoryItemSlot.Weapon:
                return emptySlotWeaponIcon;
            case InventoryItemSlot.Accessory:
                return emptySlotAccessoryIcon;
            case InventoryItemSlot.Helmet:
                return emptySlotHelmetIcon;
            case InventoryItemSlot.Legs:
                return emptySlotBootsIcon;
            default:
                Debug.LogError("No item icon found for slot: " + itemSlot.ToString());
                return null;
        }
    }

    public void SetPermanentCurrencyValue(int money)
    {
        permanentCurrencyValue.text = "" + money;
    }

    public void SetTempCurrencyValue(int money)
    {
        tempCurrencyValue.text = "" + money;
    }

    public void SetCurrentHealthValue(float _NewCurrentHP)
    {
        currentHPValue = _NewCurrentHP;

        if( _NewCurrentHP > maxHealthValue ){
            Debug.LogError("Current HP set greater than max HP!");
            currentHPValue = maxHealthValue;
        }

        healthText.text = Mathf.FloorToInt(currentHPValue) + " / " + Mathf.FloorToInt(maxHealthValue);

        healthSlider.value = currentHPValue;        
    }

    public void SetMaxHealthValue(float _NewMaxHP)
    {
        maxHealthValue = _NewMaxHP;
        healthSlider.maxValue = _NewMaxHP;

        healthText.text = Mathf.FloorToInt(currentHPValue) + " / " + Mathf.FloorToInt(maxHealthValue);

        if(currentHPValue != 0){
            SetCurrentHealthValue(currentHPValue);
        }
    }

    public void SetHealthPotionValue(int numPotions)
    {
        healthPotionValue.text = "" + numPotions;
    }

    public void OpenNPCShop(SpeakerData shopkeeper)
    {
        AlertTextUI.instance.DisableAlert();
        if(shopkeeper.SpeakerID() == SpeakerID.Bryn){
            brynShopUI.OpenShopUI();
        }
        else if(shopkeeper.SpeakerID() == SpeakerID.Stellan){
            stellanShopUI.OpenShopUI();
        }
        else if(shopkeeper.SpeakerID() == SpeakerID.Doctor){
            doctorShopUI.OpenShopUI();
        }
        else if(shopkeeper.SpeakerID() == SpeakerID.Rhian){
            weaponsShopUI.OpenShopUI();
        }
        else{
            Debug.LogError("Failed to open shop for NPC " + shopkeeper.SpeakerID());
        }
    }

    public void CloseNPCShop(SpeakerData shopkeeper)
    {
        AlertTextUI.instance.EnableShopAlert();
        if(shopkeeper.SpeakerID() == SpeakerID.Bryn){
            brynShopUI.CloseShopUI();
        }
        else if(shopkeeper.SpeakerID() == SpeakerID.Stellan){
            stellanShopUI.CloseShopUI();
        }
        else if(shopkeeper.SpeakerID() == SpeakerID.Doctor){
            doctorShopUI.CloseShopUI();
        }
        else if(shopkeeper.SpeakerID() == SpeakerID.Rhian){
            weaponsShopUI.CloseShopUI();
        }
        else{
            Debug.LogError("Failed to close shop for NPC " + shopkeeper.SpeakerID());
        }
    }
}
