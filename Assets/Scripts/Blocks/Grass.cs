using System;

public class Grass : Block
{
    public override string[] randomTextures { get; } =
        {"block_grass", "block_grass_1", "block_grass_2", "block_grass_3", "block_grass_4"};

    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;


    public override ItemStack GetDrop()
    {
        if (new Random().NextDouble() <= 0.25f)
            return new ItemStack(Material.Wheat_Seeds, 1);

        return new ItemStack();
    }
}