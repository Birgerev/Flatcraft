using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : Entity
{
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)]
    public ItemStack item = new ItemStack();
    [EntityDataTag(false)]
    public bool canPickup = true;


    //Entity State
    private float cosIndex;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        getRenderer().sprite = item.getSprite();

        //Bobbing
        getRenderer().transform.localPosition = new Vector3(0, Mathf.Cos(cosIndex) *0.1f);
        cosIndex += 2f * Time.deltaTime;

        if (isOnGround)
            GetComponent<Rigidbody2D>().velocity *= 0.95f;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!canPickup)
            return;

        if (col.GetComponent<DroppedItem>() != null)
        {
            if (col.GetComponent<DroppedItem>().item.material == item.material)
            {
                if (transform.position.x < col.transform.position.x)
                    return;
                if (transform.position.x == col.transform.position.x)
                    if (transform.position.y < col.transform.position.y)
                        return;
                if (!col.GetComponent<DroppedItem>().canPickup)
                    return;

                item.amount += col.GetComponent<DroppedItem>().item.amount;
                col.GetComponent<DroppedItem>().canPickup = false;
                Destroy(col.gameObject);
                return;
            }
        }
        if (col.GetComponent<Player>() != null)
        {
            if (col.GetComponent<Player>().inventory.AddItem(item))
            {
                Destroy(gameObject);
            }
        }
    }
}
