using System;

public class Leaves : Block
{
    public override string texture { get; set; } = "block_leaves";
    public override bool solid { get; set; } = false;
    public override float averageRandomTickDuration { get; } = 100;

    public override float breakTime { get; } = 0.3f;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

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