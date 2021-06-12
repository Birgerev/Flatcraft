using System.Text;

public class SeedGenerator
{
    public static int SeedByLocation(Location loc)
    {
        return SeedByParameters(WorldManager.world.seed, loc.x, loc.y, (int)loc.dimension);
    }
    
    public static int SeedByParameters(params int[] parameters)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var parameter in parameters)
        {
            builder.Append((char) (parameter % 65535));
        }

        return builder.GetHashCode();
    }
}