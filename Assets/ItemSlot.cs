using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public Text amountText;
    public Image texture;

    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        ItemStack i = getItemStack();

        if (i.amount == 0)
            amountText.text = "";
        else
            amountText.text = i.amount.ToString();

        texture.sprite = i.getSprite();
    }
    
    public virtual ItemStack getItemStack()
    {
        return new ItemStack(Material.Stone, 2);
    }
}
