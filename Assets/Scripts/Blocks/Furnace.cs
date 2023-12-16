public class Furnace : InventoryContainer
{
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Wooden;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Stone;

    public override Inventory NewInventory()
    {
        return FurnaceInventory.CreatePreset();
    }
}