using UnityEngine;

public class Door : Block
{
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override bool solid { get; set; } = true;

    public virtual string open_texture { get; } = "";
    public virtual string closed_texture { get; } = "";

    public Location otherBlockLocation =>
        location + new Location(0, GetMaterial() == Material.Wooden_Door_Bottom ? 1 : -1);

    public Material otherBlockMaterial => GetMaterial() == Material.Wooden_Door_Bottom
        ? Material.Wooden_Door_Top
        : Material.Wooden_Door_Bottom;

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
        var otherDoor = (Door) otherBlockLocation.GetBlock();
        var open = !GetOpenState();


        SetOpenState(open);
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
        var open = data.GetData("open") == "true";

        return open;
    }

    public override void Break()
    {
        if (otherBlockLocation.GetMaterial() == otherBlockMaterial) otherBlockLocation.SetMaterial(Material.Air);

        base.Break();
    }

    public override void Tick()
    {
        var open = data.GetData("open") == "true";

        texture = open ? open_texture : closed_texture;
        solid = !open;

        Render();
        UpdateColliders();

        base.Tick();
    }

    public override void UpdateColliders()
    {
        var open = false;
        open = data.GetData("open") == "true";

        UpdateColliders(!open, false);
    }
}