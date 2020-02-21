using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSand : Entity
{
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)]
    public Material material = Material.Sand;

    //Entity State

    public override void Start()
    {
        base.Start();

        if (material == Material.Air)
            Die();

        getRenderer().sprite = new ItemStack(material).getSprite();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        if ((isOnGround || (isInLiquid && GetComponent<Rigidbody2D>().velocity.y == 0)) && age > 1f && !dead)
        {
            if (Chunk.getBlock(location) == null)
                Chunk.setBlock(location, material);
            else
            {
                new ItemStack(material, 1).Drop(location);
            }

            Die();
            return;
        }
    }

    public override void Hit(float damage)
    {
        //Disabling Hit
    }
}
