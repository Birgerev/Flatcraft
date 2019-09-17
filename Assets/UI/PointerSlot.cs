using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PointerSlot : ItemSlot
{
    public override void Start()
    {
        base.Start();

        item = new ItemStack();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        transform.position = Input.mousePosition;
    }
}
