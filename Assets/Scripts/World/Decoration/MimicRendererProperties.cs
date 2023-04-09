using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MimicRendererProperties : MonoBehaviour
{
    [FormerlySerializedAs("parentRenderer")] 
    public SpriteRenderer rendererToMimic;

    [Space] 
    public bool brightnessValue = true;
    public bool color = true;
    
    private SpriteRenderer _thisRenderer;

    private void Start()
    {
        _thisRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if(color)
            _thisRenderer.color = rendererToMimic.color;

        if (brightnessValue)
        {
            _thisRenderer.material.SetColor("_LightColor", rendererToMimic.material.GetColor("_LightColor"));
            _thisRenderer.material.SetFloat("_LightFlicker", rendererToMimic.material.GetFloat("_LightFlicker"));
        }
    }
}
