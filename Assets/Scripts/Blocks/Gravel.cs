using UnityEngine;
using Random = System.Random;

public class Gravel : Block
{
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type properToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Gravel;

    public override void Drop()
    {
        if (new Random(SeedGenerator.SeedByWorldLocation(location)).NextDouble() <= 0.1f)
            new ItemStack(Material.Flint, 1).Drop(location);
        else
            new ItemStack(Material.Gravel, 1).Drop(location);
    }

    public override void Tick()
    {
        if ((location + new Location(0, -1)).GetMaterial() == Material.Air)
        {
            FallingBlock fs = (FallingBlock) Entity.Spawn("FallingBlock");
            fs.transform.position = location.GetPosition() - new Vector2(0, 0.5f);
            fs.material = GetMaterial();

            location.SetMaterial(Material.Air).Tick();
        }

        base.Tick();
    }
}