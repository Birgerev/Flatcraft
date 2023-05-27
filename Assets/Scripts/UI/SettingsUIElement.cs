using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUIElement : MonoBehaviour
{
    public string settingsKey;

    private void Start()
    {
        LoadValue();
    }

    private void SetValue(string value)
    {
        SettingsManager.Values[value] = value;
    }

    protected virtual void LoadValue() { }
}
