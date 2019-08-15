using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oak_Log : Block
{
    public override bool playerCollide { get; } = false;

    public override string default_texture { get; } = "block_oak_log";
    public override float breakTime { get; } = 3f;

    public override void Tick()
    {
        base.Tick();
    }
}
