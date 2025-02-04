﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIGear : ShopUI
{
    public GameObject shopCompareItemPanel;
    public GearSwapUI shopCompareUI;
    [HideInInspector] public ItemPanelGearShop activeCompareItem;

    [SerializeField] private Button compareCancelButton;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseButtonText;

    [SerializeField] private Shop shopInventory;

    public override void OpenShopUI()
    {
        base.OpenShopUI();

        SetShopUIValues();
    }
  
    public override void CloseShopUI( bool closeWithESCKey = false )
    {
        ToggleShopInventoryOn(closeWithESCKey);
        base.CloseShopUI(closeWithESCKey);
    }

    private void SetShopUIValues()
    {
        // From the list of items this shop currently carries, set all item values
        int i = 0;
        foreach(GeneratedEquipment item in shopInventory.inventory){
            ( (ItemPanelGearShop)(itemPanels[i]) ).SetGearItemValues(item, shopInventory.shopJustRestocked);

            i++;

            if(i > itemPanels.Count){
                Debug.LogError("More than five shop items found!");
            }
        }
        shopInventory.shopJustRestocked = false;
    }

    public void ToggleShopInventoryOn( bool closeWithESCKey = false )
    {
        if(closeWithESCKey){
            // Close all expanded panels
            shopCompareUI.gearSwapInventoryUI.OnInventoryClose();
        }        

        shopInventoryPanel.SetActive(true);
        shopCompareItemPanel.SetActive(false);
        InGameUIManager.instance.ToggleInGameGearIconPanel(true);

        leaveShopButton.Select();
    }

    public void ToggleShopCompareOn(bool canAfford)
    {
        shopInventoryPanel.SetActive(false);
        shopCompareItemPanel.SetActive(true);
        InGameUIManager.instance.ToggleInGameGearIconPanel(false);

        compareCancelButton.Select();

        if(activeCompareItem){
            shopCompareUI.OnGearSwapUIOpen(activeCompareItem.item);
            purchaseButtonText.text = "Purchase - " + activeCompareItem.currentCostValue;
        }
        else{
            Debug.LogError("Tried to open compare UI without an active shop item to compare to!");
        }

        if(canAfford){
            purchaseButton.interactable = true;
        }
        else{
            purchaseButton.interactable = false;
        }
    }

    public void OnCancelClicked()
    {
        // Deselect active inventory panel, if necessary
        shopCompareUI.gearSwapInventoryUI.OnInventoryClose();
        
        ToggleShopInventoryOn();
        activeCompareItem = null;
    }

    public void OnPurchaseItemClicked()
    {
        activeCompareItem.PurchaseItem();
        OnCancelClicked();
    }

    public override void SetShopUIInteractable(bool set)
    {
        base.SetShopUIInteractable(set);
        if(shopCompareItemPanel.activeInHierarchy){
            shopCompareUI.SetSwapUIInteractable(set);
            if(set){
                compareCancelButton.Select();
            }
        }
    }
}