public class Cactus : Block
{
    public override bool solid { get; set; } = false;
    public override bool requiresGround { get; } = true;
    public override float breakTime { get; } = 0.65f;
    public override bool isFlammable { get; } = true;
    public override float averageRandomTickDuration { get; } = 18 * 60;
    private const int MaxHeight = 3;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wool;

    public override void Tick()
    {
        if ((location + new Location(-1, 0)).GetMaterial() != Material.Air)
            Break();
        if ((location + new Location(1, 0)).GetMaterial() != Material.Air)
            Break();
        base.Tick();
    }

    public override void RandomTick()
    {
        base.RandomTick();
        
        Grow();
    }

    private void Grow()
    {
        Location aboveLoc = location + new Location(0, 1);

        if (aboveLoc.GetMaterial() == Material.Air && GetHeight() < MaxHeight)
        {
            aboveLoc.SetMaterial(GetMaterial());
        }
    }

    private int GetHeight()
    {
        int height = 0;
        Location loc = location;
        while (loc.GetMaterial() == GetMaterial())
        {
            height++;
            loc = loc + new Location(0, -1);
        }

        return height;
    }
}