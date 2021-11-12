﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StoryManager : MonoBehaviour
{
    // Struct for storing data about story beats that changes at runtime (current state stuff)
    public struct BeatStatus{
        public bool beatIsActive;          // If the player did this thing on their latest run (so characters can/should respond to it)
        public int numberOfCompletions;    // The number of times the player has done this thing

        public HashSet<SpeakerID> speakersWithComments; // Things are removed once they no longer have new things to say on a topic

        // Constructor
        public BeatStatus(bool active, int num, List<SpeakerID> speakers){
            speakersWithComments = new HashSet<SpeakerID>();
            // Add all the speakerIDs to the hashset
            for(int i = 0; i < speakers.Count; ++i){
                speakersWithComments.Add(speakers[i]);
            }

            this.beatIsActive = active;
            this.numberOfCompletions = num;
        }

        // Constructor with HashSet instead of List
        public BeatStatus(bool active, int num, HashSet<SpeakerID> speakers){
            speakersWithComments = new HashSet<SpeakerID>();
            this.speakersWithComments = speakers;

            this.beatIsActive = active;
            this.numberOfCompletions = num;
        }
    }

    public static StoryManager instance;    // Singleton

    public int currentRunNumber {get; private set;}

    public Dictionary<StoryBeat,BeatStatus> storyBeatDatabase = new Dictionary<StoryBeat,BeatStatus>();     // All story beats of type Conversation or Killed
    public HashSet<StoryBeat> activeStoryBeats = new HashSet<StoryBeat>();    // For the DialogueManager to see just the active beats

    // TODO: Update their beat status values??? -> UpdateBeatStatus() can do that for them too, just need to call it somewhere
    // But if calling them to set them active somewhere, like in the DialogueManager, should also set them inactive after (end of a run presumably)
    public Dictionary<StoryBeatItem,BeatStatus> itemStoryBeats = new Dictionary<StoryBeatItem,BeatStatus>();  // All item dialogue triggers
    public Dictionary<StoryBeat,BeatStatus> genericStoryBeats = new Dictionary<StoryBeat,BeatStatus>();     // For attemptBarter, lowHP, default, and repeatable

    void Awake()
    {
        // Make this a singleton so that it can be accessed from anywhere and there's only one
        if( instance ){
            Destroy(gameObject);
        }
        else{
            instance = this;
        }
        DontDestroyOnLoad(gameObject);      // ... right? (VERIFY THIS)

        currentRunNumber = 1;

        // Load in the StoryBeats (only of type Killed and Conversation) from the StoryBeats folder in Resources
        Object[] storyBeatList = Resources.LoadAll("SpecificStoryBeats", typeof(StoryBeat));
        foreach(Object s in storyBeatList){
            StoryBeat beat = (StoryBeat)s;
            // Add the storybeat to the dictionary
            storyBeatDatabase.Add( beat, new BeatStatus( false, 0, beat.GetSpeakersWithComments() ) );
        }

        // Load in the ItemDialogueTriggers from the ItemTriggers folder in Resources
        Object[] itemDialogueList = Resources.LoadAll("ItemStoryBeats", typeof(StoryBeatItem));
        foreach(Object i in itemDialogueList){
            StoryBeatItem item = (StoryBeatItem)i;
            itemStoryBeats.Add(item,new BeatStatus( false, 0, item.GetSpeakersWithComments() ));
        }

        // Load in the generic story beats from the GenericStoryBeats folders in Resources
        Object[] genericDialogueList = Resources.LoadAll("GenericStoryBeats", typeof (StoryBeat));
        foreach(Object g in genericDialogueList){
            StoryBeat beat = (StoryBeat)g;
            genericStoryBeats.Add( beat, new BeatStatus( false, 0, beat.GetSpeakersWithComments() ) );
        }
    }

    // Increment when you BEGIN a new run (when leaving the hub world)?
    // TODO: Call this when you begin a new run (probably Game Manager)
    public void IncrementRunNumber()
    {
        currentRunNumber++;
    }

    // Returns true if the speakerID is in that beat's speaker list at this time
    public bool SpeakerIsInSpeakerList(StoryBeat beat, SpeakerID speakerID)
    {
        StoryBeatType beatType = beat.GetBeatType();
        bool flag = true;

        // If it's a Killed Event or Conversation Event
        if((beatType == StoryBeatType.killedBy || beatType == StoryBeatType.creatureKilled || beatType == StoryBeatType.dialogueCompleted) && (!BeatIsInDatabase(beat) || !storyBeatDatabase[beat].speakersWithComments.Contains(speakerID))){
            flag = false;
        }
        // If it's an item
        else if(beatType == StoryBeatType.item){
            StoryBeatItem item = (StoryBeatItem)beat;
            if( !BeatIsInDatabase(beat) || !itemStoryBeats[item].speakersWithComments.Contains(speakerID) ){
                flag = false;
            }
        }
        // If it's a generic storybeat
        else if(((int)beatType <= 5) && (!BeatIsInDatabase(beat) || !genericStoryBeats[beat].speakersWithComments.Contains(speakerID))){
            flag = false;
        }
        if(flag == false){
            Debug.LogError("Failed to access " + speakerID + " in beat " + beat.GetYarnHeadNode() + "'s speaker list!");
        }
        return flag;
    }

    private bool BeatIsInDatabase(StoryBeat beat)
    {
        StoryBeatType beatType = beat.GetBeatType();
        bool flag = true;
        // If it's a Killed Event or Conversation Event
        if((beatType == StoryBeatType.killedBy || beatType == StoryBeatType.creatureKilled || beatType == StoryBeatType.dialogueCompleted) && (!storyBeatDatabase.ContainsKey(beat))){
            flag = false;
        }
        // If it's an item
        else if(beatType == StoryBeatType.item){
            StoryBeatItem item = (StoryBeatItem)beat;
            if( !itemStoryBeats.ContainsKey(item) ){
                flag = false;
            }
        }
        // If it's a generic storybeat
        else if((int)beatType <= 5 && !genericStoryBeats.ContainsKey(beat)){
            flag = false;
        }
        if( flag == false ){
            Debug.LogError("Tried to access beat " + beat.GetYarnHeadNode() + " of type " + beat.GetBeatType() + ", but the beat is not in the database!");
        }
        return flag;
    }

    // Removes a speaker from a given beat's list of speakers who have something to say about that beat
    // Called once an entire branch starting from that beat's head node is completed -> that way we no longer consider that in that speaker's pool of things to say
    public void RemoveSpeakerFromBeat(StoryBeat beat, SpeakerID speakerID)
    {
        StoryBeatType beatType = beat.GetBeatType();
        if( SpeakerIsInSpeakerList(beat, speakerID) ){
            // If Killed or Conversation Event
            if( beatType == StoryBeatType.creatureKilled || beatType == StoryBeatType.killedBy || beatType == StoryBeatType.dialogueCompleted ){
                // VERIFY that this works; might not bc of struct stuff, in which case would have to make a new BeatStatus with a new hash set that doesn't have that speakerID
                storyBeatDatabase[beat].speakersWithComments.Remove(speakerID);
            }
            // If item
            else if( beatType == StoryBeatType.item ){
                StoryBeatItem item = (StoryBeatItem)beat;
                itemStoryBeats[item].speakersWithComments.Remove(speakerID);
            }
            // If generic
            else if( (int)beatType <= 5 ){
                genericStoryBeats[beat].speakersWithComments.Remove(speakerID);
            }
        }
    }

    // Find the StoryBeat corresponding to a given node name string, given a beat type
    public StoryBeat FindBeatFromNodeName(string nodeName, StoryBeatType beatType)
    {
        // If Killed or Conversation Event search storyBeatDatabase
        if( beatType == StoryBeatType.creatureKilled || beatType == StoryBeatType.killedBy || beatType == StoryBeatType.dialogueCompleted ){
            foreach(StoryBeat beat in storyBeatDatabase.Keys){
                if( beat.GetYarnHeadNode().Equals(nodeName) ){
                    return beat;
                }
            }
        }
        // If item search itemDialogueTriggers
        else if( beatType == StoryBeatType.item ){
            foreach(StoryBeatItem beat in itemStoryBeats.Keys){
                if( beat.GetYarnHeadNode().Equals(nodeName) ){
                    return beat;
                }
            }
        }
        // If generic search genericStoryBeats
        else if( (int)beatType <= 5 ){
            foreach(StoryBeat beat in genericStoryBeats.Keys){
                if( beat.GetYarnHeadNode().Equals(nodeName) ){
                    return beat;
                }
            }
        }
        else{
            Debug.LogError("No beats found for beat type: " + beatType);
            return null;
        }
        Debug.LogError("No beats found for node name " + nodeName + " with beat type " + beatType);
        return null;
    }

    // Takes in a string that should match a StoryBeatType enum string value perfectly; convert to the enum value
    public StoryBeatType GetBeatTypeFromString(string beatTypeString)
    {
        StoryBeatType beatType = StoryBeatType.enumSize;
        for(int i = 0; i < (int)StoryBeatType.enumSize; ++i){
            if( ((StoryBeatType)i).ToString() == beatTypeString ){
                beatType = (StoryBeatType)i;
            }
        }
        if( beatType == StoryBeatType.enumSize ){
            Debug.LogError("No story beat type found for string: " + beatTypeString + ". Beat type set to " + beatType + ".");
        }
        return beatType;
    }

    // Update beat status to reflect the given active/inactive bool value + increment by the num provided (either 0 or 1)
    public void UpdateBeatStatus(StoryBeat beat, bool setActive, int incrementCompletionNum)
    {
        StoryBeatType beatType = beat.GetBeatType();
        if( BeatIsInDatabase(beat) ){
            // If Killed or Conversation Event
            if( beatType == StoryBeatType.creatureKilled || beatType == StoryBeatType.killedBy || beatType == StoryBeatType.dialogueCompleted ){
                storyBeatDatabase[beat] = new BeatStatus( setActive, storyBeatDatabase[beat].numberOfCompletions + incrementCompletionNum, storyBeatDatabase[beat].speakersWithComments );
            }
            // If item
            else if( beatType == StoryBeatType.item ){
                StoryBeatItem item = (StoryBeatItem)beat;
                itemStoryBeats[item] = new BeatStatus( setActive, storyBeatDatabase[beat].numberOfCompletions + incrementCompletionNum, storyBeatDatabase[beat].speakersWithComments );
            }
            // If generic
            else if( (int)beatType <= 5 ){
                genericStoryBeats[beat] = new BeatStatus( setActive, storyBeatDatabase[beat].numberOfCompletions + incrementCompletionNum, storyBeatDatabase[beat].speakersWithComments );
            }
        }
    }

    // When you achieve this story beat on a run, increment the # completions and set achieved to true
    private void AchievedStoryBeat(StoryBeat beat)
    {
        if( beat.GetBeatType() != StoryBeatType.creatureKilled && beat.GetBeatType() != StoryBeatType.killedBy && beat.GetBeatType() != StoryBeatType.dialogueCompleted ){
            Debug.LogError("Tried to call AchievedStoryBeat on wrong beat type: " + beat.GetBeatType() + " " + beat.GetYarnHeadNode() + "!");
            return;
        }

        // If there are prereqs, check them
        Dictionary<StoryBeat,int> prereqs = beat.GetPrereqStoryBeats();
        if(prereqs.Count > 0){
            foreach(StoryBeat p in prereqs.Keys){
                // If one of the prereqs has not been met, don't mark this story beat as active
                if( storyBeatDatabase[p].numberOfCompletions < prereqs[p] ){
                    return;
                }
            }
        }
        // If all potential prereqs are met, update the story beat status to be active and increment completion
        UpdateBeatStatus(beat, true, 1);
    }

    // At the end of every run, check if any new story beats have been activated; if so, add them to the list for the DialogueManager to access
    // TODO: call at the end of every run
    public void CheckForNewStoryBeats()
    {
        // Reset active story beats
        activeStoryBeats.Clear();

        // Check if any story beats have been set active, and if so add them to the list for the dialogue runner and set new story available to true
        foreach( StoryBeat beat in storyBeatDatabase.Keys ){
            if( storyBeatDatabase[beat].beatIsActive ){
                activeStoryBeats.Add(beat);
            }
        }

        // Now that the latest achieved have been queued, reset everything that doesn't carry over to not active
        foreach( StoryBeat beat in storyBeatDatabase.Keys ){
            if( !beat.CarriesOver() ){
                UpdateBeatStatus(beat, false, 0);
            }
        }
    }

    // Called when the event is invoked either by killing a creature OR being killed by a creature
    // TODO: Invoke this whenever the situations occur in the game manager?
    public void KilledEventOccurred(EnemyStatObject enemy, StoryBeatType beatType)
    {
        // If this event's beatType is NOT creatureKilled OR killedBy, error
        if( !(beatType == StoryBeatType.creatureKilled || beatType == StoryBeatType.killedBy) ){
            Debug.LogError("KilledEventOccurred for wrong StoryBeatType: " + beatType + " " + enemy + "!");
            return;
        }   

        foreach( StoryBeat beat in storyBeatDatabase.Keys ){
            if( beat.GetBeatType() == beatType ){
                // Definitely one of the two killed types at this point, so cast it and check if the enemy is correct
                StoryBeatCreatureKilled trigger = (StoryBeatCreatureKilled)beat;
                if( trigger.GetEnemy() == enemy ){
                    AchievedStoryBeat(beat);
                    return;
                }
            }
        }
        
        Debug.Log("No story beat found for " + beatType + " " + enemy + "!");
    }

    // TODO: Invoke this whenever the situations occur in the dialogue manager?
    public void ConversationEventOccurred(SpeakerID npc, string otherDialogueHeadNode)
    {
        foreach( StoryBeat beat in storyBeatDatabase.Keys ){
            // Check only the ConversationTrigger type StoryBeats
            if( beat.GetBeatType() == StoryBeatType.dialogueCompleted ){
                StoryBeatConversation trigger = (StoryBeatConversation)beat;
                if( trigger.GetTalkedToNPC() == npc && trigger.GetOtherDialogue() == otherDialogueHeadNode ){
                    AchievedStoryBeat(beat);
                    return;
                }
            }
        }
        Debug.Log("No story beat found for " + otherDialogueHeadNode + " " + npc + "!");
    }
}
