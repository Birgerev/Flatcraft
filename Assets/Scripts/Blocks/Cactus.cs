using System.Collections.Generic;

public class Cactus : Vegetation
{
    private const int MaxGrowHeight = 3;
    
    public override float breakTime { get; } = 0.65f;
    public override bool isFlammable { get; } = true;
    public override float averageRandomTickDuration { get; } = 18 * 60;

    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Wool;

    public override void Tick()
    {
        Material matAbove = (location + new Location(0, 1)).GetMaterial();
        //If block above is not air or this block, break
        if (matAbove != Material.Air && matAbove != GetMaterial())
            Break();
        
        base.Tick();
    }

    public override void RandomTick()
    {
        base.RandomTick();
        
        Grow();
    }

    private void Grow()
    {
        Location aboveLoc = location + new Location(0, 1);

        if (aboveLoc.GetMaterial() == Material.Air && GetHeight() < MaxGrowHeight)
        {
            aboveLoc.SetMaterial(GetMaterial());
        }
    }

    private int GetHeight()
    {
        int height = 0;
        Location loc = location;
        while (loc.GetMaterial() == GetMaterial())
        {
            height++;
            loc = loc + new Location(0, -1);
        }

        return height;
    }

    protected override List<Material> ValidGround()
    {
        return new List<Material>() {Material.Sand, GetMaterial()};
    }
}