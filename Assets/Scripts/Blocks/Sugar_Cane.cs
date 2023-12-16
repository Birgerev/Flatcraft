using System;
using System.Collections.Generic;

public class Sugar_Cane : Vegetation
{
    private const int MaxHeight = 3;
    public override float AverageRandomTickDuration { get; } = 18 * 60;
    
    public override void RandomTick()
    {
        base.RandomTick();
        
        Grow();
    }

    private void Grow()
    {
        Location aboveLoc = location + new Location(0, 1);

        if (aboveLoc.GetMaterial() == Material.Air && GetHeight() < MaxHeight)
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
        List<Material> mats = base.ValidGround();
        
        //Add current material as valid ground, to enable stacking
        mats.Add(GetMaterial());
        mats.Add(Material.Sand);
        mats.Add(Material.Dirt);
        
        //TODO water check
        
        return mats;
    }
}