﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    public GameObject textContainer;
    public GameObject textPrefab;

    private List<FloatingText> floatingTexts = new List<FloatingText>();

    private void Start()
    {
        if(GameManager.instance.InSceneWithRandomGeneration()){
            FindObjectOfType<FloorGenerator>().OnGenerationComplete.AddListener(StartOnGenerationComplete);
        }
        else{
            StartOnGenerationComplete();
        }
    }

    private void StartOnGenerationComplete()
    {
        EntityHealth healthScript = Player.instance.GetComponent<EntityHealth>();
        healthScript.OnDeath.AddListener(Hide);
    }

    private void Update()
    {
        foreach (FloatingText txt in floatingTexts)
            txt.UpdateFloatingText();
    }

    public void Show(string msg, int fontSize, Vector3 position, Vector3 motion, float duration, GameObject parent, string type, bool isCrit)
    {
        FloatingText floatingText = GetFloatingText(parent, type, duration);

        switch(type)
        {
            case "damage-player":
                floatingText.stat += float.Parse(msg);
                floatingText.txt.text = "<color=" + InGameUIManager.MAGENTA_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(floatingText.stat) + "</color>";
                break;
            case "damage-enemy":
                floatingText.stat += float.Parse(msg);
                if(!isCrit)
                    floatingText.txt.text = UIUtils.GetTruncatedDecimalForUIDisplay(floatingText.stat);
                else{
                    floatingText.txt.text = "<size=150%><color=" + InGameUIManager.STR_GOLD_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(floatingText.stat) + "</color></size>";
                }
                break;
            case "health":
                floatingText.stat += float.Parse(msg);
                floatingText.txt.text = "<color=" + InGameUIManager.SLIME_GREEN_COLOR + ">" + UIUtils.GetTruncatedDecimalForUIDisplay(floatingText.stat) + "</color>";
                break;
            case "dodge":
                floatingText.txt.text = "<color=" + InGameUIManager.TURQUOISE_COLOR + ">DODGE</color>";
                break;
            default:
                floatingText.txt.text = msg;
                break;
        }

        floatingText.txt.fontSize = fontSize;

        floatingText.go.transform.position = Camera.main.WorldToScreenPoint(position);
        floatingText.motion = motion;
        floatingText.duration = duration;

        floatingText.Show();
    }

    private FloatingText GetFloatingText(GameObject parent, string type, float duration)
    {
        FloatingText txt = floatingTexts.Find(t => t.active && t.parent == parent && t.type == type);

        if(txt == null)
            txt = floatingTexts.Find(t => !t.active);

        if(txt == null)
        {
            txt = new FloatingText();
            txt.go = Instantiate(textPrefab);
            txt.go.transform.SetParent(textContainer.transform);
            txt.txt = txt.go.GetComponent<TMP_Text>();
            txt.parent = parent;
            txt.type = type;
            txt.lastShown = Time.time;
            //txt.duration = duration;

            floatingTexts.Add(txt);
        }
        else
        {
            txt.parent = parent;
            txt.type = type;
            txt.lastShown = Time.time;
            //txt.duration = duration;
        }

        return txt;
    }

    public void Hide(EntityHealth health)
    {
        foreach(FloatingText txt in floatingTexts)
        {
            txt.Hide();
        }
    }
}
