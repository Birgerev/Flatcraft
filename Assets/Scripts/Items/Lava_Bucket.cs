using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava_Bucket : Filled_Bucket
{
    public override Material bucketBlock { get; set; } = Material.Lava;
}
