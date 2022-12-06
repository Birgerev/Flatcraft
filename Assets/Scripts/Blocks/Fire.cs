using UnityEngine;
using Random = System.Random;

public class Fire : Block
{
    public override string texture { get; set; } = "block_fire_0";
    public override string[] alternativeTextures { get; } = {"block_fire_0", "block_fire_1", "block_fire_2"};
    public override float changeTextureTime { get; } = 0.3f;

    public override bool solid { get; set; } = false;
    public override bool trigger { get; set; } = true;
    public override float breakTime { get; } = 0.01f;
    public override bool requiresGround { get; } = true;
    public override float averageRandomTickDuration { get; } = 1;
    public override int glowLevel { get; } = 5;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Fire;

    public override void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null)
            col.GetComponent<Entity>().fireTime = 7;

        base.OnTriggerStay2D(col);
    }

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void RandomTick()
    {
        Random random = new Random();
        bool ambinetSound = random.NextDouble() < 0.2d;
        bool spread = random.NextDouble() < 1d;
        bool burnUp = random.NextDouble() < 0.6d;

        if (ambinetSound)
            Sound.Play(location, "block/fire/ambient", SoundType.Block, 0.8f, 1.2f);

        if (spread)
            AttemptSpreadFire();
        
        if(burnUp)
        {
            bool netherrackBelow = (location + new Location(0, -1)).GetMaterial() == Material.Netherrack;

            if (!netherrackBelow)
            {
                location.SetMaterial(Material.Air).Tick();
                if ((location + new Location(0, -1)).GetBlock().isFlammable)
                    (location + new Location(0, -1)).SetMaterial(Material.Air).Tick();
            }
        }

        base.RandomTick();
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

            Block blockBelow = (loc + new Location(0, -1)).GetBlock();

            //If target loc isn't empty
            if (loc.GetMaterial() != Material.Air)
                continue;
            //If below target loc is empty or isn't flammable
            if (blockBelow.GetMaterial() == Material.Air || !blockBelow.isFlammable)
                continue;

            //Otherwise, ignite
            loc.SetMaterial(Material.Fire);
            return;
        }
    }
}