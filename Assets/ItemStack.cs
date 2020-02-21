using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public Material material;
    public int amount;
    public string data;
    public int durablity;

    public ItemStack()
    {
        this.material = Material.Air;
        this.amount = 0;
    }

    public ItemStack(Material material)
    {
        this.material = material;
        this.amount = 1;
        this.durablity = getMaxDurability();
    }
    
    public ItemStack(Material material, int amount)
    {
        this.material = material;
        this.amount = amount;
        this.durablity = getMaxDurability();
    }
    
    public ItemStack(Material material, int amount, string data)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
        this.durablity = getMaxDurability();
    }

    public ItemStack(Material material, int amount, string data, int durablity)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
        this.durablity = durablity;
    }

    public Sprite getSprite()
    {
        System.Type type = System.Type.GetType(material.ToString());
        if (type == null)
            return null;

        string texture = (string)type.GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        return (Sprite)Resources.Load<Sprite>("Sprites/" + texture);
    }

    public int getMaxDurability()
    {
        if (material == Material.Air)
            return -1;

        System.Type type = System.Type.GetType(material.ToString());

        if (type == null || !type.IsSubclassOf(typeof(Item)))
            return -1;

        object item = System.Activator.CreateInstance(type);
        
        return ((Item)item).maxDurabulity;
    }

    public void Drop(Location location)
    {
        Drop(location, false);
    }

    public void Drop(Location location, bool randomVelocity)
    {
        Vector2 velocity = Vector2.zero;
        if (randomVelocity)
        {
            System.Random random = new System.Random((this).GetHashCode() + location.GetHashCode());
            Vector2 maxVelocity = new Vector2(1, 2);
            velocity = new Vector2((float)random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
            (float)random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x);
        }

        Drop(location, velocity);
    }

    public void Drop(Location location, Vector2 velocity)
    {
        if (material == Material.Air || amount <= 0)
            return;

        GameObject obj = Entity.Spawn("DroppedItem").gameObject;

        obj.transform.position = new Vector3(location.x, location.y, 0);
        obj.GetComponent<DroppedItem>().item = this;
        obj.GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public ItemStack Clone()
    {
        return (ItemStack)this.MemberwiseClone();
    }
}
