﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemCooldownUI : MonoBehaviour
{
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private InventoryItemSlot itemSlot;

    public bool isActive {get; private set;}

    public float counter;

    void Start()
    {
        isActive = false;
    }

    public InventoryItemSlot GetItemSlot()
    {
        return itemSlot;
    }

    public void StartCooldownCountdown(float value)
    {
        isActive = true;
        counter = value;   
        SetTextToCounterValue();     
    }

    public void EndCooldownCountdown()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public void SetTextToCounterValue()
    {
        cooldownText.text = UIUtils.GetTruncatedDecimalForUIDisplay(counter) + "";
    }
}
