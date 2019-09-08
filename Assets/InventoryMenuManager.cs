using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenuManager : MonoBehaviour
{
    public PlayerInventoryMenu playerInventoryMenu;
    public ContainerInventoryMenu containerInventoryMenu;
    public CraftingInventoryMenu craftingInventoryMenu;

    public static InventoryMenuManager instance;

    private void Start()
    {
        instance = this;
    }
}
