using UnityEngine;
using Random = System.Random;

public class Gravel : Block
{
    public override float BreakTime { get; } = 0.75f;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Gravel;

    protected override ItemStack[] GetDrops()
    {
        if (new Random().NextDouble() < 0.1f)
            return new[] { new ItemStack(Material.Flint) };

        return base.GetDrops();
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