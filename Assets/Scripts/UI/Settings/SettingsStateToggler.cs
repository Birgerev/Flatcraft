using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsStateToggler : SettingsUIElement
{
    [Space]
    public string[] stateValue;
    public string[] stateString;

    private int _currentState;
    private Text _text;
    
    private void Awake()
    {
        _text = GetComponentInChildren<Text>();
        GetComponent<Button>().onClick.AddListener(NextToggle);
    }

    private void Update()
    {
        _text.text = stateString[_currentState];
    }

    public override void LoadValue()
    {
        _currentState = Array.IndexOf(stateValue, SettingsManager.GetStringValue(settingsKey));
    }
    
    private void NextToggle()
    {
        _currentState++;
        _currentState %= stateValue.Length;
        
        SetValue(stateValue[_currentState]);
    }
}
