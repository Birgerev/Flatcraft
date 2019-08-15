using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : Block
{
    public override string default_texture { get; } = "";
    public override float breakTime { get; } = 0.75f;

    public override void FirstTick()
    {
        base.FirstTick();
    }

    public override void Tick()
    {
        base.Tick();

        //If not covered by a block
        if(Chunk.getBlock(getPosition() + new Vector2Int(0, 1)) != null)
        {
        }
    }

    public override void Hit(float time)
    {

    }
}
