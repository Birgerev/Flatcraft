using System;

public class Grass : Vegetation
{
    public override string[] randomTextures { get; } =
        {"block_grass", "block_grass_1", "block_grass_2", "block_grass_3", "block_grass_4"};

    public override ItemStack GetDrop()
    {
        if (new Random().NextDouble() <= 0.25f)
            return new ItemStack(Material.Wheat_Seeds, 1);

        return new ItemStack();
    }
}