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
        if (isOnGround && age > 1f && !dead)
        {
            //Rounding Errors!
            print("falloing sand, rounding " + transform.position + " to " + Vector2Int.RoundToInt((Vector2)transform.position) + " at time " + Time.frameCount);
            if (Chunk.getBlock(Vector2Int.RoundToInt((Vector2)transform.position)) == null)
                Chunk.setBlock(Vector2Int.RoundToInt((Vector2)transform.position), material);
            else
            {
                new ItemStack(material, 1).Drop(Vector2Int.RoundToInt((Vector2)transform.position));
            }

            Die();
            return;
        }

        base.Update();
    }

    public override void Hit(float damage)
    {
        //Disabling Hit
    }
}
