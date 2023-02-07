public class Coal_Ore : Block
{
    public override string[] randomTextures { get; } = {"block_coal_ore", "block_coal_ore_1"};

    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Wooden;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Coal, 1);
    }
}