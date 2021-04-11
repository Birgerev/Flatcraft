using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlock : MonoBehaviour
{
    public static Dictionary<Material, Material> viableMaterials = new Dictionary<Material, Material>
    {
        {Material.Stone, Material.Stone},
        {Material.Cobblestone, Material.Cobblestone},
        {Material.Dirt, Material.Dirt},
        {Material.Oak_Planks, Material.Oak_Planks},
        {Material.Obsidian, Material.Obsidian},
        {Material.Sand, Material.Sand},
        {Material.Sandstone, Material.Sand},
        {Material.Grass, Material.Dirt},
        {Material.Wooden_Door_Bottom, Material.Oak_Planks},
        {Material.Wooden_Door_Top, Material.Oak_Planks},
        {Material.Wooden_Trapdoor, Material.Oak_Planks},
    };
        
    public Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = new ItemStack(material).GetSprite();
    }
}
