using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Wolf : PassiveEntity
{
    //Entity Properties
    public override float maxHealth { get; } = 8;
    protected override float walkSpeed { get; } = 6f;

    public override EntityController GetController()
    {
        return new WolfController(this);
    }
    
}