using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Block
{
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
    
    public override bool playerCollide { get; } = true;

    public virtual string open_texture { get; } = "";
    public virtual string closed_texture { get; } = "";

    public Location otherBlockLocation => location + new Location(0, (GetMaterial() == Material.Wooden_Door_Bottom) ? 1 : -1);

    public Material otherBlockMaterial => (GetMaterial() == Material.Wooden_Door_Bottom) ? Material.Wooden_Door_Top : Material.Wooden_Door_Bottom;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Wooden_Door_Bottom, 1);
    }

    public override void Initialize()
    {
        base.Initialize();
        
        Tick();
    }

    public override void Interact()
    {
        Door otherDoor = (Door)otherBlockLocation.GetBlock();
        bool open = !GetOpenState();
        
        
        this.SetOpenState(open);
        otherDoor.SetOpenState(open);

        base.Interact();
    }

    public void SetOpenState(bool open)
    {
        data.SetData("open", open ? "true" : "false");

        Tick();
        Autosave();
    }

    public bool GetOpenState()
    {
        bool open = (data.GetData("open") == "true");
        
        return open;
    }

    public override void Break()
    {
        if (otherBlockLocation.GetMaterial() == otherBlockMaterial)
        {
            otherBlockLocation.SetMaterial(Material.Air);
        }

        base.Break();
    }

    public override void Tick()
    {
        bool open = (data.GetData("open") == "true");

        texture = open ? open_texture : closed_texture;

        Render();

        base.Tick();
    }

    public override void UpdateColliders()
    {
        bool open = false;
        
        open = (data.GetData("open") == "true");

        GetComponent<Collider2D>().enabled = !open;
        GetComponent<Collider2D>().isTrigger = false;
    }
}
