using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Block
{
    public override bool autosave { get; } = false;
    public override bool playerCollide { get; } = true;

    public virtual string open_texture { get; } = "";
    public virtual string closed_texture { get; } = "";

    public override void Interact()
    {
        ToggleOpen();
        
        base.Interact();
    }

    public virtual void ToggleOpen()
    {
        bool open = false;

        if (data.ContainsKey("open"))
        {
            open = bool.Parse(data["open"]);
        }

        data["open"] = !open + "";
    }

    public override void Tick()
    {
        bool open = false;

        if (data.ContainsKey("open"))
        {
            open = bool.Parse(data["open"]);
        }

        texture = open ? open_texture : closed_texture;

        Render();

        base.Tick();
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
