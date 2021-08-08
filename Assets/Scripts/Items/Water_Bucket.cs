using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Bucket : Filled_Bucket
{
    public override string texture { get; set; } = "item_water_bucket";
    public override Material bucketBlock { get; set; } = Material.Water;
}
