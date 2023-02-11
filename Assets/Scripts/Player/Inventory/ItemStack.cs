using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[Serializable]
public struct ItemStack
{
    public int amount;
    public int Amount{
        get
        {
            return amount;
        }
        set
        {
            if(value == 0)
                this = default(ItemStack);
            if (value > Inventory.MaxStackSize)
                amount = Inventory.MaxStackSize;
            
            amount = value;
        }
    }
    public string data;
    public int durability;
    public Material material;

    public ItemStack(Material material)
    {
        this.material = material;
        this.amount = -1;
        this.durability = -1;
        this.data = "";

        this.Amount = 1;
        this.durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount)
    {
        this.material = material;
        this.amount = -1;
        this.durability = -1;
        this.data = "";

        this.Amount = amount;
        this.durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount, string data)
    {
        this.material = material;
        this.amount = -1;
        this.data = data;
        this.durability = -1;
        
        this.Amount = amount;
        this.durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount, string data, int durability)
    {
        this.material = material;
        this.amount = -1;
        this.data = data;
        this.durability = durability;
        
        this.Amount = amount;
    }

    public ItemStack(string saveString)
    {
        string[] values = saveString.Split('*');

        this.material = (Material) Enum.Parse(typeof(Material), values[0]);
        this.amount = -1;
        this.data = values[2];
        this.durability = int.Parse(values[3]);
        
        this.Amount = int.Parse(values[1]);
    }

    public string GetTexturePath()
    {
        string texturePath = "Sprites/item/item_" + material;
        
        Type type = Type.GetType(material.ToString());
        if (type == null)
        {
            Debug.LogWarning("Tried getting texture for non existent Item: " + material);
            return "";
        }

        bool isBlock = type.IsSubclassOf(typeof(Block));
        if (isBlock && Resources.Load<Sprite>(texturePath) == null)
            texturePath = "Sprites/block/block_" + material;
            
        return texturePath;
    }

    public Sprite GetSprite()
    {
        return Resources.Load<Sprite>(GetTexturePath());
    }

    public Color[] GetTextureColors()
    {
        List<Color> textureColors = new List<Color>();
        foreach (Color color in GetSprite().texture.GetPixels())
            if (color.a != 0)
                textureColors.Add(color);

        return textureColors.ToArray();
    }

    public int GetMaxDurability()
    {
        if (material == Material.Air)
            return -1;

        Type type = Type.GetType(material.ToString());

        if (type == null || !type.IsSubclassOf(typeof(Item)))
            return -1;

        object item = Activator.CreateInstance(type);

        return ((Item) item).maxDurability;
    }

    public float GetItemEntityDamage()
    {
        if (material == Material.Air)
            return 1;

        Type type = Type.GetType(material.ToString());

        if (type == null || !type.IsSubclassOf(typeof(Item)))
            return 1;

        object item = Activator.CreateInstance(type);

        return ((Item) item).entityDamage;
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
            Random random = new Random(GetHashCode() + location.GetHashCode());
            Vector2 maxVelocity = new Vector2(1, 2);
            velocity = new Vector2((float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
                (float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x);
        }

        Drop(location, velocity);
    }

    public void Drop(Location location, Vector2 velocity)
    {
        if (material == Material.Air || Amount <= 0)
            return;

        GameObject obj = Entity.Spawn("DroppedItem").gameObject;

        obj.GetComponent<DroppedItem>().Location = location;
        obj.GetComponent<DroppedItem>().item = this;
        obj.GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public override string ToString()
    {
        return material + "*" + Amount + "*" + data + "*" + durability;
    }
}