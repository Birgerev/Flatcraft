using System;

public class Grass : Vegetation
{
    public override string[] randomTextures { get; } =
        {"grass", "grass_1", "grass_2", "grass_3", "grass_4"};

    public override ItemStack GetDrop()
    {
        if (new Random().NextDouble() <= 0.25f)
            return new ItemStack(Material.Wheat_Seeds, 1);

        return new ItemStack();
    }
}