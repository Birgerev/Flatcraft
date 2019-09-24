using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Block
{
    public static string default_texture = "block_stone";
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Wooden;
    
    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Cobblestone, 1);
    }

    public override void Tick()
    {
        base.Tick();
    }
}
