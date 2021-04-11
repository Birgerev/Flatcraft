using System.IO;
using System.Runtime.InteropServices;
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
        var path = WorldManager.world.getPath() + "\\chunks\\" + dimension + "\\" + chunkX;

        return Directory.Exists(path);
    }

    public bool HasBeenGenerated()
    {
        if (!HasBeenSaved())
            return false;

        var chunkDataPath = WorldManager.world.getPath() + "\\chunks\\" + dimension + "\\" + chunkX + "\\chunk";
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
        
        var newChunk = Object.Instantiate(WorldManager.instance.chunkPrefab);
        NetworkServer.Spawn(newChunk);
        newChunk.GetComponent<Chunk>().chunkPosition = this;
        return newChunk.GetComponent<Chunk>();
    }

    public void CreateChunkPath()
    {
        var path = WorldManager.world.getPath() + "\\chunks\\" + dimension + "\\" + chunkX;
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
        Chunk chunk = (Chunk)WorldManager.instance.chunks[this];
        
        return chunk;
    }

    public bool isInRenderDistance()
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

            if (chunkX == 0)
                return true;
            
            float distanceFromPlayer = Mathf.Abs(worldX + Chunk.Width / 2 - loc.x);
            if (distanceFromPlayer < range * Chunk.Width)
                return true;
        }

        return false;
    }
}