using UnityEngine;
using Random = System.Random;

public class Fire : Block
{
    public override string[] RandomTextures { get; } = {"fire", "fire_1", "fire_2"};
    public override float ChangeTextureTime { get; } = 0.3f;

    public override bool Solid { get; set; } = false;
    public override bool Trigger { get; set; } = true;
    public override float BreakTime { get; } = 0.01f;
    public override bool RequiresGround { get; } = true;
    public override float AverageRandomTickDuration { get; } = 5;
    public override LightValues LightSourceValues { get; } = new LightValues(15, new Color(1, .6f, .4f), true);

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Fire;

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null)
            col.GetComponent<Entity>().fireTime = 7;
    }

    protected override ItemStack[] GetDrops()
    {
        return null;
    }

    public override void RandomTick()
    {
        Random random = new Random();
        bool ambinetSound = random.NextDouble() < 0.3d;
        bool spread = random.NextDouble() < .8d;
        bool burnUp = random.NextDouble() < 0.3d;

        if (ambinetSound)
            Sound.Play(location, "block/fire/ambient", SoundType.Block, 0.8f, 1.2f);

        if (spread)
            AttemptSpreadFire();
        
        if(burnUp)
        {
            bool netherrackBelow = (location + new Location(0, -1)).GetMaterial() == Material.Netherrack;

            if (!netherrackBelow)
            {
                //Extinguish
                location.SetMaterial(Material.Air).Tick();
                
                //If block below is flammable, it will burn up
                if ((location + new Location(0, -1)).GetBlock().IsFlammable)
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
            Location targetLoc = (location + new Location(random.Next(-1, 1 + 1), random.Next(-2, 2 + 1)));

            //Skip location if it goes out of bounds
            if (targetLoc.y <= 0 || targetLoc.y > Chunk.Height)
                continue;

            Block blockBelowTarget = (targetLoc + new Location(0, -1)).GetBlock();

            //If target loc isn't empty, return
            if (targetLoc.GetMaterial() != Material.Air)
                continue;
            //If below target loc is empty or isn't flammable
            if (blockBelowTarget == null || !blockBelowTarget.IsFlammable)
                continue;

            //Otherwise, ignite
            targetLoc.SetMaterial(Material.Fire);
            return;
        }
    }
}