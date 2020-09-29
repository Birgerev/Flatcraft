using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Item
{
    public Dictionary<string, string> data = new Dictionary<string, string>();

    public string texture;
    public virtual int maxDurabulity { get; } = -1;


    private void Start()
    {
        texture = (string) GetType().GetField("default_texture", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);
    }

    public void Interact(Location loc, int mouseType, bool firstFrameDown)
    {
        if (mouseType == 0) InteractLeft(loc, firstFrameDown);

        if (mouseType == 1) InteractRight(loc, firstFrameDown);
    }

    public virtual void InteractLeft(Location loc, bool firstFrameDown)
    {
        var block = loc.GetBlock();

        if (block != null) block.Hit(1 / Player.interactionsPerPerSecond);
    }

    public virtual void InteractRight(Location loc, bool firstFrameDown)
    {
        var block = loc.GetBlock();

        if (firstFrameDown) block.Interact();
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