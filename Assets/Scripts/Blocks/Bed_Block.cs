public class Bed_Block : Block
{
    public override bool rotate_x { get; } = true;
    public override bool solid { get; set; } = false;

    public override float breakTime { get; } = 0.65f;
    public override bool isFlammable { get; } = true;

    public Location otherBlockLocation
    {
        get
        {
            //other block position is based on whether this block is a bottom or top piece
            var otherBlockXRelative = GetMaterial() == Material.Bed_Bottom ? -1 : 1;
            //if block is flipped, invert side of other block
            if (GetData().GetTag("rotated_x") == "true")
                otherBlockXRelative *= -1;

            return location + new Location(otherBlockXRelative, 0);
        }
    }

    public Material otherBlockMaterial => GetMaterial() == Material.Bed_Bottom ? Material.Bed_Top : Material.Bed_Bottom;

    public override void Interact()
    {
        Player.localEntity.Sleep();
        Player.localEntity.bedLocation = location;

        base.Interact();
    }

    public override void Tick()
    {
        var otherBlock = otherBlockLocation.GetBlock();
        if (otherBlock == null)
            Break(false);
        else if (otherBlock.GetMaterial() != otherBlockMaterial) 
            Break(false);

        base.Tick();
    }

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Bed, 1);
    }
}