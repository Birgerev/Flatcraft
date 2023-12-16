public class Bed_Block : Block
{
    public override bool RotateX { get; } = true;
    public override bool Solid { get; set; } = false;

    public override float BreakTime { get; } = 0.65f;
    public override bool IsFlammable { get; } = true;

    public Location otherBlockLocation
    {
        get
        {
            //other block position is based on whether this block is a bottom or top piece
            int otherBlockXRelative = GetMaterial() == Material.Bed_Bottom ? -1 : 1;
            //if block is flipped, invert side of other block
            if (GetData().GetTag("rotated_x") == "true")
                otherBlockXRelative *= -1;

            return location + new Location(otherBlockXRelative, 0);
        }
    }

    public Material otherBlockMaterial => GetMaterial() == Material.Bed_Bottom ? Material.Bed_Top : Material.Bed_Bottom;

    public override void Interact(PlayerInstance player)
    {
        Player playerEntity = player.playerEntity.GetComponent<Player>();
        playerEntity.Sleep();
        playerEntity.bedLocation = location;

        base.Interact(player);
    }

    public override void Tick()
    {
        Block otherBlock = otherBlockLocation.GetBlock();
        if (otherBlock == null)
            Break(false);
        else if (otherBlock.GetMaterial() != otherBlockMaterial)
            Break(false);

        base.Tick();
    }

    protected override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Bed)};
    }
}