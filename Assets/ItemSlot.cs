using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public Text amountText;
    public Image texture;

    public ItemStack item;

    // Start is called before the first frame update
    public virtual void Start()
    {
    }

    // Update is called once per frame
    public virtual void Update()
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
    }

    public virtual void Click()
    {
        GetComponentInParent<InventoryMenu>().OnClickSlot(transform.GetSiblingIndex(), (Input.GetMouseButtonUp(0)) ? 0 : 1);
    }
}
