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

        PlaySound(open);

        Tick();
        Autosave();
    }

    public virtual void PlaySound(bool open)
    {
        Sound.Play(location, "random/door/door_" + (open ? "open" : "close"), SoundType.Blocks, 0.8f, 1.2f);
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