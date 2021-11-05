using UnityEngine;
using UnityEngine.UI;

public class HotbarItemSlot : ItemSlot
{
    public Sprite normal;
    public Sprite selected;

    private void Update()
    {
        if (Time.frameCount % 2 == 0)
            UpdateSlot();
    }

    // Update is called once per frame
    public override void UpdateSlot()
    {
        if (Player.localEntity == null || Player.localEntity.inventoryId == 0)
            return;

        base.UpdateSlot();

        item = Player.localEntity.GetInventory().GetHotbarItems()[transform.GetSiblingIndex()];

        GetComponent<Image>().sprite = Player.localEntity.GetInventory().selectedSlot == transform.GetSiblingIndex()
            ? selected
            : normal;
    }
}