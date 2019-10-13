using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController
{
    public LivingEntity instance;

    public EntityController(LivingEntity instance)
    {
        this.instance = instance;
    }

    public virtual void Tick()
    {
    }
}
