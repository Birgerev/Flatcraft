using System;
using System.Collections.Generic;

public class Tall_Grass : Vegetation
{
    public override ItemStack GetDrop()
    {
        if (new Random().NextDouble() <= 0.25f)
            return new ItemStack(Material.Wheat_Seeds, 1);

        return new ItemStack();
    }

    public override void GeneratingTick()
    {
        base.GeneratingTick();
        
        Location above = (location + new Location(0, 1));
        above.SetMaterial(Material.Tall_Grass);
    }

    public override void Break(bool drop)
    {
        base.Break(drop);
        
        //If this block is destroyed, destroy supporting tall grass block below 
        Location below = (location + new Location(0, -1));
        if (below.GetMaterial() == GetMaterial())
            below.GetBlock().Break(false);
    }

    public override List<Material> ValidGround()
    {
        List<Material> mats = base.ValidGround();
        
        //Add current material as valid ground, to enable stacking
        mats.Add(GetMaterial());
        
        return mats;
    }
}