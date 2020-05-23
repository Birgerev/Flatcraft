using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableItem : Item
{
    public virtual Material blockMaterial { get; } = Material.Air;
}
