using System.IO;
using UnityEngine;

public struct ChunkPosition
{
    public int chunkX;
    public Dimension dimension;
    public int worldX => chunkX * Chunk.Width;

    public ChunkPosition(int chunkX, Dimension dimension)
    {
        this.chunkX = chunkX;
        this.dimension = dimension;
    }

    public ChunkPosition(Location loc)
    {
        var chunkX = 0;
        if (loc.x >= 0)
            chunkX = (int) (loc.x / (float) Chunk.Width);
        else
            chunkX = Mathf.CeilToInt((loc.x + 1f) / Chunk.Width) - 1;

        this.chunkX = chunkX;
        dimension = loc.dimension;
    }

    public bool HasBeenSaved()
    {
        var path = WorldManager.world.getPath() + "\\region\\" + dimension + "\\" + chunkX;

        return Directory.Exists(path);
    }

    public bool HasBeenGenerated()
    {
        if (!HasBeenSaved())
            return false;

        var chunkDataPath = WorldManager.world.getPath() + "\\region\\" + dimension + "\\" + chunkX + "\\chunk";
        var chunkDataLines = File.ReadAllLines(chunkDataPath);
        foreach (var line in chunkDataLines)
            if (line.Contains("hasBeenGenerated"))
            {
                if (line.Split('=')[1] == "true")
                    return true;
                return false;
            }

        return false;
    }

    public bool IsChunkLoaded()
    {
        return WorldManager.instance.chunks.ContainsKey(this);
    }

    public Chunk CreateChunk()
    {
        if (IsChunkLoaded())
            return null;

        var newChunk = Object.Instantiate(WorldManager.instance.chunkPrefab);
        newChunk.GetComponent<Chunk>().chunkPosition = this;
        return newChunk.GetComponent<Chunk>();
    }

    public void CreateChunkPath()
    {
        var path = WorldManager.world.getPath() + "\\region\\" + dimension + "\\" + chunkX;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\entities");
            File.Create(path + "\\blocks").Close();
            File.Create(path + "\\chunk").Close();
        }
    }

    public Chunk GetChunk()
    {
        Chunk chunk;

        WorldManager.instance.chunks.TryGetValue(this, out chunk);

        return chunk;
    }

    public bool isInRenderDistance()
    {
        return IsWithinDistanceOfPlayer(Chunk.RenderDistance);
    }

    public bool IsWithinDistanceOfPlayer(int range)
    {
        Location playerLocation = new Location(0, 0);
        if (Player.localInstance != null)
            playerLocation = Player.localInstance.Location;

        if (dimension != playerLocation.dimension)
            return false;
        if (chunkX == 0)
            return true;

        float distanceFromPlayer = Mathf.Abs(worldX + Chunk.Width / 2 - playerLocation.x);

        return distanceFromPlayer < range * Chunk.Width;
    }
}