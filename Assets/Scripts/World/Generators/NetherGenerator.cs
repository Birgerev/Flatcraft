using System;

public class NetherGenerator : WorldGenerator
{
    public const int LavaLevel = 32;

    public override Material GenerateTerrainBlock(Location loc)
    {
        if (loc.y > 128)
            return Material.Air;
        
        Random r = new Random(SeedGenerator.SeedByWorldLocation(loc));
        ChunkPosition cPos = new ChunkPosition(loc);
        Biome biome = Biome.GetBiomeAt(cPos);

        Material mat = Material.Air;

        float noiseValue = biome.GetLandscapeNoiseAt(loc);

        //-Netherrack generation-//
        if (noiseValue > 0.1f)
            mat = Material.Netherrack;

        //-Guaranteed Netherrack-//
        if (loc.y >= 100)
            mat = Material.Netherrack;
        if (loc.y <= 16)
            mat = Material.Netherrack;
        
        //TODO fortress
        

        //-Caves-//
        if (WorldManager.instance.caveHollowBlocks.Contains(loc))
            mat = Material.Air;

        //-Lava Sea-//
        if (mat == Material.Air && loc.y <= LavaLevel)
            mat = Material.Lava;

        //-Random Lava-//
        if(mat == Material.Netherrack && r.NextDouble() < 0.003d)
            mat = Material.Lava;

        //-Lower Bedrock Generation-//
        if (loc.y <= 4)
        {
            //Fill layer 0 and then progressively less chance of bedrock further up
            if (r.Next(0, loc.y + 2) <= 1)
                mat = Material.Bedrock;
        }

        //-Upper Bedrock Generation-//
        if (loc.y >= 128 - 4 && loc.y <= 128)
        {
            //Fill layer 256 and then progressively less chance of bedrock further down
            if (r.Next(0, 128 - loc.y + 2) <= 1)
                mat = Material.Bedrock;
        }
        
        return mat;
    }
    
    public override BlockState GenerateStructures(Location loc, Biome biome)
    {
        Random r = new Random(SeedGenerator.SeedByWorldLocation(loc));
        Material mat = loc.GetMaterial();
        Material matBeneath = (loc + new Location(0, -1)).GetMaterial();
        
        //Generate Liquid Pockets
        if (loc.y == LavaLevel && mat == Material.Lava && r.NextDouble() <= 0.005d)
        {
            return new BlockState(Material.Structure_Block, new BlockData("structure=Nether_Fortress"));
        }

        return new BlockState(Material.Air);
    }
}