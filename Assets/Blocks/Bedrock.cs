public class Bedrock : Block
{
    public static string default_texture = "block_bedrock";
    public override float breakTime { get; } = 100000000;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}