using UnityEngine;

public class Whole_Door : Door
{
    public Location otherBlockLocation =>
        location + new Location(0, GetMaterial() == Material.Wooden_Door_Bottom ? 1 : -1);

    public virtual Material otherBlockMaterial => Material.Air;
    
    public override void Interact()
    {
        var otherDoor = (Door) otherBlockLocation.GetBlock();
        var open = !GetOpenState();

        otherDoor.SetOpenState(open);

        base.Interact();
    }

    public override void Break()
    {
        if (otherBlockLocation.GetMaterial() == otherBlockMaterial) otherBlockLocation.SetMaterial(Material.Air);

        base.Break();
    }
}