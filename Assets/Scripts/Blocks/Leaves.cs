using System;

public class Leaves : Block
{
    public override bool Solid { get; set; } = false;
    public override float AverageRandomTickDuration { get; } = 100;

    public override float BreakTime { get; } = 0.3f;
    public override bool IsFlammable { get; } = true;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Grass;

    public override void RandomTick()
    {
        TryDecay();

        base.RandomTick();
    }

    public void TryDecay()
    {
        int range = 4;
        bool foundSupport = false;

        for (int x = -range; x < range; x++)
        for (int y = -range; y < range; y++)
            if (new Location(location.x + x, location.y + y).GetBlock() != null)
                if (new Location(location.x + x, location.y + y).GetBlock().GetType().IsSubclassOf(typeof(Log)))
                {
                    foundSupport = true;
                    break;
                }

        if (!foundSupport)
            Break();
    }
}