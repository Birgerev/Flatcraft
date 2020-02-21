using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
    public virtual Tool_Type tool_type { get; } = Tool_Type.None;
    public virtual Tool_Level tool_level { get; } = Tool_Level.None;

    public override void InteractLeft(Location loc, bool firstFrameDown)
    {
        Block block = Chunk.getBlock(loc);

        if (block != null)
        {
            block.Hit(Time.deltaTime, tool_type, tool_level);
        }
    }
}
public enum Tool_Type
{
    None,
    Pickaxe,
    Axe,
    Shovel,
    Hoe,
    Sword
}
public enum Tool_Level
{
    None = 1,
    Wooden = 2,
    Stone = 3,
    Gold = 4,
    Iron = 5,
    Diamond = 6
}