using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HotbarItemSlot : ItemSlot
{
    public Sprite normal;
    public Sprite selected;
    
    // Update is called once per frame
    public override void UpdateSlot()
    {
        if (Player.localInstance == null)
            return;
        
        base.UpdateSlot();

        item = Player.localInstance.inventory.getHotbar()[transform.GetSiblingIndex()];

        GetComponent<Image>().sprite = (Player.localInstance.inventory.selectedSlot == transform.GetSiblingIndex()) ? selected : normal;
    }
}
