using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SettingsSlider : SettingsUIElement
{
    [Space]
    public string textPrefix;
    public string textSuffix;
    
    private Slider _slider;
    private Text _text;
//TODO boolean
    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _text = GetComponentInChildren<Text>();
    }

    protected override void Start()
    {
        base.Start();
        
        _slider.onValueChanged.AddListener(ValueChangedEvent);
    }

    private void Update()
    {
        _text.text = textPrefix + _slider.value + textSuffix;
    }

    public override void LoadValue()
    {
        _slider.value = SettingsManager.GetIntValue(settingsKey);
    }
    
    private void ValueChangedEvent(float value)
    {
        SetValue(((int)_slider.value).ToString());
    }
}
