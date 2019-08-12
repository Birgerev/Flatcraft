using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaves : Block
{
    public override string texture { get; } = "block_leaves";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.3f;

    public override void Tick()
    {
        base.Tick();
    }
}
