using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bedrock : Block
{
    public static string default_texture = "block_bedrock";
    public override float breakTime { get; } = 100000000;
}
