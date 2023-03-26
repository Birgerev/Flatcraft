public class Farmland_Dry : Block
{
    public override float breakTime { get; } = 0.75f;
    public override float averageRandomTickDuration { get; } = 5;

    public override Tool_Type properToolType { get; } = Tool_Type.Shovel;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Dirt;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
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