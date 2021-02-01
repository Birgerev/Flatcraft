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
        if (Player.localInstance == null)
            return;

        base.UpdateSlot();

        item = Player.localInstance.inventory.getHotbar()[transform.GetSiblingIndex()];

        GetComponent<Image>().sprite = Player.localInstance.inventory.selectedSlot == transform.GetSiblingIndex()
            ? selected
            : normal;
    }
}