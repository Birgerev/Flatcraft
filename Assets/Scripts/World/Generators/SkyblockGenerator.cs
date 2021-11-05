public class SkyblockGenerator : WorldGenerator
{
    public override Material GenerateTerrainBlock(Location loc)
    {
        return Material.Air;
    }

    public override BlockState GenerateStructures(Location loc, Biome biome)
    {
        if (loc.x == 0 && loc.y == 60 && loc.dimension == Dimension.Overworld)
            return new BlockState(Material.Structure_Block, new BlockData("structure=Sky_Block/Grass"));
        
        if (loc.x == 65 && loc.y == 60 && loc.dimension == Dimension.Overworld)
            return new BlockState(Material.Structure_Block, new BlockData("structure=Sky_Block/Sand"));
        
        if (loc.x == 0 && loc.y == 60 &&  loc.dimension == Dimension.Nether)
            return new BlockState(Material.Structure_Block, new BlockData("structure=Sky_Block/Nether"));

        return new BlockState(Material.Air);
    }
}