﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NonWeaponItem : Equipment
{
    public InventoryItemSlot slot {get; protected set;}
    public bool fire = false;
    [HideInInspector] public bool clearToFire = true;

    [HideInInspector] public AnimationStateController anim;

    public Coroutine cooldownRoutine {get; protected set;}
    public Coroutine durationRoutine {get; protected set;}

    [Tooltip("Optional ONE OFF SFX to play right when the item is triggered - if something needs to loop, handle that in the unique item script")]
    [SerializeField][FMODUnity.EventRef] private string itemActivationSFXOneOff;
    
    protected Player player;

    protected virtual void Awake()
    {
        player = Player.instance;
        anim = player.GetComponentInChildren<AnimationStateController>();
    }

    // This gets called when this ability's key first gets pressed
    protected virtual void TriggerAbility()
    {
        Debug.Log(data.equipmentBaseData.name + " is clear to fire: " + clearToFire);
        if(clearToFire)
        {
            clearToFire = false;
            anim.animator.SetBool("IsUse" + slot, true);

            if(itemActivationSFXOneOff != ""){
                AudioManager.Instance.PlaySFX(itemActivationSFXOneOff, Player.instance.gameObject);
            }
        }
    }

    // This gets called when this ability's duration begins
    protected void StartDurationRoutine()
    {
        Debug.Log("Duration started for " + data.equipmentBaseData.name);
        anim.animator.SetBool("IsUse" + slot, false);
        durationRoutine = StartCoroutine(Duration());
    }

    // This gets called when this ability's duration ends and its cooldown begins
    public virtual void ResetItemAndTriggerCooldown()
    {
        Debug.Log("Cooldown started for " + data.equipmentBaseData.name);
        cooldownRoutine = StartCoroutine(CoolDown());
        clearToFire = true;
    }

    private IEnumerator Duration()
    {
        InGameUIManager.instance.SetItemIconColor(slot, InGameUIManager.SLIME_GREEN_COLOR);
        InGameUIManager.instance.EnableCooldownStateForControlButtonUI(slot, true);

        yield return new WaitForSeconds(data.equipmentBaseData.Duration());
        anim.animator.SetTrigger("Duration" + slot.ToString());

        durationRoutine = null;
    }

    // Always triggered at the end of the item's Reset function, which is called once Duration is complete
    private IEnumerator CoolDown()
    {
        float cooldown = data.equipmentBaseData.BaseCooldownValue() * (1 / Player.instance.stats.getHaste());

        InGameUIManager.instance.StartCooldownForItem(slot, cooldown);
        yield return new WaitForSeconds(cooldown);

        // NOTE: For this to work (& the one in Duration), this trigger has to use the item slot exactly as it's spelled in the type
        // (in the animator, make sure to use the InventoryItemSlot values exactly as: "Helmet", "Accessory", "Legs")
        anim.animator.SetTrigger("Cooldown" + slot.ToString());

        cooldownRoutine = null;
    }

    // Any children with specific coroutines to deal with also need their own version of this with base.ManageCoroutinesOnUnequip(); at the beginning
    public override void ManageCoroutinesOnUnequip()
    {
        // If we're in the duration, go to cooldown and then end cooldown (but continue the UI)
        // Item SLOT is still on cooldown, can't use this ability yet, but we switched items so need to stop these instances specifically
        if(durationRoutine != null){
            StopCoroutine(durationRoutine);
            anim.animator.SetTrigger("Duration" + slot.ToString());
            anim.animator.SetTrigger("Cooldown" + slot.ToString());
            InGameUIManager.instance.StartCooldownForItem(slot, data.equipmentBaseData.BaseCooldownValue() * (1 / Player.instance.stats.getHaste()));
        }

        // If mid-cooldown, just stop the cooldown coroutine (but continue the UI)
        if(cooldownRoutine != null){
            StopCoroutine(cooldownRoutine);
            anim.animator.SetTrigger("Cooldown" + slot.ToString());
        }
    }
}
