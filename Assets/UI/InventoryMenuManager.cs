using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenuManager : MonoBehaviour
{
    public PlayerInventoryMenu playerInventoryMenu;
    public ContainerInventoryMenu containerInventoryMenu;
    public CraftingInventoryMenu craftingInventoryMenu;
    public FurnaceInventoryMenu furnaceInventoryMenu;

    public static InventoryMenuManager instance;

    private void Start()
    {
        instance = this;
    }
    
    public bool anyInventoryOpen()
    {
        bool open = false;

        foreach(InventoryMenu menu in transform.GetComponentsInChildren<InventoryMenu>())
        {
            if (menu.active)
                open = true;
        }
        return open;
    }
}
