using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : Block
{
    public static string default_texture = "block_torch";
    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.3f;
    public override bool requiresGround { get; } = true;
    
    public override int glowingLevel { get; } = 14;
    public override float flickerLevel { get; } = 1f;

    public override void Tick()
    {
        base.Tick();
    }
}
