public class Obsidian : Block
{
    public override float breakTime { get; } = 250;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Diamond;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Stone;

    public override void Tick()
    {
        CheckPortalActivation();

        base.Tick();
    }

    public override void Break(bool drop = true)
    {
        int positiveY = 1;
        while ((location + new Location(0, positiveY)).GetMaterial() == Material.Portal_Frame)
        {
            (location + new Location(0, positiveY)).GetBlock().Break();
            positiveY++;
        }

        int negativeY = -1;
        while ((location + new Location(0, negativeY)).GetMaterial() == Material.Portal_Frame)
        {
            (location + new Location(0, negativeY)).GetBlock().Break();
            negativeY--;
        }

        base.Break(drop);
    }

    public void CheckPortalActivation()
    {
        if ((location + new Location(0, 1)).GetMaterial() == Material.Fire)
            TryActivatePortal();
    }

    public void TryActivatePortal()
    {
        int y = location.y + 1;
        while (true)
        {
            Location loc = new Location(location.x, y, location.dimension);
            Material mat = loc.GetMaterial();

            if (mat == Material.Obsidian)
                break;
            if (y >= Chunk.Height)
                return;

            y++;
        }

        for (int buildY = location.y + 1; buildY < y; buildY++)
        {
            Location loc = new Location(location.x, buildY, location.dimension);

            loc.SetMaterial(Material.Portal_Frame).Tick();
        }
    }
}