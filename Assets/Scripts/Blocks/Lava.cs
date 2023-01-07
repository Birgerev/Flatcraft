using System;

public class Lava : Liquid
{
    public override string texture { get; set; } = "block_lava_3";
    public override string[] liquidTextures { get; } =
    {
        "block_lava_0", "block_lava_1", "block_lava_2", "block_lava_3"
    };

    public override int maxLiquidLevel { get; } = 4;
    public override int glowLevel { get; } = 15;
    public override float averageRandomTickDuration { get; } = 40;

    public override void RandomTick()
    {
        base.RandomTick();

        AttemptSpreadFire();
    }

    private void AttemptSpreadFire()
    {
        Random random = new Random();

        for (int attempt = 0; attempt < 10; attempt++)
        {
            //+1 because upper bound is exclusive
            Location loc = (location + new Location(random.Next(-1, 1 + 1), random.Next(-2, 2 + 1)));

            //Skip location if it goes out of bounds
            if (loc.y <= 0 || loc.y > Chunk.Height)
                continue;

            Location blockBelow = (loc + new Location(0, -1));

            //If target loc isn't empty
            if (loc.GetMaterial() != Material.Air)
                continue;
            //If below target loc is empty or isn't flammable
            if (blockBelow.GetMaterial() == Material.Air || !blockBelow.GetBlock().isFlammable)
                continue;

            //Otherwise, ignite
            loc.SetMaterial(Material.Fire);
            return;
        }
    }
}