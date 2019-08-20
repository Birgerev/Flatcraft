using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HotbarItemSlot : ItemSlot
{
    public Sprite normal;
    public Sprite selected;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        item = Player.localInstance.inventory.getHotbar()[transform.GetSiblingIndex()];

        GetComponent<Image>().sprite = 
(Player.localInstance.inventory.selectedSlot == transform.GetSiblingIndex()) ? selected : normal;
    }
}
