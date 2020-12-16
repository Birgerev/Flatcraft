public class Dead_Bush : Block
{
    public static string default_texture = "block_dead_bush_0";

    public override string[] alternative_textures { get; } =
        {"block_dead_bush_0", "block_dead_bush_1", "block_dead_bush_2"};

    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 0.3f;
    public override bool requiresGround { get; } = true;
    public override bool isFlammable { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;


    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Stick, 1);
    }
}