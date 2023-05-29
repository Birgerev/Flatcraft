using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUIElement : MonoBehaviour
{
    public string settingsKey;

    protected virtual void Start()
    {
        LoadValue();
    }

    protected void SetValue(string value)
    {
        SettingsManager.Values[settingsKey] = value;
    }

    public virtual void LoadValue() { }
}
