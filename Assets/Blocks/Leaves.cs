using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaves : Block
{
    public static string default_texture = "block_leaves";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.3f;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void Tick()
    {
        base.Tick();
    }
}
