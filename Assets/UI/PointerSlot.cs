using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PointerSlot : ItemSlot
{
    private void Start()
    {
        item = new ItemStack();
    }

    // Update is called once per frame
    public override void UpdateSlot()
    {
        base.UpdateSlot();

        transform.position = Input.mousePosition;
    }
}
