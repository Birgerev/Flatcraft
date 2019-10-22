using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : EntityController
{
    public bool isWalking = false;
    public bool walkingRight = false;

    public static float idleStateChangeChance = 0.2f;
    public static float walkingStateChangeChance = 0.4f;

    public AnimalController(LivingEntity instance) : base(instance)
    {

    }

    public override void Tick()
    {
        base.Tick();

        Block block = Chunk.getBlock(Vector2Int.RoundToInt(instance.transform.position));
        Block blockInFront = Chunk.getBlock(Vector2Int.RoundToInt(instance.transform.position + new Vector3(walkingRight ? 1 : -1, 0)));
        if (isWalking)
        {
            instance.Walk(walkingRight ? 1 : -1);

            //Jump when there is a block in front of entity
            if (blockInFront != null && blockInFront.playerCollide)
            {
                instance.Jump();
            }
        }

        //Swim in water
        if (block != null && block.GetMaterial() == Material.Water)
            instance.Jump();


        //Change States
        System.Random r = new System.Random((instance.transform.position.ToString() + " "+instance.age).GetHashCode());
        
        if(r.NextDouble() < (isWalking ? walkingStateChangeChance : idleStateChangeChance) / (1.0f / Time.deltaTime))
        {
            isWalking = !isWalking;
            walkingRight = r.Next(0, 2) == 0;
        }
    }
}
