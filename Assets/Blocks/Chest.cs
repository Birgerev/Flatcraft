using System;

public class Chest : InventoryContainer
{
    public static string default_texture = "block_chest";
    public override bool solid { get; set; } = false;
    public override float breakTime { get; } = 6;

    public override Tool_Type propperToolType { get; } = Tool_Type.Axe;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Wood;

    public override Type inventoryType { get; } = typeof(Inventory);

    public override void Tick()
    {
        if (inventory == null)
        {
            base.Tick();
            return;
        }

        inventory.name = "Chest";
        base.Tick();
    }

    private Inventory getInventory()
    {
        return inventory;
    }
}