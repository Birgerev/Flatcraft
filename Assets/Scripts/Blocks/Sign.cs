using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Block
{
    public override string texture { get; set; } = "block_sign";
    public override bool solid { get; set; } = false;
    
    public override bool requiresGround { get; } = true;
    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    
    public override void Interact(PlayerInstance player)
    {
        base.Interact(player);
        
        SignEditMenu.Create(player, location);
    }

    public override void ServerInitialize()
    {
        base.ServerInitialize();

        //Create inventory if none has been assigned
        if (!GetData().HasTag("line1"))
        {
            BlockData newData = GetData()
                .SetTag("line0", "").SetTag("line1", "").SetTag("line2", "").SetTag("line3", "");
            
            location.SetData(newData);
        }
    }
    
    /*public Location GetRelativeAttachmentLocation()
    {
        Location[] possibleRelativePoints = 
            new Location[] {new Location(0, -1), new Location(-1, 0), new Location(1, 0)};

        foreach (Location relativeLoc in possibleRelativePoints)
        {
            Location loc = location + relativeLoc;
            Block block = loc.GetBlock();
            if (block != null && block.solid)
                return relativeLoc;
        }

        return default;
    }*/
}
