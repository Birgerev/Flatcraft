using UnityEngine;
using UnityEngine.UI;

public class HotbarItemSlot : ItemSlot
{
    public Sprite normal;
    public Sprite selected;

    private void Update()
    {
        if (Time.frameCount % 2 == 0)
            UpdateSlotContents();
    }

    // Update is called once per frame
    public override void UpdateSlotContents()
    {
        if (Player.LocalEntity == null || Player.LocalEntity.inventoryId == 0)
            return;

        base.UpdateSlotContents();

        item = Player.LocalEntity.GetInventoryHandler().GetInventory().GetHotbarItems()[transform.GetSiblingIndex()];

        GetComponent<Image>().sprite = Player.LocalEntity.GetInventoryHandler().GetInventory().selectedSlot == transform.GetSiblingIndex()
            ? selected
            : normal;
    }
}