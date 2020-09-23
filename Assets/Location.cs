using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

public struct Location
{
    public int x;
    public int y;
    public Dimension dimension;


    public Location(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.dimension = Dimension.Overworld;
    }

    public Location(int x, int y, Dimension dimension)
    {
        this.x = x;
        this.y = y;
        this.dimension = dimension;
    }

    public static Location LocationByPosition(Vector3 pos, Dimension dimension)
    {
        return new Location(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), dimension);
    }
    
    public Vector2 GetPosition()
    {
        return new Vector2(x, y);
    }

    
    public Location SetMaterial(Material mat)
    {
        if (SaveManager.blockChanges.ContainsKey(this))
        {
            SaveManager.blockChanges.Remove(this);
        }
    
        SaveManager.blockChanges.Add(this, mat.ToString() + "*");    //Add block change to list, without old data, as it should be reset whenever the block is replaced

        ChunkPosition cPos = new ChunkPosition(this);
        if (cPos.IsChunkLoaded())
        {
            Chunk chunk = cPos.GetChunk();

            chunk.CreateLocalBlock(this, mat, "");
        }
        
        return this;
    }

    public Location SetData(string data)
    {
        string oldMaterial = GetMaterial().ToString();    //Get current material, and later apply it to our new block change entry

        if (SaveManager.blockChanges.ContainsKey(this))
        {
            string blockChangeLine = SaveManager.blockChanges[this];    //Get old material from save data
            oldMaterial = blockChangeLine.Split('*')[0];
            SaveManager.blockChanges.Remove(this);
        }
        
        SaveManager.blockChanges.Add(this, oldMaterial + "*" + data);

        Block block = GetBlock();
        if (block != null)
        {
            block.data = Block.dataFromString(data);
        }
        
        return this;
    }

    public Block GetBlock()
    {
        ChunkPosition cPos = new ChunkPosition(this);
        Chunk chunk = cPos.GetChunk();
        if (chunk != null)
        {
            Block block = chunk.getLocalBlock(this);

            return block;
        }

        return null;
    }

    public void Tick()
    {
        List<Block> blocks = new List<Block>();

        blocks.Add((this + new Location(0, 0)).GetBlock());
        blocks.Add((this + new Location(0, 1)).GetBlock());
        blocks.Add((this + new Location(0, -1)).GetBlock());
        blocks.Add((this + new Location(-1, 0)).GetBlock());
        blocks.Add((this + new Location(1, 0)).GetBlock());

        foreach (Block blockToTick in blocks) {
            if (blockToTick != null)
            {
                blockToTick.Tick();
            }
        }
    }

    public Material GetMaterial()
    {
        //Get Material from block changes
        if (SaveManager.blockChanges.ContainsKey(this))
        {
            string blockChangeLine = SaveManager.blockChanges[this];
            return (Material) System.Enum.Parse(typeof(Material), blockChangeLine.Split('*')[0]);
        }
        
        ChunkPosition cPos = new ChunkPosition(this);
        
        //Get material from loaded chunk
        if (cPos.IsChunkLoaded())
        {
            Block block = cPos.GetChunk().getLocalBlock(this);

            if (block == null)
                return Material.Air;
            else
                return block.GetMaterial();
        }
        
        //Get material from saved chunk data
        if (!cPos.HasBeenSaved())
            return Material.Air;
        string chunkPath = WorldManager.world.getPath() + "\\region\\" + cPos.dimension + "\\" + cPos.chunkX;   
        foreach (string line in File.ReadAllLines(chunkPath + "\\blocks"))
        {
            Location lineLoc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
            Material lineMaterial = (Material) System.Enum.Parse(typeof(Material), line.Split('*')[1]);
            string lineData = line.Split('*')[2];

            if (lineLoc.Equals(this))
            {
                return lineMaterial;
            }
        }

        return Material.Air;
    }

    public string GetData()
    {
        //Get data from block changes
        if (SaveManager.blockChanges.ContainsKey(this))
        {
            string blockChangeLine = SaveManager.blockChanges[this];
            return blockChangeLine.Split('*')[1];
        }
        
        ChunkPosition cPos = new ChunkPosition(this);

        //Get data from loaded chunk
        if (cPos.IsChunkLoaded())
        {
            Block block = cPos.GetChunk().getLocalBlock(this);

            if (block == null)
                return "";
            else
                return Block.stringFromData(block.data);
        }
        
        //Get data from saved chunk data
        if (!cPos.HasBeenSaved())
            return "";
        string chunkPath = WorldManager.world.getPath() + "\\region\\" + cPos.dimension + "\\" + cPos.chunkX;   
        foreach (string line in File.ReadAllLines(chunkPath + "\\blocks"))
        {
            Location lineLoc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]), int.Parse(line.Split('*')[0].Split(',')[1]));
            Material lineMaterial = (Material) System.Enum.Parse(typeof(Material), line.Split('*')[1]);
            string lineData = line.Split('*')[2];

            if (lineLoc.Equals(this))
            {
                return lineData;
            }
        }

        return "";
    }

    public static Location operator +(Location a, Location b)
    {
        return new Location(a.x + b.x, a.y + b.y, a.dimension);
    }
    public static Location operator -(Location a, Location b)
    {
        return new Location(a.x - b.x, a.y - b.y, a.dimension);
    }
}
