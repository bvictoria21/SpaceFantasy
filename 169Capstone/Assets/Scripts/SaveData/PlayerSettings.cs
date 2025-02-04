﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerPrefKeys
{
    masterVolume,
    musicVolume,
    sfxVolume,

    textSpeed,

    aimAtCursor,

    enumSize
}

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings instance;

    public const float DEFAULT_VOLUME = 1;
    public float masterVolumeValue {get; private set;}
    public float musicVolumeValue {get; private set;}
    public float sfxVolumeValue {get; private set;}

    public TextSpeedSetting currentTextSpeed {get; private set;}

    public bool aimAtCursor {get; private set;}

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
        SetupSettings();
    }

    #region Volume Settings
        public void SaveNewMasterVolume(float newVolume)
        {
            masterVolumeValue = newVolume;
            SetMasterVolumeToCurrentSetting();

            PlayerPrefs.SetFloat(PlayerPrefKeys.masterVolume.ToString(), masterVolumeValue);
            PlayerPrefs.Save();
        }

        public void SetMasterVolumeToCurrentSetting()
        {
            AudioManager.Instance.SetMasterVolume(masterVolumeValue);
        }

        public void SaveNewMusicVolume(float newVolume)
        {
            musicVolumeValue = newVolume;
            SetMusicVolumeToCurrentSetting();

            PlayerPrefs.SetFloat(PlayerPrefKeys.musicVolume.ToString(), musicVolumeValue);
            PlayerPrefs.Save();
        }

        public void SetMusicVolumeToCurrentSetting()
        {
            AudioManager.Instance.SetMusicVolume(musicVolumeValue);
        }

        public void SaveNewSFXVolume(float newVolume)
        {
            sfxVolumeValue = newVolume;
            SetSFXVolumeToCurrentSetting();

            PlayerPrefs.SetFloat(PlayerPrefKeys.sfxVolume.ToString(), sfxVolumeValue);
            PlayerPrefs.Save();
        }

        public void SetSFXVolumeToCurrentSetting()
        {
            AudioManager.Instance.SetSFXVolume(sfxVolumeValue);
        }
    #endregion

    public void SaveNewTextSpeed(TextSpeedSetting textSpeed)
    {
        currentTextSpeed = textSpeed;
        SetTextSpeedToCurrentSetting();

        PlayerPrefs.SetInt(PlayerPrefKeys.textSpeed.ToString(), (int)currentTextSpeed);
        PlayerPrefs.Save();
    }

    public void SetTextSpeedToCurrentSetting()
    {
        switch(currentTextSpeed){
            case TextSpeedSetting.defaultSpeed:
                DialogueManager.instance.SetTextSpeed(DialogueManager.DEFAULT_TEXT_SPEED);
                return;
            case TextSpeedSetting.fast:
                DialogueManager.instance.SetTextSpeed(DialogueManager.FAST_TEXT_SPEED);
                return;
            case TextSpeedSetting.instant:
                DialogueManager.instance.SetTextSpeed(0);
                return;
        }
        Debug.LogError("No text speed setting found for text speed: " + currentTextSpeed);
    }

    public void SaveAimAtCursor(bool value)
    {
        aimAtCursor = value;
        SetAimAtCursorToCurrentSetting();

        if(aimAtCursor){
            PlayerPrefs.SetInt(PlayerPrefKeys.aimAtCursor.ToString(), 1);
        }
        else{
            PlayerPrefs.SetInt(PlayerPrefKeys.aimAtCursor.ToString(), 0);
        }
        
        PlayerPrefs.Save();
    }

    public void SetAimAtCursorToCurrentSetting()
    {
        InputManager.aimAtCursor = aimAtCursor;
    }

    /*
        For each setting value, check if it exists and if so set to the saved value; otherwise, add it with the default value
        If there are saved values, set the ACTUAL values here (not just the saved variables here)
    */
    private void SetupSettings()
    {
        if(!PlayerPrefs.HasKey(PlayerPrefKeys.masterVolume.ToString())){
            // Set to the default
            SaveNewMasterVolume(DEFAULT_VOLUME);
        }
        else{
            // If there is already a setting saved, retrieve it and set the current volume to that
            masterVolumeValue = PlayerPrefs.GetFloat(PlayerPrefKeys.masterVolume.ToString());
            SetMasterVolumeToCurrentSetting();
        }

        if(!PlayerPrefs.HasKey(PlayerPrefKeys.musicVolume.ToString())){
            SaveNewMusicVolume(DEFAULT_VOLUME);
        }
        else{
            musicVolumeValue = PlayerPrefs.GetFloat(PlayerPrefKeys.musicVolume.ToString());
            SetMusicVolumeToCurrentSetting();
        }

        if(!PlayerPrefs.HasKey(PlayerPrefKeys.sfxVolume.ToString())){
            SaveNewSFXVolume(DEFAULT_VOLUME);
        }
        else{
            sfxVolumeValue = PlayerPrefs.GetFloat(PlayerPrefKeys.sfxVolume.ToString());
            SetSFXVolumeToCurrentSetting();
        }

        // If there is no text speed key or if the saved one is invalid, set text speed to default
        if(!PlayerPrefs.HasKey(PlayerPrefKeys.textSpeed.ToString()) || PlayerPrefs.GetInt(PlayerPrefKeys.textSpeed.ToString()) >= (int)TextSpeedSetting.enumSize || PlayerPrefs.GetInt(PlayerPrefKeys.textSpeed.ToString()) < 0){
            SaveNewTextSpeed(TextSpeedSetting.defaultSpeed);
        }
        else{
            currentTextSpeed = (TextSpeedSetting)PlayerPrefs.GetInt(PlayerPrefKeys.textSpeed.ToString());
            SetTextSpeedToCurrentSetting();
        }

        if(!PlayerPrefs.HasKey(PlayerPrefKeys.aimAtCursor.ToString())){
            SaveAimAtCursor(true);
        }
        else{
            int value = PlayerPrefs.GetInt(PlayerPrefKeys.aimAtCursor.ToString());
            if(value == 1){
                aimAtCursor = true;
            }
            else{
                aimAtCursor = false;
            }
            SetAimAtCursorToCurrentSetting();
        }

        FindObjectOfType<SettingsMenu>()?.SetSettingsUIToSavedValues();
    }
}