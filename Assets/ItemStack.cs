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
        Drop(position, false);
    }

    public void Drop(Vector2Int position, bool randomVelocity)
    {
        Vector2 velocity = Vector2.zero;
        if (randomVelocity)
        {
            System.Random random = new System.Random((this).GetHashCode() + position.GetHashCode());
            Vector2 maxVelocity = new Vector2(1, 2);
            velocity = new Vector2((float)random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
            (float)random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x);
        }

        Drop(position, velocity);
    }

    public void Drop(Vector2Int position, Vector2 velocity)
    {
        if (material == Material.Air || amount <= 0)
            return;

        GameObject obj = Entity.Spawn("DroppedItem").gameObject;

        obj.transform.position = new Vector3(position.x, position.y, 0);
        obj.GetComponent<DroppedItem>().item = this;
        obj.GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public ItemStack Clone()
    {
        return (ItemStack)this.MemberwiseClone();
    }
}
