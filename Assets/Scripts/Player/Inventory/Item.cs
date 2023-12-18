using System;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public Dictionary<string, string> data = new Dictionary<string, string>();

    public virtual int maxDurability { get; } = -1;
    public virtual float entityDamage { get; } = 1;

    public void Interact(PlayerInstance player, Location loc, int mouseType, bool firstFrameDown)
    {
        if (mouseType == 0)
            InteractLeft(player, loc, firstFrameDown);

        if (mouseType == 1)
            InteractRight(player, loc, firstFrameDown);
    }

    protected virtual void InteractLeft(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        Block block = loc.GetBlock();

        if (block != null)
            block.Hit(player, 1 / PlayerInteraction.InteractionsPerPerSecond);
    }

    protected virtual void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        Block block = loc.GetBlock();

        if (firstFrameDown && block != null)
            block.Interact(player);
    }
    
    public Material GetMaterial()
    {
        return (Material) Enum.Parse(typeof(Material), GetType().Name);
    }
}