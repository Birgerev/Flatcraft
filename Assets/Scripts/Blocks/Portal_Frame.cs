public class Portal_Frame : Block
{
    public override string texture { get; set; } = "block_portal_frame";
    public override bool solid { get; set; } = false;
    public override bool trigger { get; set; } = true;

    public override float breakTime { get; } = 9999999999f;
    public override int glowLevel { get; } = 11;

    public override Tool_Type propperToolType { get; } = Tool_Type.None;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Glass;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }
}