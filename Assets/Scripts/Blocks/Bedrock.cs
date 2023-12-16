public class Bedrock : Block
{
    public override float breakTime { get; } = 100000000;

    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Stone;
}