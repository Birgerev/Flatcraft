using System;

public class Grass : Vegetation
{
    public override bool CanBeOverriden { get; set; } = true;
    
    public override string[] RandomTextures { get; } =
        {"grass", "grass_1", "grass_2", "grass_3", "grass_4"};

    protected override ItemStack[] GetDrops()
    {
        if (new Random().NextDouble() <= 0.25f)
            return new[] { new ItemStack(Material.Wheat_Seeds)};

        return null;
    }
}