using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Liquid
{
    public override string default_texture { get; } = "block_water";
    public override int max_liquid_level { get; } = 8;
}
