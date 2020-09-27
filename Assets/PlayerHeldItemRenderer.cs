using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeldItemRenderer : MonoBehaviour
{
    public Player player;
    private Material lastItemInHand;

    private void Update()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        Material itemInHand = player.inventory.getSelectedItem().material;
        if (itemInHand == Material.Air)
        {
            SetColor(Color.clear);
            return;
        }
        if (itemInHand == lastItemInHand)
        {
            return;
        }


        System.Type type = System.Type.GetType(itemInHand.ToString());
        string textureName = (string)type.GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
        Sprite texture = (Sprite)Resources.Load<Sprite>("Sprites/" + textureName);

        Color firstNotAlphaColor = Color.white;
        foreach (Color pixel in texture.texture.GetPixels())
        {
            if (pixel.a > 0.1f)
            {
                firstNotAlphaColor = pixel;
                break;
            }
        }

        SetColor(firstNotAlphaColor);
        lastItemInHand = itemInHand;
    }

    private void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}
