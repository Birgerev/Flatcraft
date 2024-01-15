using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[Serializable]
public struct ItemStack
{
    public int _amount;//NOTE, should never be accessed, but can't make private since it breaks network serializing
    public int Amount{
        get => _amount;
        set
        {
            _amount = value;
            
            //If amount = 0, make this item empty
            if(value == 0)
                this = default;
            
            //Don't allow more than max stack size
            if (value > Inventory.MaxStackSize)
                _amount = Inventory.MaxStackSize;
        }
    }
    
    public string data;
    public int durability;
    public Material material;

    public ItemStack(Material material, int amount = 1, string data = "", int durability = -1)
    {
        this.material = material;
        this.data = data;
        this.durability = durability;
        
        //_amount is properly assigned through the Amount wrapper, which implements max stack size etc
        _amount = amount;
        Amount = amount;
        
        //If durability is default value, use max durability
        if (durability == -1)
            this.durability = GetMaxDurability();
    }

    public ItemStack(string saveString)
    {
        string[] values = saveString.Split('*');

        material = (Material) Enum.Parse(typeof(Material), values[0]);
        data = values[2];
        durability = int.Parse(values[3]);
        
        //_amount is properly assigned through the Amount wrapper, which implements max stack size etc
        int amount = int.Parse(values[1]);
        _amount = 1;
        Amount = amount;
    }

    public string GetTexturePath()
    {
        Type type = Type.GetType(material.ToString());
        if (type == null)
        {
            Debug.LogWarning("Tried getting texture for non existent Item: " + material);
            return "";
        }

        //Special case for blocks
        if (type.IsSubclassOf(typeof(Block)))
        {
            string blockItemPath = "Sprites/item/block_item/" + material;
            if (Resources.Load<Sprite>(blockItemPath) != null)
                return blockItemPath;
        
            return "Sprites/block/" + material;
        }

        return "Sprites/item/" + material;
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

    public void ApplyDurability(int durabilityToApply = 1)
    {
        //Don't apply durability to items that don't have durability
        if (GetMaxDurability() == -1) return;
        
        durability -= durabilityToApply;

        //Subtract one item and reset durability when it reaches 0
        if (durability <= 0)
        {
            Amount--;
            durability = GetMaxDurability();
        }
    }

    public int GetMaxDurability()
    {
        if (material == Material.Air) return -1;

        Type type = Type.GetType(material.ToString());
        if (type == null || !type.IsSubclassOf(typeof(Item))) return -1;

        Item item = (Item)Activator.CreateInstance(type);
        return item.maxDurability;
    }

    public float GetItemEntityDamage()
    {
        if (material == Material.Air) return 1;

        Type type = Type.GetType(material.ToString());
        if (type == null || !type.IsSubclassOf(typeof(Item))) return 1;

        Item item = (Item)Activator.CreateInstance(type);
        return item.entityDamage;
    }
    
    public void Drop(Location location, bool randomVelocity = false)
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