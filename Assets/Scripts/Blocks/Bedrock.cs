public class Bedrock : Block
{
    public override float breakTime { get; } = 100000000;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}