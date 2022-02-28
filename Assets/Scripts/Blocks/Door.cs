﻿public class Door : Block
{
    public override bool solid { get; set; } = true;

    public virtual string open_texture { get; } = "";
    public virtual string closed_texture { get; } = "";

    public override void ServerInitialize()
    {
        base.ServerInitialize();

        Tick();
    }

    public override void Initialize()
    {
        bool open = GetData().GetTag("open") == "true";
        solid = !open;

        UpdateColliders();

        base.Initialize();
    }

    public override void Interact(PlayerInstance player)
    {
        bool open = !GetOpenState();

        SetOpenState(open);

        base.Interact(player);
    }

    public override string GetTexture()
    {
        bool open = GetData().GetTag("open") == "true";

        return open ? open_texture : closed_texture;
    }

    public void SetOpenState(bool open)
    {
        SetData(GetData().SetTag("open", open ? "true" : "false"));

        PlaySound(open);
    }

    public virtual void PlaySound(bool open)
    {
        Sound.Play(location, "random/door/door_" + (open ? "open" : "close"), SoundType.Block, 0.8f, 1.2f);
    }

    public bool GetOpenState()
    {
        bool open = GetData().GetTag("open") == "true";

        return open;
    }
}