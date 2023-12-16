public class Crafting_Table : InventoryContainer
{
    public override bool Solid { get; set; } = false;
    public override float BreakTime { get; } = 3;

    public override Tool_Type ProperToolType { get; } = Tool_Type.Axe;
    public override BlockSoundType BlockSoundType { get; } = BlockSoundType.Wood;

    public override Inventory NewInventory()
    {
        return CraftingInventory.CreatePreset();
    }
}