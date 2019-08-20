using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public Material material;
    public int amount;
    public string data;

    public ItemStack()
    {
        this.material = Material.Air;
        this.amount = 0;
    }

    public ItemStack(Material material)
    {
        this.material = material;
        this.amount = 1;
    }
    
    public ItemStack(Material material, int amount)
    {
        this.material = material;
        this.amount = amount;
    }
    
    public ItemStack(Material material, int amount, string data)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
    }

    public Sprite getSprite()
    {
        System.Type type = System.Type.GetType(material.ToString());
        if (type == null)
            return null;

        string texture = (string)type.GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        return (Sprite)Resources.Load<Sprite>("Sprites/" + texture);
    }

    public void Drop(Vector2Int position)
    {
        GameObject obj = MonoBehaviour.Instantiate((GameObject)Resources.Load("Objects/DroppedItem"));

        obj.transform.position = new Vector3(position.x, position.y, 0);
        obj.GetComponent<DroppedItem>().item = this;
    }

    public ItemStack Clone()
    {
        return (ItemStack)this.MemberwiseClone();
    }
}
