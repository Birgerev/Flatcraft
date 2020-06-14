using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
