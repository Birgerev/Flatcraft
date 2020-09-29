using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Text amountText;

    private CanvasGroup canvasGroup;
    public GameObject durabilityBar;
    public Image durabilityBarFiller;

    public ItemStack item;
    public Image texture;

    // Start is called before the first frame update
    private void Update()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
            return;
        }

        if (Time.frameCount % 30 == 0 && canvasGroup.interactable) UpdateSlot();
    }


    // Update is called once per frame
    public virtual void UpdateSlot()
    {
        if (item == null)
            item = new ItemStack();

        if (item.amount == 0)
        {
            amountText.text = "";
            item.material = Material.Air;
        }
        else if (item.amount == 1)
        {
            amountText.text = "";
        }
        else
        {
            amountText.text = item.amount.ToString();
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
        var menu = GetComponentInParent<InventoryMenu>();
        var slot = menu.getSlotObjects().ToList().IndexOf(this);

        GetComponentInParent<InventoryMenu>().OnClickSlot(slot, Input.GetMouseButtonUp(0) ? 0 : 1);
        UpdateSlot();
    }
}