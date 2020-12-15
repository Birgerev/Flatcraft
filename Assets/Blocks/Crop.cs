using System;

public class Crop : Block
{
    public override bool solid { get; set; } = false;

    public override float breakTime { get; } = 0.05f;
    public override bool isFlammable { get; } = true;
    public override float averageRandomTickDuration { get; } = 100;
    public override bool autosave { get; } = true;
    public override bool requiresGround { get; } = true;

    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Grass;

    public virtual string[] crop_textures { get; } = { };
    public virtual Material seed { get; } = Material.Air;
    public virtual Material result { get; } = Material.Air;


    public override void Tick()
    {
        texture = crop_textures[GetStage()];
        Render();

        base.Tick();
    }

    public override void RandomTick()
    {
        Grow();
        CheckFarmland();

        base.RandomTick();
    }

    public void CheckFarmland()
    {
        var materialBeneath = (location - new Location(0, 1)).GetMaterial();

        if (materialBeneath != Material.Farmland_Wet && materialBeneath != Material.Farmland_Dry) Break();
    }

    public void Grow()
    {
        if (GetStage() >= GetAmountOfStages() - 1)
            return;

        data.SetData("crop_stage", (GetStage() + 1).ToString());
    }

    public int GetAmountOfStages()
    {
        return crop_textures.Length;
    }

    public int GetStage()
    {
        if (!data.HasData("crop_stage"))
            data.SetData("crop_stage", "0");

        return int.Parse(data.GetData("crop_stage"));
    }

    public override void Drop()
    {
        new ItemStack(seed, new Random().Next(0, 3)).Drop(location);

        if (GetStage() == GetAmountOfStages() - 1)
            new ItemStack(result, 1).Drop(location);
    }
}