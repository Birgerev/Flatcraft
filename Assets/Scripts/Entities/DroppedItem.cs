﻿using Mirror;
using UnityEngine;

public class DroppedItem : Entity
{
    //Entity State
    private float cosIndex;
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] [SyncVar] public ItemStack item = new ItemStack();

    [ServerCallback]
    private void OnTriggerStay2D(Collider2D col)
    {
        if (dead || age < 0.5f || !isServer)
            return;

        if (col.GetComponent<DroppedItem>() != null)
            if (col.GetComponent<DroppedItem>().item.material == item.material)
            {
                if (age < col.GetComponent<DroppedItem>().age || col.GetComponent<DroppedItem>().dead)
                    return;

                item.amount += col.GetComponent<DroppedItem>().item.amount;
                col.GetComponent<DroppedItem>().Die();
                return;
            }

        if (col.GetComponent<Player>() != null)
            if (col.GetComponent<Player>().GetInventory().AddItem(item))
            {
                Sound.Play(Location, "random/pickup_pop", SoundType.Entities, 0.7f, 1.3f);
                Die();
            }
    }

    [Server]
    public override void Initialize()
    {
        if (item.material == Material.Air || item.amount <= 0)
            Die();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        base.Initialize();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //Despawn
        if (age > 60 * 5)
            Die();

        if (isOnGround)
            GetComponent<Rigidbody2D>().velocity *= 0.95f;
    }

    [Client]
    public override void ClientUpdate()
    {
        GetRenderer().sprite = item.GetSprite();

        //Bobbing
        GetRenderer().transform.localPosition = new Vector3(0, Mathf.Cos(cosIndex) * 0.1f);
        cosIndex += 2f * Time.deltaTime;

        base.ClientUpdate();
    }

    [Server]
    public override void Hit(float damage, Entity source)
    {
        //Disabling Hit
    }
}