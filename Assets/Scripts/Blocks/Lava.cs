﻿using System;

public class Lava : Liquid
{
    public override string texture { get; set; } = "block_lava_3";
    public override string[] liquidTextures { get; } =
    {
        "block_lava_0", "block_lava_1", "block_lava_2", "block_lava_3"
    };

    public override int maxLiquidLevel { get; } = 4;
    public override int glowLevel { get; } = 15;
    public override float averageRandomTickDuration { get; } = 60;

    public override void RandomTick()
    {
        Random random = new Random();
        Location spreadLocation = new Location();

        int attempts = 0;
        while (spreadLocation.Equals(new Location()))
        {
            int x = random.Next(-1, 1 + 1); //+1 because upper bound is exclusive
            int y = random.Next(-2, 2 + 1);

            if ((location + new Location(x, y)).GetMaterial() == Material.Air &&
                (location + new Location(x, y - 1)).GetMaterial() != Material.Air &&
                (location + new Location(x, y - 1)).GetBlock() != null &&
                (location + new Location(x, y - 1)).GetBlock().isFlammable)
                (location + new Location(x, y)).SetMaterial(Material.Fire);

            if (attempts > 20)
                return;
            attempts++;
        }

        spreadLocation.SetMaterial(Material.Fire).Tick();

        base.RandomTick();
    }
}