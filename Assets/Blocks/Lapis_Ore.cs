using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lapis_Ore : Block
{
    public override string default_texture { get; } = "block_lapis_ore";
    public override float breakTime { get; } = 6;

    public override void Tick()
    {
        base.Tick();
    }
}
