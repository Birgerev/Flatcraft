using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : Block
{
    public static string default_texture = "block_cactus";
    public override bool playerCollide { get; } = false;
    public override bool requiresGround { get; } = true;
    public override float breakTime { get; } = 0.65f;

    public override void Tick()
    {
        base.Tick();
    }
}
