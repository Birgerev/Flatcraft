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
        Material itemInHand = player.GetInventory().GetSelectedItem().material;

        if (itemInHand == lastItemInHand) 
            return;

        lastItemInHand = itemInHand;
        
        if (itemInHand == Material.Air)
        {
            SetColor(Color.clear);
            return;
        }

        SetColor(new ItemStack(itemInHand).GetTextureColors()[0]);
    }

    private void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}