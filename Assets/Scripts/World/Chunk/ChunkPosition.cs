using System.IO;
using Mirror;
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
        int chunkX = 0;
        if (loc.x >= 0)
            chunkX = (int) (loc.x / (float) Chunk.Width);
        else
            chunkX = Mathf.CeilToInt((loc.x + 1f) / Chunk.Width) - 1;

        this.chunkX = chunkX;
        dimension = loc.dimension;
    }

    public bool HasBeenSaved()
    {
        string path = WorldManager.world.GetPath() + "\\chunks\\" + dimension + "\\" + chunkX;

        return Directory.Exists(path);
    }

    public bool HasBeenGenerated()
    {
        if (!HasBeenSaved())
            return false;

        string chunkDataPath = WorldManager.world.GetPath() + "\\chunks\\" + dimension + "\\" + chunkX + "\\chunk";
        string[] chunkDataLines = File.ReadAllLines(chunkDataPath);
        foreach (string line in chunkDataLines)
            if (line.Contains("hasBeenGenerated"))
            {
                if (line.Split('=')[1] == "true")
                    return true;
                return false;
            }

        return false;
    }

    public bool IsChunkCreated()
    {
        if (WorldManager.instance == null)
            return true;

        return WorldManager.instance.chunks.ContainsKey(this);
    }

    public bool IsChunkLoaded()
    {
        if (!IsChunkCreated())
            return false;

        return GetChunk().isLoaded;
    }


    public Chunk CreateChunk()
    {
        if (IsChunkCreated())
            return null;

        GameObject newChunk = Object.Instantiate(WorldManager.instance.chunkPrefab);
        newChunk.GetComponent<Chunk>().chunkPosition = this;
        NetworkServer.Spawn(newChunk);

        return newChunk.GetComponent<Chunk>();
    }

    public void CreateChunkPath()
    {
        string path = WorldManager.world.GetPath() + "\\chunks\\" + dimension + "\\" + chunkX;
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
        Chunk chunk = (Chunk) WorldManager.instance.chunks[this];

        return chunk;
    }

    public bool IsInRenderDistance()
    {
        return IsWithinDistanceOfPlayer(Chunk.RenderDistance);
    }

    public bool IsWithinDistanceOfPlayer(int range)
    {
        foreach (Player player in Player.players)
        {
            Location loc = player.Location;

            if (dimension != loc.dimension)
                continue;
            
            float distanceFromPlayer = Mathf.Abs(worldX + Chunk.Width / 2 - loc.x);
            if (distanceFromPlayer < range * Chunk.Width)
                return true;
        }

        return false;
    }
}