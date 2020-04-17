using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Block
{
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    
    public override bool autosave { get; } = true;
    public override bool playerCollide { get; } = true;

    public virtual string open_texture { get; } = "";
    public virtual string closed_texture { get; } = "";

    public Location otherBlockLocation
    {
        get
        {
            return location + new Location(0, ((GetMaterial() == Material.Wooden_Door_Bottom) ? 1 : -1));
        }
    }
    
    public Material otherBlockMaterial
    {
        get
        {
            return (GetMaterial() == Material.Wooden_Door_Bottom) ? Material.Wooden_Door_Top : Material.Wooden_Door_Bottom;
        }
    }
    
    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Wooden_Door_Bottom, 1);
    }
    
    public override void Interact()
    {
        Door otherDoor = (Door)Chunk.getBlock(otherBlockLocation);
        bool open = !GetOpenState();
        
        
        this.SetOpenState(open);
        otherDoor.SetOpenState(open);

        base.Interact();
    }

    public void SetOpenState(bool open)
    {
        data["open"] = open + "";

        Tick(true);
    }

    public bool GetOpenState()
    {
        bool open = false;
        
        if (data.ContainsKey("open"))
        {
            open = bool.Parse(data["open"]);
        }
        
        return open;
    }

    public override void Break()
    {
        Block otherBlock = Chunk.getBlock(otherBlockLocation);
        if (otherBlock != null && otherBlock.GetMaterial() == otherBlockMaterial)
        {
            Chunk.setBlock(otherBlockLocation, Material.Air);
        }

        base.Break();
    }

    public override void Tick(bool spread)
    {
        bool open = false;

        if (data.ContainsKey("open"))
        {
            open = bool.Parse(data["open"]);
        }

        texture = open ? open_texture : closed_texture;

        Render();

        base.Tick(spread);
    }

    public override void UpdateColliders()
    {
        bool open = false;

        if (data.ContainsKey("open"))
        {
            open = bool.Parse(data["open"]);
        }

        GetComponent<Collider2D>().enabled = !open;
        GetComponent<Collider2D>().isTrigger = false;
    }
}
