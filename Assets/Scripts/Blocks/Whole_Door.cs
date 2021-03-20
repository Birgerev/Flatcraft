using UnityEngine;

public class Whole_Door : Door
{
    public Location otherBlockLocation =>
        location + new Location(0, IsBottomDoorPart() ? 1 : -1);

    public virtual Material otherBlockMaterial => Material.Air;

    public override void BuildTick()
    {
        if(otherBlockLocation.GetMaterial() != otherBlockMaterial)
        {
            otherBlockLocation.SetMaterial(otherBlockMaterial);
            otherBlockLocation.GetBlock().ScheduleBlockBuildTick();
        }

        base.BuildTick();
    }

    public override void Interact(PlayerInstance player)
    {
        var otherDoor = (Door) otherBlockLocation.GetBlock();
        var open = !GetOpenState();

        otherDoor.SetOpenState(open);

        base.Interact(player);
    }
    public override void PlaySound(bool open)
    {
        if (IsBottomDoorPart())
            base.PlaySound(open);
    }

    public bool IsBottomDoorPart()
    {
        return GetMaterial() == Material.Wooden_Door_Bottom;
    }

    public override void Break()
    {
        if (otherBlockLocation.GetMaterial() == otherBlockMaterial) otherBlockLocation.SetMaterial(Material.Air);

        base.Break();
    }
}