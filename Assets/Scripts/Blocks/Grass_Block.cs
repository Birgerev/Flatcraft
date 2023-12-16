using System;

public class Grass_Block : Block
{
    public override float BreakTime { get; } = 0.75f;
    public override float AverageRandomTickDuration { get; } = 20;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Dirt)};
    }

    public override void RandomTick()
    {
        TrySpread();
        TryDecay();

        base.RandomTick();
    }

    public override void GeneratingTick()
    {
        TryDecay();

        base.GeneratingTick();
    }

    public void TryDecay()
    {
        Block blockAbove = (location + new Location(0, 1)).GetBlock();
        
        //Return if no block above
        if (blockAbove == null)
            return;
        
        //Return if neither solid nor liquid
        if (!(blockAbove.Solid || blockAbove is Liquid))
            return;
        
        location.SetMaterial(Material.Dirt);
    }

    public void TrySpread()
    {
        Random r = new Random();

        Location targetLoc = location + new Location(r.NextDouble() > 0.5f ? 1 : -1, r.Next(-1, 1 + 1));
        Block blockAboveTarget = (targetLoc + new Location(0, 1)).GetBlock();
        if (targetLoc.GetMaterial() == Material.Dirt && (blockAboveTarget == null || !blockAboveTarget.Solid))
            targetLoc.SetMaterial(Material.Grass_Block);
    }
}