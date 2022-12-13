using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MimicRendererColor : MonoBehaviour
{
    [FormerlySerializedAs("parentRenderer")] 
    public SpriteRenderer rendererToMimic;
    private SpriteRenderer _thisRenderer;

    private void Start()
    {
        _thisRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        _thisRenderer.color = rendererToMimic.color;
    }
}
