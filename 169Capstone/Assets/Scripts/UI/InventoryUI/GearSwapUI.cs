﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GearSwapUI : MonoBehaviour
{
    public InventoryUI gearSwapInventoryUI;
    public Button replaceItemButton;
    public Button keepCurrentItemButton;

    private GeneratedEquipment newItem;
    [SerializeField] private InventoryUIItemPanel newItemPanel;

    public void OnGearSwapUIOpen(GeneratedEquipment item)
    {
        gearSwapInventoryUI.SetInventoryItemValues();   // Only set item values, no stat values (is this even necessary? it might be called already)

        newItem = item;
        newItemPanel.SetItemPanelValues(item);
        
        if(gearSwapInventoryUI.itemPanels.Count == 0){
            Debug.LogError("No item panels found in gear swap inventory UI!");
        }
        gearSwapInventoryUI.OnInventoryOpen();

        // If you have an item equipped in that slot, expand that one on open
        if(PlayerInventory.instance.gear[item.equipmentData.ItemSlot()] != null){
            ExpandItemOfSameType();
        }
    }

    private void ExpandItemOfSameType()
    {
        InventoryItemSlot slot = newItem.equipmentData.ItemSlot();

        if(!PlayerInventory.instance.gear[slot]){
            Debug.Log("No item of slot type " + slot.ToString() + " equipped; not expanding item panel.");
            return;
        }

        foreach(InventoryUIItemPanel panel in gearSwapInventoryUI.itemPanels){
            if(panel.GetItemSlot() == slot){
                gearSwapInventoryUI.CardToggle(panel);
            }
        }
    }

    public void CloseGearSwapUI()
    {
        gearSwapInventoryUI.OnInventoryClose();
        InputManager.instance.ToggleCompareItemUI(false, null);    // Close the menu
    }

    public void OnNewItemSelect()
    {
        PlayerInventory.instance.EquipItem(newItem.equipmentData.ItemSlot(), newItem);
        CloseGearSwapUI();
    }

    public void SetSwapUIInteractable(bool set)
    {
        replaceItemButton.interactable = set;
        keepCurrentItemButton.interactable = set;
        gearSwapInventoryUI.SetInventoryInteractable(set);
    }
}
