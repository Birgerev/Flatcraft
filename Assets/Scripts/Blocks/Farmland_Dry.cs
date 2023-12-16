public class Farmland_Dry : Block
{
    public override float breakTime { get; } = 0.75f;
    public override float averageRandomTickDuration { get; } = 5;

    public override Tool_Type properToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Dirt;

    public override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Dirt)};
    }

    public override void RandomTick()
    {
        CheckWater();

        base.RandomTick();
    }

    public void CheckWater()
    {
        bool hasWater = false;
        for (int x = -4; x <= 4; x++)
            if ((location + new Location(x, 0)).GetMaterial() == Material.Water)
            {
                hasWater = true;
                break;
            }

        if (!hasWater)
            DryUp();
        if (hasWater)
            BecomeWet();
    }

    public void DryUp()
    {
        location.SetMaterial(Material.Dirt);
    }

    public void BecomeWet()
    {
        location.SetMaterial(Material.Farmland_Wet);
    }
}