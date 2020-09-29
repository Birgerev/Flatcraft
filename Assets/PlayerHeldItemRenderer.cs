using System;
using System.Reflection;
using UnityEngine;

public class PlayerHeldItemRenderer : MonoBehaviour
{
    private Material lastItemInHand;
    public Player player;

    private void Update()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        var itemInHand = player.inventory.getSelectedItem().material;
        if (itemInHand == Material.Air)
        {
            SetColor(Color.clear);
            return;
        }

        if (itemInHand == lastItemInHand) return;


        var type = Type.GetType(itemInHand.ToString());
        var textureName = (string) type.GetField("default_texture", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);
        var texture = Resources.Load<Sprite>("Sprites/" + textureName);

        var firstNotAlphaColor = Color.white;
        foreach (var pixel in texture.texture.GetPixels())
            if (pixel.a > 0.1f)
            {
                firstNotAlphaColor = pixel;
                break;
            }

        SetColor(firstNotAlphaColor);
        lastItemInHand = itemInHand;
    }

    private void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}