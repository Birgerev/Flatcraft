using System;

public class Oak_Leaves : Leaves
{
    protected override ItemStack[] GetDrops()
    {
        Random r = new Random();
        
        if(r.NextDouble() < 0.2f)
            return new[] { new ItemStack(Material.Oak_Sapling)};
        if(r.NextDouble() < 0.1f)
            return new[] { new ItemStack(Material.Stick, r.Next(1, 2 + 1))};
        
        //TODO if(r.NextDouble() < 0.03f)
        //    return new ItemStack(Material.Apple);
        
        return null;
    }
}
