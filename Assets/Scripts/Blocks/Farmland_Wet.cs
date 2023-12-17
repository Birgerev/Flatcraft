public class Farmland_Wet : Block
{
    public override float BreakTime { get; } = 0.75f;
    public override float AverageRandomTickDuration { get; } = 5;


    public override Tool_Type ProperToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Dirt;

    protected override ItemStack[] GetDrops()
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
    }

    public void DryUp()
    {
        location.SetMaterial(Material.Farmland_Dry);
    }
}