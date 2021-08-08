public class Farmland_Wet : Block
{
    public override string texture { get; set; } = "block_farmland_wet";
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
    }

    public void DryUp()
    {
        location.SetMaterial(Material.Farmland_Dry);
    }
}