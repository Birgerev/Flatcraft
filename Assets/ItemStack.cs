using System;
using System.Collections.Generic;
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
    public string GetTexture()
    {
        System.Type materialType = Type.GetType(material.ToString());
        string texture = "invalid item class (not inheriting from Block nor Item)";

        if (materialType.IsSubclassOf(typeof(Block)))
            texture = ((Block)Activator.CreateInstance(materialType)).texture;
        else if (materialType.IsSubclassOf(typeof(Item)))
            texture = ((Item)Activator.CreateInstance(materialType)).texture;

        return texture;
    }

    public Sprite GetSprite()
    {
        return Resources.Load<Sprite>("Sprites/" + GetTexture());
    }
    public Color[] GetTextureColors()
    {
        List<Color> textureColors = new List<Color>();
        foreach (Color color in GetSprite().texture.GetPixels())
        {
            if (color.a != 0)
                textureColors.Add(color);
        }

        return textureColors.ToArray();
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

    public float GetItemEntityDamage()
    {
        if (material == Material.Air)
            return 1;

        var type = Type.GetType(material.ToString());

        if (type == null || !type.IsSubclassOf(typeof(Item)))
            return 1;

        var item = Activator.CreateInstance(type);

        return ((Item)item).entityDamage;
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