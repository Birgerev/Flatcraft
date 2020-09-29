public class Oak_Plank_Stairs : Stairs
{
    public static string default_texture = "block_oak_plank_stairs";
    public override float breakTime { get; } = 3f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;
}