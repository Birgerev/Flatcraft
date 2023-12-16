using System;

public class Grass_Block : Block
{
    public override float breakTime { get; } = 0.75f;
    public override float averageRandomTickDuration { get; } = 20;

    public override Tool_Type properToolType { get; } = Tool_Type.Shovel;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Grass;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
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
        if (!(blockAbove.solid || blockAbove is Liquid))
            return;
        
        location.SetMaterial(Material.Dirt);
    }

    public void TrySpread()
    {
        Random r = new Random();

        Location targetLoc = location + new Location(r.NextDouble() > 0.5f ? 1 : -1, r.Next(-1, 1 + 1));
        Block blockAboveTarget = (targetLoc + new Location(0, 1)).GetBlock();
        if (targetLoc.GetMaterial() == Material.Dirt && (blockAboveTarget == null || !blockAboveTarget.solid))
            targetLoc.SetMaterial(Material.Grass_Block);
    }
}