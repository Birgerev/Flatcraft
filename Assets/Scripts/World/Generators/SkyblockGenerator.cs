
public class SkyblockGenerator : WorldGenerator
{
    public override Material GenerateTerrainBlock(Location loc)
    {
        return Material.Air;
    }

    public override void GenerateStructures(Location loc)
    {
        if(loc.x == 0 && loc.y == 60)
            loc.SetMaterial(Material.Structure_Block).SetData(new BlockData("structure=Sky_Block"));
    }
}
