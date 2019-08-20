using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : Entity
{
    public ItemStack item;

    public SpriteRenderer sprite;


    private float cosIndex;

    // Start is called before the first frame update
    public override void Start()
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    public override void Update()
    {
        sprite.sprite = item.getSprite();

        //Bobbing
        sprite.transform.localPosition = new Vector3(0, Mathf.Cos(cosIndex) *0.1f);
        cosIndex += 2f * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<DroppedItem>() != null)
        {
            if (col.GetComponent<DroppedItem>().item.material == item.material)
            {
                if (transform.position.x > col.transform.position.x)
                {
                    item.amount += col.GetComponent<DroppedItem>().item.amount;
                    Destroy(col.gameObject);
                    return;
                }
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
