using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Text fullscreenText;
    public Slider musicSlider;

    private void Start()
    {
        //Assign current values to UI elements
        musicSlider.value = SettingsManager.GetIntValue("soundCategory_music");
    }

    private void Update()
    {
        //TODO fullscreen settings manager
        fullscreenText.text = "Fullscreen: " + (ScreenManager.IsFullscreen() ? "On" : "Off");
        
        SettingsManager.Values["soundCategory_music"] = musicSlider.value.ToString();
    }

    public void ToggleFullscreen()
    {
        ScreenManager.ToggleFullscreen();
    }
    
    public void Close()
    {
        Destroy(gameObject);
    }
}
