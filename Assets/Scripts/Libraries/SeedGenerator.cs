using System;
using System.Text;

public class SeedGenerator
{
    public static int SeedByWorldLocation(Location loc)
    {
        return SeedByParameters(WorldManager.world.seed, loc.x, loc.y, (int)loc.dimension);
    }
    
    public static int SeedByParameters(params int[] parameters)
    {
        var hash = new HashCode();

        foreach (var n in parameters)
        {
            hash.Add(n);
        }

        return hash.ToHashCode();
    }
}