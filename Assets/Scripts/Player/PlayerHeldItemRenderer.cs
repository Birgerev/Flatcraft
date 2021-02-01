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
        Material itemInHand = player.inventory.getSelectedItem().material;
        if (itemInHand == Material.Air)
        {
            SetColor(Color.clear);
            return;
        }

        if (itemInHand == lastItemInHand) 
            return;

        SetColor(new ItemStack(itemInHand).GetTextureColors()[0]);

        lastItemInHand = itemInHand;
    }

    private void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}