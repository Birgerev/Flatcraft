﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : Liquid
{
    public static string default_texture = "block_lava";
    public override int max_liquid_level { get; } = 4;
}
