using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wooden_Door : PlaceableItem
{
    public static string default_texture = "item_wooden_door";
    public override Material blockMaterial { get; } = Material.Wooden_Door_Bottom;
}
