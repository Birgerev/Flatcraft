using System;

public class Crop : Block
{
    public override bool solid { get; set; } = false;

    public override float breakTime { get; } = 0.01f;
    public override bool isFlammable { get; } = true;
    public override float averageRandomTickDuration { get; } = 100;

    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Grass;

    public virtual string[] crop_textures { get; } = { };
    public virtual Material seed { get; } = Material.Air;
    public virtual Material result { get; } = Material.Air;

    public override void BuildTick()
    {
        if (!GetData().HasTag("crop_stage"))
            SetData(GetData().SetTag("crop_stage", "0"));
    }

    protected override string GetTexture()
    {
        return crop_textures[GetStage()];
    }
    
    public override void Tick()
    {
        CheckFarmland();

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
        Material materialBeneath = (location - new Location(0, 1)).GetMaterial();

        if (materialBeneath != Material.Farmland_Wet && materialBeneath != Material.Farmland_Dry)
            Break();
    }

    public virtual void Grow()
    {
        if (GetStage() >= GetAmountOfStages() - 1)
            return;

        location.SetData(GetData().SetTag("crop_stage", (GetStage() + 1).ToString())).Tick();
    }

    public int GetAmountOfStages()
    {
        return crop_textures.Length;
    }

    public int GetStage()
    {
        int stage = -1;
        string stageTag = GetData().GetTag("crop_stage");
        int.TryParse(stageTag, out stage);
        return stage;
    }

    protected override ItemStack[] GetDrops()
    {
        if (GetStage() == GetAmountOfStages() - 1)
        {
            return new[] { 
                new ItemStack(seed, new Random().Next(1, 3 + 1)), 
                new ItemStack(result)};
        }

        return new[] { new ItemStack(seed)};
    }
}