using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerInventoryMenu : InventoryMenu
{
    public Inventory inventory;

    public virtual int totalSlotAmount { get; set; } = 45;
}
