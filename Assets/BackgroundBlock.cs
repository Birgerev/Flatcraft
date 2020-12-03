using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class BackgroundBlock : MonoBehaviour
{
    public static List<Material> viableMaterials = new List<Material> {Material.Stone, Material.Cobblestone, Material.Dirt, Material.Grass, Material.Oak_Planks, Material.Obsidian, Material.Sand, Material.Sandstone};
    public Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        var type = Type.GetType(material.ToString());
        if (type == null)
            return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        var texture = (string) type.GetField("default_texture", BindingFlags.Public | BindingFlags.Static).GetValue(null);

        spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/" + texture);
    }
}
