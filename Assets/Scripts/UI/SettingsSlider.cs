using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SettingsSlider : SettingsUIElement
{
    private Slider _slider;

    private void Start()
    {
        _slider = GetComponent<Slider>();
    }

    protected override void LoadValue()
    {
        _slider.value = SettingsManager.GetIntValue(settingsKey);
    }
}
