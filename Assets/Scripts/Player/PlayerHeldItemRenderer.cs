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
        Material itemInHand = player.GetInventoryHandler().GetInventory().GetSelectedItem().material;

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