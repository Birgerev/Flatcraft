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
}
