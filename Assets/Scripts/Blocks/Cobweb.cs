public class Cobweb : Block
{
    public override float breakTime { get; } = 10f;
    public override bool solid { get; set; } = false;

    public override Tool_Type properToolType { get; } = Tool_Type.Sword;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}