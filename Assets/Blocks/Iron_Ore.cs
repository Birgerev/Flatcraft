using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iron_Ore : Block
{
    public override string default_texture { get; } = "block_iron_ore";
    public override float breakTime { get; } = 6;

    public override void Tick()
    {
        base.Tick();
    }
}
