using System;
using System.Collections.Generic;

[Serializable]
public struct BlockState
{
    public Material material;
    public BlockData data;

    public BlockState(Block block)
    {
        material = block.GetMaterial();
        data = block.location.GetData();
    }

    public BlockState(Material mat)
    {
        material = mat;
        data = new BlockData();
    }

    public BlockState(Material mat, BlockData data)
    {
        material = mat;
        this.data = data;
    }

    public BlockState(string saveString)
    {
        material = (Material) Enum.Parse(typeof(Material), saveString.Split('*')[0]);
        data = new BlockData(saveString.Split('*')[1]);
    }

    public string GetSaveString()
    {
        return material + "*" + data.GetSaveString();
    }
}