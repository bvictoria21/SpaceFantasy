﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Longsword : Equipment
{
    private bool isAttacking;
    private bool holdingAttack;
    private int heldEffectCounter = 0;
    private int maxHeldEffect = 2;
    private float[] damageModifier = new float[] { 0.75f, 1, 1.25f };

    private int bonusStackCounter = 0;
    private int bonusStackMax = 3;
    private float bonusDuration = 3;
    private float attackSpeedModifierBonus = 0.2f;

    private Player player;
    private Movement movement;
    private AnimationStateController playerAnim;
    private Collider swordCollider;
    private Coroutine attackSpeedRoutine;

    // Start is called before the first frame update
    void Start()
    {
        player = Player.instance;
        movement = player.GetComponentInChildren<Movement>();

        playerAnim = player.GetComponentInChildren<AnimationStateController>();
        playerAnim.endAttack.AddListener(disableAttacking);

        itemModel.GetComponentInChildren<LongswordCollisionWatcher>().hitEvent.AddListener(DealDamage);
        swordCollider = itemModel.GetComponentInChildren<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(InputManager.instance.isAttacking && !isAttacking)
        {
            isAttacking = true;
            holdingAttack = true;
            movement.isAttacking = true;
            movement.lockLookDirection = true;
        }

        if(holdingAttack && !InputManager.instance.isAttacking)
        {
            holdingAttack = false;
            heldEffectCounter = 0;
        }

        playerAnim.animator.SetBool("IsHoldingAttack", heldEffectCounter > 0);
        playerAnim.animator.SetBool("IsAttacking", isAttacking);
        playerAnim.animator.SetFloat("AttackSpeed", player.stats.getAttackSpeed());
        swordCollider.enabled = playerAnim.attackActive;
    }

    public void DealDamage(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            DamageData damageData = player.stats.getSTRDamage();
            damageData.damageValue = damageModifier[heldEffectCounter] * damageData.damageValue;

            bool killed = other.GetComponent<EntityHealth>().Damage(damageData, DamageSourceType.Player);
            
            if(killed)
            {
                if(bonusStackCounter < bonusStackMax)
                    ++bonusStackCounter;

                if(attackSpeedRoutine != null)
                    StopCoroutine(attackSpeedRoutine);
                
                player.stats.SetBonusForStat(this, StatType.AttackSpeed, EntityStats.BonusType.multiplier, bonusStackCounter * attackSpeedModifierBonus);
                attackSpeedRoutine = StartCoroutine(bonusDecayRoutine());
            }

            GameManager.instance.EnableHitStop();
        }
        else
        {
            other.GetComponent<PropJumpBreak>().BreakProp();
        }
    }

    public void disableAttacking()
    {
        if(holdingAttack)
        {
            if(heldEffectCounter < maxHeldEffect)
            {
                ++heldEffectCounter;
            }
            else
            {
                heldEffectCounter = 0;
            }
        }
        else
        {
            isAttacking = false;
            movement.isAttacking = false;
            movement.lockLookDirection = false;
        }
    }

    private IEnumerator bonusDecayRoutine()
    {
        yield return new WaitForSeconds(bonusDuration);
        
        --bonusStackCounter;
        player.stats.SetBonusForStat(this, StatType.AttackSpeed, EntityStats.BonusType.multiplier, bonusStackCounter * attackSpeedModifierBonus);

        if(bonusStackCounter > 0)
            attackSpeedRoutine = StartCoroutine(bonusDecayRoutine());
    }

    public override void ManageCoroutinesOnUnequip()
    {
        // If the coroutine was running, stop it and revert the stat bonus
        if(bonusStackCounter > 0 && attackSpeedRoutine != null){
            player.stats.SetBonusForStat(this, StatType.AttackSpeed, EntityStats.BonusType.multiplier, 0);
            StopCoroutine(attackSpeedRoutine);
        }
    }
}
