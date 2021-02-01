using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlock : MonoBehaviour
{
    public static List<Material> viableMaterials = new List<Material> {Material.Stone, Material.Cobblestone, Material.Dirt, Material.Oak_Planks, Material.Obsidian, Material.Sand, Material.Sandstone};
    public Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = new ItemStack(material).GetSprite();
    }
}
