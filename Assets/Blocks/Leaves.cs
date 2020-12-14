public class Leaves : Block
{
    public static string default_texture = "block_leaves";
    public override bool solid { get; set; } = false;
    public override float averageRandomTickDuration { get; } = 100;

    public override float breakTime { get; } = 0.3f;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void RandomTick()
    {
        TryDecay();

        base.RandomTick();
    }

    public void TryDecay()
    {
        var range = 4;
        var foundSupport = false;

        for (var x = -range; x < range; x++)
        for (var y = -range; y < range; y++)
            if (new Location(location.x + x, location.y + y).GetBlock() != null)
                if (new Location(location.x + x, location.y + y).GetBlock().GetType().IsSubclassOf(typeof(Log)))
                {
                    foundSupport = true;
                    break;
                }

        if (!foundSupport) Break();
    }
}