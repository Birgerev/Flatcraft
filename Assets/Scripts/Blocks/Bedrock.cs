public class Bedrock : Block
{
    public override float BreakTime { get; } = 100000000;

    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Stone;
}