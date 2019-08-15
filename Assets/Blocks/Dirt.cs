using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : Block
{
    public override string default_texture { get; } = "block_dirt";
    public override float breakTime { get; } = 0.75f;

    public override void Tick()
    {
        base.Tick();
    }
}
