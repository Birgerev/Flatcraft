using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct ChunkPosition
{
    public int chunkX;
    public Dimension dimension;
    public int worldX { get
        {
            return chunkX * Chunk.Width; 
        } }

    public ChunkPosition(int chunkX, Dimension dimension)
    {
        this.chunkX = chunkX;
        this.dimension = dimension;
    }
    
    public ChunkPosition(Location loc)
    {
        int chunkX = 0;
        if (loc.x >= 0)
        {
            chunkX = (int)((float)loc.x / (float)Chunk.Width);
        }
        else
        {
            chunkX = Mathf.CeilToInt(((float)loc.x + 1f) / (float)Chunk.Width) - 1;
        }
        
        this.chunkX = chunkX;
        this.dimension = loc.dimension;
    }

    public bool HasBeenSaved()
    {
        string path = WorldManager.world.getPath() + "\\region\\" + this.dimension + "\\" + this.chunkX;
        
        return Directory.Exists(path);
    }

    public bool HasBeenGenerated()
    {
        if (!HasBeenSaved())
            return false;
        
        string chunkDataPath = WorldManager.world.getPath() + "\\region\\" + dimension + "\\" + chunkX + "\\chunk";
        string[] chunkDataLines = File.ReadAllLines(chunkDataPath);
        foreach (string line in chunkDataLines)
        {
            if (line.Contains("hasBeenGenerated"))
            {
                if (line.Split('=')[1] == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        return false;
    }
    
    public bool IsChunkLoaded()
    {
        return WorldManager.instance.chunks.ContainsKey(this);
    }

    public Chunk CreateChunk()
    {
        if (this.IsChunkLoaded())
            return null;
        
        GameObject newChunk = GameObject.Instantiate(WorldManager.instance.chunkPrefab);
        newChunk.GetComponent<Chunk>().chunkPosition = this;
        return newChunk.GetComponent<Chunk>();
    }

    public void CreateChunkPath()
    {
        string path = WorldManager.world.getPath() + "\\region\\" + this.dimension + "\\" + this.chunkX;
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
        if (this.chunkX == 0)
            return true;

        Location playerLocation;


        if (Player.localInstance == null)
            playerLocation = new Location(0, 0);
        else
            playerLocation = Player.localInstance.location;

        float distanceFromPlayer = Mathf.Abs((this.worldX + (Chunk.Width/2)) - playerLocation.x);

        return distanceFromPlayer < range * Chunk.Width;
    }
}
