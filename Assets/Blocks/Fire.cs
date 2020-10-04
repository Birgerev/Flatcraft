using System;

public class Fire : Block
{
    public static string default_texture = "block_fire_0";
    public override string[] alternative_textures { get; } = {"block_fire_0", "block_fire_1", "block_fire_2"};
    public override float change_texture_time { get; } = 0.3f;

    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override float averageRandomTickDuration { get; } = 10;
    public override int glowLevel { get; } = 15;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;    //TODO new fire sound

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void RandomTick()
    {
        var random = new Random();
        bool spread = (random.NextDouble() < 0.5d);


        if (spread)
        {
            var spreadLocation = new Location();

            int attempts = 0;
            while (spreadLocation.Equals(new Location()))
            {
                var x = random.Next(-1, 1);
                var y = random.Next(-1, 1);

                if ((location + new Location(x, y)).GetMaterial() == Material.Air &&
                    (location + new Location(x, y - 1)).GetMaterial() != Material.Air)
                    (location + new Location(x, y)).SetMaterial(Material.Fire);

                if (attempts > 20)
                    return;
                attempts++;
            }

            spreadLocation.SetMaterial(Material.Fire).Tick();
        }
        else
        {
            location.SetMaterial(Material.Air);
        }

        base.RandomTick();
    }

    public override void Tick()
    {
        base.Tick();
    }
}