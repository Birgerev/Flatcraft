using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : Entity
{
    public virtual float health { get; set; }
    public virtual float maxHealth { get; } = 20;

    private float lastGroundedYlevel;

    public override void Start()
    {
        base.Start();

        health = maxHealth;

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        fallDamageCheck();
    }

    private void fallDamageCheck()
    {
        if (isOnGround)
        {
            float damage = (lastGroundedYlevel - transform.position.y) - 3;
            if (damage >= 1)
                TakeFallDamage(damage);

            lastGroundedYlevel = transform.position.y;
        }
    }

    public virtual void TakeFallDamage(float damage)
    {
        Damage(damage);
    }

    public virtual void Damage(float damage)
    {
        health -= damage;

        if (health <= 0)
            Die();
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
