using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : Liquid
{
    public override string default_texture { get; } = "block_lava";
    public override int max_liquid_level { get; } = 4;
}
