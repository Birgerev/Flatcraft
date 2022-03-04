using System;

public class Birch_Leaves : Leaves
{
    public override ItemStack GetDrop()
    {
        Random r = new Random();
        
        if(r.NextDouble() < 0.2f)
            return new ItemStack(Material.Birch_Sapling); 
        if(r.NextDouble() < 0.1f)
            return new ItemStack(Material.Stick, r.Next(1, 2 + 1));

        return new ItemStack();
    }
}
