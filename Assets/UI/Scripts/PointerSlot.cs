﻿using UnityEngine;

public class PointerSlot : ItemSlot
{
    private void Start()
    {
        item = new ItemStack();
    }

    private void Update()
    {
        //always update pointer (overriding previous if statements)
        UpdateSlot();
        Tooltip.isPointerHoldingItem = item.material != Material.Air;
        ;
    }

    // Update is called once per frame
    public override void UpdateSlot()
    {
        base.UpdateSlot();

        transform.position = Input.mousePosition;
    }
}