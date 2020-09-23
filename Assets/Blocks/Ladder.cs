using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Block
{
    public override bool playerCollide { get; } = false;
    public override bool triggerCollider { get; } = true;
    
    public override float breakTime { get; } = 3f;
    
    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    
    public static string default_texture = "block_ladder";
    
    public virtual void OnTriggerStay2D(Collider2D col)
    {
        if(col.GetComponent<Entity>() != null)
        {
            col.GetComponent<Entity>().isOnLadder = true;
        }
    }

    public virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null)
        {
            col.GetComponent<Entity>().isOnLadder = false;
        }
    }
}
