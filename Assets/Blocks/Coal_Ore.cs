﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coal_Ore : Block
{
    public static string default_texture = "block_coal_ore";
    public override float breakTime { get; } = 6;

    public override void Tick()
    {
        base.Tick();
    }
}
