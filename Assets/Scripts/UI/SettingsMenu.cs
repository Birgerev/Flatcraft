using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Text fullscreenText;
    [Space]
    public Slider masterSlider;
    public Slider entitiesSlider;
    public Slider blockSlider;
    public Slider musicSlider;

    private void Start()
    {
        LoadUIValues();
    }

    private void Update()
    {
        //Close menu on ESC
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();

        ApplyUIValues();
    }

    private void LoadUIValues()
    {
        //Assign current values to UI elements
        masterSlider.value = SettingsManager.GetIntValue("soundCategory_master");
        entitiesSlider.value = SettingsManager.GetIntValue("soundCategory_entities");
        blockSlider.value = SettingsManager.GetIntValue("soundCategory_block");
        musicSlider.value = SettingsManager.GetIntValue("soundCategory_music");
    }

    private void ApplyUIValues()
    {
        //TODO fullscreen settings manager
        fullscreenText.text = "Fullscreen: " + (ScreenManager.IsFullscreen() ? "On" : "Off");
        
        SettingsManager.Values["soundCategory_master"] = masterSlider.value.ToString();
        SettingsManager.Values["soundCategory_entities"] = entitiesSlider.value.ToString();
        SettingsManager.Values["soundCategory_block"] = blockSlider.value.ToString();
        SettingsManager.Values["soundCategory_music"] = musicSlider.value.ToString();
    }

    public void ToggleFullscreen()
    {
        ScreenManager.ToggleFullscreen();
    }

    public void ResetDefaultSettings()
    {
        SettingsManager.OverwriteWithDefaultSettings();
        LoadUIValues();
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
