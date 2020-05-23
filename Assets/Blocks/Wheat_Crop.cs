using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheat_Crop : Crop
{
    public static string default_texture = "block_wheat_crop_0";
    public override string[] crop_textures { get; } = { "block_wheat_crop_0", "block_wheat_crop_1", "block_wheat_crop_2", "block_wheat_crop_3" };
    
    public override Material seed { get; } = Material.Wheat_Seeds;
    public override Material result { get; } = Material.Wheat;
}
