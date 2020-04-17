using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public Text amountText;
    public Image texture;
    public GameObject durabilityBar;
    public Image durabilityBarFiller;

    public ItemStack item;

    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    private void Update()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
            return;
        }

        if (Time.frameCount % 5 == 0 && canvasGroup.interactable)
        {
            UpdateSlot();
        }
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

        texture.sprite = item.getSprite();
    
        if (item.getMaxDurability() == -1)
        {
            durabilityBar.SetActive(false);
            durabilityBarFiller.gameObject.SetActive(false);
        }
        else
        {
            durabilityBar.SetActive(true);
            durabilityBarFiller.gameObject.SetActive(true);
            durabilityBarFiller.fillAmount = (float)item.durablity / (float)item.getMaxDurability();
        }
    }

    public virtual void Click()
    {
        GetComponentInParent<InventoryMenu>().OnClickSlot(transform.GetSiblingIndex(), (Input.GetMouseButtonUp(0)) ? 0 : 1);
        UpdateSlot();
    }
}
