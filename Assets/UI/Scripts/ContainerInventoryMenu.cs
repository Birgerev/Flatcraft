using UnityEngine.UI;

public class ContainerInventoryMenu : InventoryMenu
{
    public Text inventoryTitle;

    public override void SetTitle()
    {
        base.SetTitle();
        inventoryTitle.text = inventories[1].name;
    }
}