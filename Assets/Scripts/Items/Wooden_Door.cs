using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wooden_Door : PlaceableItem
{
    public override string texture { get; set; } = "item_wooden_door";
    public override Material blockMaterial { get; } = Material.Wooden_Door_Bottom;
}
