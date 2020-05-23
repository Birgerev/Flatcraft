using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tall_Grass : Block
{
    public static string default_texture = "block_tall_grass_0";
    public override string[] alternative_textures { get; } = { "block_tall_grass_0", "block_tall_grass_1", "block_tall_grass_2" };

    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.3f;
    public override bool requiresGround { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;
    
    public override ItemStack GetDrop()
    {
        float r = getRandomChance();

        if (r < 0.25f)
            return new ItemStack(Material.Wheat_Seeds, 1);
        else
            return new ItemStack();
    }
}
