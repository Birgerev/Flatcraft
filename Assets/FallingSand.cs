using System;
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

    public void Awake()
    {
        GetComponent<Rigidbody2D>().simulated = false;    //wait till the block that spawned the sand dissapears to begin simulating physics
    }
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

        if (!GetComponent<Rigidbody2D>().simulated)    //once the block that spawned the fallingSand is gone, begin simulating physics
            if(!Physics2D.OverlapBox(location.getPosition(), new Vector2(0.2f, 0.2f), 0))
            {
                GetComponent<Rigidbody2D>().simulated = true;
            }else print(Physics2D.OverlapBox(location.getPosition(), new Vector2(0.2f, 0.2f), 0).gameObject.name);
        
        
        
        if ((isOnGround || (isInLiquid && GetComponent<Rigidbody2D>().velocity.y == 0)) && age > 1f && !dead)
        {
            if (Chunk.getBlock(location) == null)
                Chunk.setBlock(location, material);
            else
            {
                new ItemStack(material, 1).Drop(location);
            }
            
            Die();
        }
    }

    public override void Hit(float damage)
    {
        //Disabling Hit
    }
}
