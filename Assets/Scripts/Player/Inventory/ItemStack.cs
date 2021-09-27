﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public struct ItemStack
{
    public int amount;
    public string data;
    public int durability;
    public Material material;

    public ItemStack(Material material)
    {
        this.material = material;
        this.amount = 1;
        this.durability = -1;
        this.data = "";
        
        this.durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount)
    {
        this.material = material;
        this.amount = amount;
        this.durability = -1;
        this.data = "";
        
        this.durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount, string data)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
        this.durability = -1;
        
        this.durability = GetMaxDurability();
    }

    public ItemStack(Material material, int amount, string data, int durability)
    {
        this.material = material;
        this.amount = amount;
        this.data = data;
        this.durability = durability;
    }

    public ItemStack(string saveString)
    {
        string[] values = saveString.Split('*');

        this.material = (Material) Enum.Parse(typeof(Material), values[0]);
        this.amount = int.Parse(values[1]);
        this.data = values[2];
        this.durability = int.Parse(values[3]);
    }

    public string GetTexture()
    {
        Type materialType = Type.GetType(material.ToString());
        string texture = "invalid item class (not inheriting from Block nor Item)";

        if (materialType.IsSubclassOf(typeof(Block)))
            texture = ((Block) Activator.CreateInstance(materialType)).texture;
        else if (materialType.IsSubclassOf(typeof(Item)))
            texture = ((Item) Activator.CreateInstance(materialType)).texture;

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
        if (material == Material.Air || amount <= 0)
            return;

        GameObject obj = Entity.Spawn("DroppedItem").gameObject;

        obj.GetComponent<DroppedItem>().Location = location;
        obj.GetComponent<DroppedItem>().item = this;
        obj.GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public override string ToString()
    {
        return material + "*" + amount + "*" + data + "*" + durability;
    }
}