using UnityEngine;

public class DroppedItem : Entity
{
    [EntityDataTag(false)] public bool canPickup = true;


    //Entity State
    private float cosIndex;
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] public ItemStack item = new ItemStack();

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        if (item.material == Material.Air || item.amount <= 0)
            Die();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        GetRenderer().sprite = item.GetSprite();

        //Bobbing
        GetRenderer().transform.localPosition = new Vector3(0, Mathf.Cos(cosIndex) * 0.1f);
        cosIndex += 2f * Time.deltaTime;

        if (isOnGround)
            GetComponent<Rigidbody2D>().velocity *= 0.95f;
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (!canPickup || age < 0.5f)
            return;

        if (col.GetComponent<DroppedItem>() != null)
            if (col.GetComponent<DroppedItem>().item.material == item.material)
            {
                if (age < col.GetComponent<DroppedItem>().age)
                    return;
                if (!col.GetComponent<DroppedItem>().canPickup)
                    return;

                item.amount += col.GetComponent<DroppedItem>().item.amount;
                col.GetComponent<DroppedItem>().canPickup = false;
                col.GetComponent<DroppedItem>().Die();
                return;
            }

        if (col.GetComponent<Player>() != null)
            if (col.GetComponent<Player>().inventory.AddItem(item))
            {
                canPickup = false;
                Die();
            }
    }

    public override void Hit(float damage)
    {
        //Disabling Hit
    }
}