using System;
using System.Collections.Generic;

public class Sugar_Cane : Vegetation
{
    public override void GeneratingTick()
    {
        base.GeneratingTick();
        
        Random r = new Random(SeedGenerator.SeedByWorldLocation(location));
        
        int caneLength = 2 + r.Next(0, 2 + 1);
        for (int i = 0; i < caneLength; i++)
        {
            Location nAbove = (location + new Location(0, i));
            nAbove.SetMaterial(GetMaterial());
        }
    }

    //TODO water check
    
    protected override List<Material> ValidGround()
    {
        List<Material> mats = base.ValidGround();
        
        //Add current material as valid ground, to enable stacking
        mats.Add(GetMaterial());
        
        return mats;
    }
}