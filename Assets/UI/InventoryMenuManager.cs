using UnityEngine;

public class InventoryMenuManager : MonoBehaviour
{
    public static InventoryMenuManager instance;
    public ContainerInventoryMenu containerInventoryMenu;
    public CraftingInventoryMenu craftingInventoryMenu;
    public FurnaceInventoryMenu furnaceInventoryMenu;
    public PlayerInventoryMenu playerInventoryMenu;

    private void Start()
    {
        instance = this;
    }

    public bool anyInventoryOpen()
    {
        var open = false;

        foreach (var menu in transform.GetComponentsInChildren<InventoryMenu>())
            if (menu.active)
                open = true;
        return open;
    }
}