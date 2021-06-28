using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRendererPart : MonoBehaviour
{
    public bool color = true;
    public bool flip = true;

    public SpriteRenderer parentRenderer;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (color)
            spriteRenderer.color = parentRenderer.color;

        if (flip)
        {
            spriteRenderer.flipX = parentRenderer.flipX;
            spriteRenderer.flipY = parentRenderer.flipY;
        }
    }
}