﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalContentManager : MonoBehaviour
{
    public Dictionary<JournalContentID, JournalContent> contentDatabase {get; private set;}
    public Dictionary<JournalContentID, bool> journalUnlockStatusDatabase {get; private set;}

    [SerializeField] private Sprite journalLockedSprite;

    void Awake()
    {
        journalUnlockStatusDatabase = new Dictionary<JournalContentID, bool>();
        contentDatabase = new Dictionary<JournalContentID, JournalContent>();
        LoadAllJournalContentObjects();
    }

    private void LoadAllJournalContentObjects()
    {
        // Load in crew page data
        LoadContentFromLocation("JournalContent/Crew");

        // Load in location page data
        LoadContentFromLocation("JournalContent/Location");

        // Load in stat page data
        LoadContentFromLocation("JournalContent/Stats");

        // Load in enemy page data
        LoadContentFromLocation("JournalContent/Enemies");

        // Load in item page data
        LoadContentFromLocation("JournalContent/Items");
    }

    private void LoadContentFromLocation(string location)
    {
        Object[] journalContentList = Resources.LoadAll(location, typeof(JournalContent));
        foreach(Object c in journalContentList){
            JournalContent content = (JournalContent)c;
            if(contentDatabase.ContainsKey(content.InternalID())){
                continue;
            }
            contentDatabase.Add(content.InternalID(), content);
            journalUnlockStatusDatabase.Add(content.InternalID(), !content.LockedOnStart());
        }
    }

    public Sprite JournalLockedSprite()
    {
        return journalLockedSprite;
    }
    
    public void UnlockJournalEntry(JournalContentID[] contentIDs)
    {
        bool flag = false;
        foreach(JournalContentID id in contentIDs){
            if(!journalUnlockStatusDatabase.ContainsKey(id)){
                Debug.LogWarning("No content id key found in journalUnlockStatusDatabase for id: " + id);
                continue;
            }
            if(journalUnlockStatusDatabase[id]){
                continue;
            }

            // If any have not yet been set active, set flag to true so that we can enable the UI alert after
            flag = true;

            // TODO: need to unlock for ITEM type triggers... the update function in StoryManager is never called on those so we need to set that up elsewhere

            // TODO: does the journal content display stuff need to be alerted? or does it check every time it's opened...?

            journalUnlockStatusDatabase[id] = true;
        }

        if(flag){
            AlertTextUI.instance.EnableOpenJournalAlert();
            StartCoroutine(AlertTextUI.instance.RemoveAlertAfterSeconds());
        }        
    }
}
