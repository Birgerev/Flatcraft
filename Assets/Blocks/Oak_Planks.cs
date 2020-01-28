using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oak_Planks : Block
{
    public static string default_texture = "block_oak_planks";
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
}
