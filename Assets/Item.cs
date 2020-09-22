using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public virtual int maxDurabulity { get; } = -1;

    public string texture;
    public Dictionary<string, string> data = new Dictionary<string, string>();
    

    private void Start()
    {
        texture = (string)this.GetType().
            GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
    }

    public void Interact(Location loc, int mouseType, bool firstFrameDown)
    {
        if (mouseType == 0)
        {
            InteractLeft(loc, firstFrameDown);
        }

        if (mouseType == 1)
        {
            InteractRight(loc, firstFrameDown);
        }
    }

    public virtual void InteractLeft(Location loc, bool firstFrameDown)
    {
        Block block = loc.GetBlock();
        
        if (block != null)
        {
            block.Hit(1 / Player.blockHitsPerPerSecond);
        }
    }

    public virtual void InteractRight(Location loc, bool firstFrameDown)
    {
        Block block = loc.GetBlock();
        
        if (firstFrameDown)
        {
            block.Interact();
        }
    }

    public virtual Sprite getTexture()
    {
        return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public Material GetMateral()
    {
        return (Material)System.Enum.Parse(typeof(Material), this.GetType().Name);
    }
}
