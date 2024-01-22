using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oak_Sign : Block
{
    public override bool IsSolid { get; set; } = false;
    
    public override bool RequiresGround { get; } = true;
    public override float BreakTime { get; } = 6;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;
    
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
