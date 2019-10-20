using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond_Axe : Tool
{
    public override Tool_Type tool_type { get; } = Tool_Type.Axe;
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;

    public static string default_texture = "item_diamond_axe";
}
