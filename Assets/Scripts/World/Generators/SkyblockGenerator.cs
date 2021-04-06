
public class SkyblockGenerator : WorldGenerator
{
    public override Material GenerateTerrainBlock(Location loc)
    {
        return Material.Air;
    }

    public override BlockState GenerateStructures(Location loc, Biome biome)
    {
        if(loc.x == 0 && loc.y == 60)
            return new BlockState(Material.Structure_Block, new BlockData("structure=Sky_Block"));

        return new BlockState(Material.Air);
    }
}
