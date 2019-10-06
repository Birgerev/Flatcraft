using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logged_Leaves : Block
{
    public override bool playerCollide { get; } = false;

    public static string default_texture = "block_logged_leaves";
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Oak_Log, 1);
    }

    public override void Tick()
    {
        base.Tick();
    }
}
