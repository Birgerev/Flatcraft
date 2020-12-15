public class Cactus : Block
{
    public static string default_texture = "block_cactus";
    public override bool solid { get; set; } = false;
    public override bool requiresGround { get; } = true;
    public override float breakTime { get; } = 0.65f;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wool;

    public override void Tick()
    {
        if ((location + new Location(-1, 0)).GetMaterial() != Material.Air)
            Break();
        if ((location + new Location(1, 0)).GetMaterial() != Material.Air)
            Break();
        base.Tick();
    }
}