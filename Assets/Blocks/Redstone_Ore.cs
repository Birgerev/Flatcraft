using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redstone_Ore : Block
{
    public static string default_texture = "block_redstone_ore";
    public override float breakTime { get; } = 6;

    public override void Tick()
    {
        base.Tick();
    }
}
