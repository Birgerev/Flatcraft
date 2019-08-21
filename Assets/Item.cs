using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public string texture;
    public Dictionary<string, string> data = new Dictionary<string, string>();
    

    private void Start()
    {
        texture = (string)this.GetType().
            GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
    }

    public void Interact(Vector2Int position, int mouseType, bool firstFrameDown)
    {
        if (mouseType == 0)
        {
            InteractLeft(position, firstFrameDown);
        }

        if (mouseType == 1)
        {
            InteractRight(position, firstFrameDown);
        }
    }

    public virtual void InteractLeft(Vector2Int position, bool firstFrameDown)
    {
        Block block = Chunk.getBlock(position);
        
        if (block != null)
        {
            block.Hit(Time.deltaTime);
        }
    }

    public virtual void InteractRight(Vector2Int position, bool firstFrameDown)
    {
        Block block = Chunk.getBlock(position);
        
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
