using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plank_Stairs : Stairs
{
    public static string default_texture = "block_plank_stairs";
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
}
