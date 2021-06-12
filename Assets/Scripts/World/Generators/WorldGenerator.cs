public class WorldGenerator
{
    public virtual Material GenerateTerrainBlock(Location loc)
    {
        return Material.TNT;
    }

    public virtual BlockState GenerateStructures(Location loc, Biome biome)
    {
        return new BlockState(Material.Air);
    }
}