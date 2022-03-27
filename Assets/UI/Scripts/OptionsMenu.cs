using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Text fullscreenText;

    private void Update()
    {
        fullscreenText.text = "Fullscreen: " + (ScreenManager.IsFullscreen() ? "On" : "Off");
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
