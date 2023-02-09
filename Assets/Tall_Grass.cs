using System;

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

    public override bool ValidGround()
    {
        Material below = (location + new Location(0, -1)).GetMaterial();

        //If were on top of tall grass, all is good
        if (below == GetMaterial())
            return true;

        return base.ValidGround();
    }
}