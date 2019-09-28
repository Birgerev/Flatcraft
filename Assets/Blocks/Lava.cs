using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : Liquid
{
    public static string default_texture = "block_lava";

    public override int max_liquid_level { get; } = 4;
    public override int glowingLevel { get; } = 14;
    public override float flickerLevel { get; } = 0f;
}
