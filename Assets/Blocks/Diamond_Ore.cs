using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond_Ore : Block
{
    public static string default_texture = "block_diamond_ore";
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level propperToolLevel { get; } = Tool_Level.Iron;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Diamond, 1);
    }

    public override void Tick()
    {
        base.Tick();
    }
}
