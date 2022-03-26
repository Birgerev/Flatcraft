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
        fullscreenText.text = "Fullscreen: " + (Screen.fullScreen ? "On" : "Off");
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
    
    public void Close()
    {
        Destroy(gameObject);
    }
}
