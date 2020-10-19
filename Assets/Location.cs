using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public struct Location
{
    public readonly int x;
    public readonly int y;
    public readonly Dimension dimension;


    public Location(int x, int y)
    {
        this.x = x;
        this.y = y;
        dimension = Dimension.Overworld;
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
        if (SaveManager.blockChanges.ContainsKey(this)) SaveManager.blockChanges.Remove(this);

        SaveManager.blockChanges.Add(this, mat + "*"); //Add block change to list, without old data, as it should be reset whenever the block is replaced

        var cPos = new ChunkPosition(this);
        
        if (!cPos.IsChunkLoaded()) return this;
        
        var chunk = cPos.GetChunk();
        var block = chunk.CreateLocalBlock(this, mat, new BlockData());

        return this;
    }

    public Location SetData(BlockData data)
    {
        var oldMaterial =
            GetMaterial().ToString(); //Get current material, and later apply it to our new block change entry

        if (SaveManager.blockChanges.ContainsKey(this))
        {
            var blockChangeLine = SaveManager.blockChanges[this]; //Get old material from save data
            oldMaterial = blockChangeLine.Split('*')[0];
            SaveManager.blockChanges.Remove(this);
        }

        SaveManager.blockChanges.Add(this, oldMaterial + "*" + data.GetSaveString());

        var block = GetBlock();
        if (block != null) block.data = data;

        return this;
    }

    public Block GetBlock()
    {
        var cPos = new ChunkPosition(this);
        var chunk = cPos.GetChunk();
        if (chunk != null)
        {
            var block = chunk.GetLocalBlock(this);

            return block;
        }

        return null;
    }

    public void Tick()
    {
        var blocks = new List<Block>
        {
            (this + new Location(0, 0)).GetBlock(),
            (this + new Location(0, 1)).GetBlock(),
            (this + new Location(0, -1)).GetBlock(),
            (this + new Location(-1, 0)).GetBlock(),
            (this + new Location(1, 0)).GetBlock()
        };


        foreach (var blockToTick in blocks)
            if (blockToTick != null)
                blockToTick.ScheduleBlockTick();
    }

    public Material GetMaterial()
    {
        //Get Material from block changes
        if (SaveManager.blockChanges.ContainsKey(this))
        {
            var blockChangeLine = SaveManager.blockChanges[this];
            return (Material) Enum.Parse(typeof(Material), blockChangeLine.Split('*')[0]);
        }

        var cPos = new ChunkPosition(this);

        //Get material from loaded chunk
        if (cPos.IsChunkLoaded())
        {
            var block = cPos.GetChunk().GetLocalBlock(this);

            if (block == null)
                return Material.Air;
            return block.GetMaterial();
        }

        //Get material from saved chunk data
        if (!cPos.HasBeenSaved())
            return Material.Air;
        var chunkPath = WorldManager.world.getPath() + "\\region\\" + cPos.dimension + "\\" + cPos.chunkX;
        foreach (var line in File.ReadAllLines(chunkPath + "\\blocks"))
        {
            var lineLoc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                int.Parse(line.Split('*')[0].Split(',')[1]));
            var lineMaterial = (Material) Enum.Parse(typeof(Material), line.Split('*')[1]);
            var lineData = line.Split('*')[2];

            if (lineLoc.Equals(this)) return lineMaterial;
        }

        return Material.Air;
    }

    public BlockData GetData()
    {
        //Get data from block changes
        if (SaveManager.blockChanges.ContainsKey(this))
        {
            var blockChangeLine = SaveManager.blockChanges[this];
            return new BlockData(blockChangeLine.Split('*')[1]);
        }

        var cPos = new ChunkPosition(this);

        //Get data from loaded chunk
        if (cPos.IsChunkLoaded())
        {
            var block = cPos.GetChunk().GetLocalBlock(this);

            if (block == null)
                return new BlockData();
            
            return block.data;
        }

        //Get data from saved chunk data
        if (!cPos.HasBeenSaved())
            return new BlockData();
        var chunkPath = WorldManager.world.getPath() + "\\region\\" + cPos.dimension + "\\" + cPos.chunkX;
        foreach (var line in File.ReadAllLines(chunkPath + "\\blocks"))
        {
            var lineLoc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                int.Parse(line.Split('*')[0].Split(',')[1]));
            var lineMaterial = (Material) Enum.Parse(typeof(Material), line.Split('*')[1]);
            var lineData = new BlockData(line.Split('*')[2]);

            if (lineLoc.Equals(this)) return lineData;
        }

        return new BlockData();
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