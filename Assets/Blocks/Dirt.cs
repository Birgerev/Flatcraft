using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : Block
{
    public static string default_texture = "block_dirt";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;

    public override void Tick()
    {
        base.Tick();
    }
}
