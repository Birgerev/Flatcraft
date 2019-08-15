using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Liquid
{
    public static string default_texture = "block_water";
    public override int max_liquid_level { get; } = 8;
}
