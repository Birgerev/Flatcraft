public class Crafting_Table : InventoryContainer
{
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 3;

    public override Tool_Type properToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override Inventory NewInventory()
    {
        return CraftingInventory.CreatePreset();
    }
}