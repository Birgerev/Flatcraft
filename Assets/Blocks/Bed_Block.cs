using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed_Block : Block
{
    public override bool rotate_x { get; } = true;
    public override bool playerCollide { get; } = false;

    public override float breakTime { get; } = 0.65f;

    public Location otherBlockLocation
    {
        get
        { 
            //other block position is based on whether this block is a bottom or top piece
            int otherBlockXRelative = (GetMaterial() == Material.Bed_Bottom) ? -1 : 1;
            //if block is flipped, invert side of other block
            if (data.ContainsKey("rotated_x"))
                if (data["rotated_x"] == "true")
                    otherBlockXRelative *= -1;

            return location + new Location(otherBlockXRelative, 0);
        }
    }
    public Material otherBlockMaterial
    {
        get
        {
            return (GetMaterial() == Material.Bed_Bottom) ? Material.Bed_Top : Material.Bed_Bottom;
        }
    }

    public override void Interact()
    {
        Player.localInstance.Sleep();
        Player.localInstance.spawnLocation = location;

        base.Interact();
    }

    public override void Tick()
    {
        if (age > 0) { 
            Block otherBlock = otherBlockLocation.GetBlock();
            if (otherBlock == null)
            {
                Break(false);
            }
            else if (otherBlock.GetMaterial() != otherBlockMaterial)
            {
                Break(false);
            }
        }

        base.Tick();
    }
    
    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Bed, 1);
    }
}
