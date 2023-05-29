using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SettingsSlider : SettingsUIElement
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    protected override void Start()
    {
        base.Start();
        
        _slider.onValueChanged.AddListener(ValueChangedEvent);
    }

    protected override void LoadValue()
    {
        _slider.value = SettingsManager.GetIntValue(settingsKey);
    }
    
    private void ValueChangedEvent(float value)
    {
        SetValue(((int)_slider.value).ToString());
    }
}
