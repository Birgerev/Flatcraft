using System.Text;

public class SeedGenerator
{
    public static int SeedByLocation(Location loc)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append((char) (WorldManager.world.seed % 65535));
        builder.Append((char) (loc.x % 65535));
        builder.Append((char) (loc.y % 65535));
        builder.Append((char) ((int) loc.dimension % 65535));

        return builder.GetHashCode();
    }
}