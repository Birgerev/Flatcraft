using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoe : Tool
{
    public override Tool_Type tool_type { get; } = Tool_Type.Hoe;
    
    protected override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        if (loc.GetMaterial() == Material.Grass || loc.GetMaterial() == Material.Dirt)
        {
            loc.SetMaterial(Material.Farmland_Dry).Tick();
            player.playerEntity.GetComponent<Player>().DoToolDurability();
        }

        base.InteractRight(player, loc, firstFrameDown);
    }
}
