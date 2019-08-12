using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bedrock : Block
{
    public override string texture { get; } = "block_bedrock";
    public override float breakTime { get; } = 100000000;

    public override void Tick()
    {
        base.Tick();
    }
}
