using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Red_Flower : Block
{
    public static string default_texture = "block_red_flower";
    public override string[] alternative_textures { get; } = { "block_red_flower_0", "block_red_flower_1", "block_red_flower_2" };

    public override bool playerCollide { get; } = false;
    public override float breakTime { get; } = 0.3f;
    public override bool requiresGround { get; } = true;
}
