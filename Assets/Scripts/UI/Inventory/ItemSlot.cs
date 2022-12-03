using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Text amountText;
    public GameObject durabilityBar;
    public Image durabilityBarFiller;
    public Image texture;

    public ItemStack item;

    // Update is called once per frame
    public virtual void UpdateSlotContents()
    {
        if (item.Amount == 0)
        {
            amountText.text = "";
            item.material = Material.Air;
        }
        else if (item.Amount == 1)
        {
            amountText.text = "";
        }
        else
        {
            amountText.text = item.Amount.ToString();
        }

        texture.sprite = item.GetSprite();

        if (item.GetMaxDurability() == -1)
        {
            durabilityBar.SetActive(false);
            durabilityBarFiller.gameObject.SetActive(false);
        }
        else
        {
            durabilityBar.SetActive(true);
            durabilityBarFiller.gameObject.SetActive(true);
            durabilityBarFiller.fillAmount = item.durability / (float) item.GetMaxDurability();
        }
    }

    public virtual void Click()
    {
        InventoryMenu menu = GetComponentInParent<InventoryMenu>();
        int inventoryIndex = menu.GetSlotInventoryIndex(this);
        int slotId = menu.GetSlotIndex(this);

        //If we detect click event, check if click was left, otherwise it was right click
        menu.OnClickSlot(inventoryIndex, slotId, Input.GetMouseButtonDown(0) ? ClickType.LeftClick : ClickType.RightClick);
    }

    public virtual void Hover(bool hover)
    {
        if (hover)
            Tooltip.hoveredItem = item;
        if (!hover && Tooltip.hoveredItem.Equals(item))
            Tooltip.hoveredItem = new ItemStack();
    }
}