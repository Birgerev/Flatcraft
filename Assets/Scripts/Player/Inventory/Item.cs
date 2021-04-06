﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Item
{
    public Dictionary<string, string> data = new Dictionary<string, string>();

    public virtual string texture { get; set; } = "";
    public virtual int maxDurabulity { get; } = -1;
    public virtual float entityDamage { get; } = 1;

    public void Interact(PlayerInstance player, Location loc, int mouseType, bool firstFrameDown)
    {
        if (mouseType == 0) 
            InteractLeft(player, loc, firstFrameDown);

        if (mouseType == 1) 
            InteractRight(player, loc, firstFrameDown);
    }

    public virtual void InteractLeft(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        var block = loc.GetBlock();

        if (block != null) 
            block.Hit(player, 1 / Player.interactionsPerPerSecond);
    }

    public virtual void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        var block = loc.GetBlock();

        if (firstFrameDown && block != null) 
            block.Interact(player);
    }

    public virtual Sprite getTexture()
    {
        return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public Material GetMateral()
    {
        return (Material) Enum.Parse(typeof(Material), GetType().Name);
    }
}