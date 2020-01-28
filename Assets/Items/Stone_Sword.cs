using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_Sword : Tool
{
    public override Tool_Type tool_type { get; } = Tool_Type.Sword;
    public override Tool_Level tool_level { get; } = Tool_Level.Stone;

    public static string default_texture = "item_stone_sword";
    public override int maxDurabulity { get; } = 131;
}
