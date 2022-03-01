using System;

public class Oak_Leaves : Leaves
{
    public override ItemStack GetDrop()
    {
        Random r = new Random();
        
        if(r.NextDouble() < 0.2f)
            return new ItemStack(Material.Oak_Sapling);
        //TODO if(r.NextDouble() < 0.03f)
        //    return new ItemStack(Material.Apple);
        if(r.NextDouble() < 0.1f)
            return new ItemStack(Material.Stick, r.Next(1, 2 + 1));

        return new ItemStack();
    }
}
