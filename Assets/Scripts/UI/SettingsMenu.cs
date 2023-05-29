using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private void Update()
    {
        //Close menu on ESC
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
    
    public void ResetDefaultSettings()
    {
        SettingsManager.OverwriteWithDefaultSettings();
        
        foreach (var uiEntry in GetComponentsInChildren<SettingsUIElement>())
            uiEntry.LoadValue();
    }

    public void ToggleFullscreen()
    {
        ScreenManager.ToggleFullscreen();//TODO entry
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
