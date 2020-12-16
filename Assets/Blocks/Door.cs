using UnityEngine;

public class Door : Block
{
    public override bool solid { get; set; } = true;

    public virtual string open_texture { get; } = "";
    public virtual string closed_texture { get; } = "";

    public override void Initialize()
    {
        base.Initialize();

        Tick();
    }

    public override void Interact()
    {
        var open = !GetOpenState();

        SetOpenState(open);

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

    public override void Tick()
    {
        var open = data.GetData("open") == "true";

        texture = open ? open_texture : closed_texture;
        solid = !open;

        Render();
        UpdateColliders();

        base.Tick();
    }
}