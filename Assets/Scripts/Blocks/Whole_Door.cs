public class Whole_Door : Door
{
    public Location otherBlockLocation =>
        location + new Location(0, IsBottomDoorPart() ? 1 : -1);

    public virtual Material otherBlockMaterial => Material.Air;

    public override void BuildTick()
    {
        if (otherBlockLocation.GetMaterial() != otherBlockMaterial)
        {
            otherBlockLocation.SetMaterial(otherBlockMaterial);
            otherBlockLocation.GetBlock().BuildTick();
        }

        base.BuildTick();
    }

    public override void Interact(PlayerInstance player)
    {
        base.Interact(player);
        
        Door otherDoor = (Door) otherBlockLocation.GetBlock();

        otherDoor?.SetOpenState(GetOpenState());
    }

    public override void PlaySound(bool open)
    {
        if (IsBottomDoorPart())
            base.PlaySound(open);
    }

    public bool IsBottomDoorPart()
    {
        //TODO this relies on being oak
        return GetMaterial() == Material.Oak_Door_Bottom;
    }

    public override void Break()
    {
        if (otherBlockLocation.GetMaterial() == otherBlockMaterial)
            otherBlockLocation.SetMaterial(Material.Air);

        base.Break();
    }
}