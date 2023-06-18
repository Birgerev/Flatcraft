using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Location
{
    public int x;
    public int y;
    public Dimension dimension;


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

    public static Location LocationByPosition(Vector3 pos)
    {
        Dimension dimension = Dimension.Overworld;

        foreach (Dimension dim in Enum.GetValues(typeof(Dimension)))
            if (pos.y >= (int) dim * Chunk.DimensionSeparationSpace)
                dimension = dim;

        return new Location(Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y) - (int) dimension * Chunk.DimensionSeparationSpace,
            dimension);
    }

    public Vector2 GetPosition()
    {
        return new Vector2(x, y + (int) dimension * Chunk.DimensionSeparationSpace);
    }

    public Material GetMaterial()
    {
        BlockState state = GetState();
        
        return state.material;
    }

    public BlockData GetData()
    {
        BlockState state = GetState();
        BlockData data = new BlockData(state.data);

        return data;
    }

    public Location SetMaterial(Material mat)
    {
        BlockState state = GetState();
        state.material = mat;
        //state.data = new BlockData();

        SetState(state);

        return this;
    }

    public Location SetData(BlockData data)
    {
        BlockState state = GetState();
        state.data = data;

        SetState(state);

        return this;
    }

    public Location SetState(BlockState state)
    {
        if (state.material == Material.Air)
            state.data = new BlockData("");

        SaveState(state);

        Chunk chunk = new ChunkPosition(this).GetChunk();
        if (chunk != null)
        {
            chunk.SetBlockState(this, state);
            chunk.LocalBlockChange(this, state);
            chunk.BlockChange(this, state);
        }

        return this;
    }

    public Location SetStateNoBlockChange(BlockState state)
    {
        SaveState(state);

        Chunk chunk = new ChunkPosition(this).GetChunk();
        if (chunk != null)
            chunk.SetBlockState(this, state);

        return this;
    }

    public Location SaveState(BlockState state)
    {
        //TODO ensure latest change gets saved
        //if (SaveManager.unsavedBlockChanges.ContainsKey(this))
        //    SaveManager.unsavedBlockChanges.Remove(this);

        SaveManager.unsavedBlockChanges.Add(new BlockChange(this, state));

        return this;
    }

    public BlockState GetState()
    {
        Chunk chunk = new ChunkPosition(this).GetChunk();

        if (chunk != null)
        {
            BlockState state = chunk.GetBlockState(this);

            return state;
        }

        return new BlockState(Material.Air);
    }

    public Block GetBlock()
    {
        Chunk chunk = new ChunkPosition(this).GetChunk();

        if (chunk != null)
        {
            Block block = chunk.GetLocalBlock(this);

            return block;
        }

        return null;
    }

    public void Tick()
    {
        List<Block> blocks = new List<Block>
        {
            this.GetBlock(), (this + new Location(0, 1)).GetBlock(), (this + new Location(0, -1)).GetBlock()
            , (this + new Location(-1, 0)).GetBlock(), (this + new Location(1, 0)).GetBlock()
        };


        foreach (Block blockToTick in blocks)
            if (blockToTick != null)
                blockToTick.Tick();
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