using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coal_Ore : Block
{
    public override string texture { get; } = "block_coal_ore";
    public override float breakTime { get; } = 6;

    public override void Tick()
    {
        base.Tick();
    }
}
