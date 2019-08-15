using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool isOnGround;
    public float age;

    public virtual void Start()
    {

    }

    public virtual void Update()
    {
        isOnGround = (getVelocity().y < 0.05f && getVelocity().y > -0.05f);
        age += Time.deltaTime;
    }

    public virtual void setVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public virtual Vector2 getVelocity()
    {
        return GetComponent<Rigidbody2D>().velocity;
    }
}
