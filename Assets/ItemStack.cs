using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

[Serializable]
public class ItemStack
{
    public int amount;
    public string data;
    public int durability;
    public Material material;

    public ItemStack()
    {
        material = Material.Air;
        amount = 0;
    }

    public ItemStack(Material material)
    {
        this.material = material;
        amount = 1;
        durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount)
    {
        this.material = material;
        this.amount = amount;
        durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount, string data)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
        durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount, string data, int durability)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
        this.durability = durability;
    }

    public Sprite GetSprite()
    {
        var type = Type.GetType(material.ToString());
        if (type == null)
            return null;

        var texture = (string) type.GetField("default_texture", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);

        return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public int GetMaxDurability()
    {
        if (material == Material.Air)
            return -1;

        var type = Type.GetType(material.ToString());

        if (type == null || !type.IsSubclassOf(typeof(Item)))
            return -1;

        var item = Activator.CreateInstance(type);

        return ((Item) item).maxDurabulity;
    }

    public void Drop(Location location)
    {
        Drop(location, false);
    }

    public void Drop(Location location, bool randomVelocity)
    {
        var velocity = Vector2.zero;
        if (randomVelocity)
        {
            var random = new Random(GetHashCode() + location.GetHashCode());
            var maxVelocity = new Vector2(1, 2);
            velocity = new Vector2((float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
                (float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x);
        }

        Drop(location, velocity);
    }

    public void Drop(Location location, Vector2 velocity)
    {
        if (material == Material.Air || amount <= 0)
            return;

        var obj = Entity.Spawn("DroppedItem").gameObject;

        obj.transform.position = new Vector3(location.x, location.y, 0);
        obj.GetComponent<DroppedItem>().item = this;
        obj.GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public ItemStack Clone()
    {
        return (ItemStack) MemberwiseClone();
    }
}